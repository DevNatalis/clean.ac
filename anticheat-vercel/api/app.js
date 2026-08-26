const express = require('express');
const cors = require('cors');
const cookie = require('cookie');
const crypto = require('crypto');
const { createClient } = require('@supabase/supabase-js');

const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'troque-esta-senha';
const SESSION_SECRET = process.env.SESSION_SECRET || 'troque-por-uma-string-aleatoria-longa';

const supabase = createClient(
  process.env.SUPABASE_URL,
  process.env.SUPABASE_SECRET_KEY // service role key: só usada aqui, no servidor
);

const app = express();
const router = express.Router();

app.use(cors());
app.use(express.json({ limit: '2mb' }));

// ---------- Sessão simples via cookie assinado (sem estado no servidor) ----------
function signSession(payload) {
  const data = Buffer.from(JSON.stringify(payload)).toString('base64url');
  const sig = crypto.createHmac('sha256', SESSION_SECRET).update(data).digest('base64url');
  return `${data}.${sig}`;
}
function verifySession(token) {
  if (!token) return null;
  const [data, sig] = token.split('.');
  if (!data || !sig) return null;
  const expected = crypto.createHmac('sha256', SESSION_SECRET).update(data).digest('base64url');
  if (sig !== expected) return null;
  try {
    return JSON.parse(Buffer.from(data, 'base64url').toString());
  } catch {
    return null;
  }
}
function getSession(req) {
  const cookies = cookie.parse(req.headers.cookie || '');
  return verifySession(cookies.acsession);
}
function setSessionCookie(res, payload) {
  const token = signSession(payload);
  res.setHeader('Set-Cookie', cookie.serialize('acsession', token, {
    httpOnly: true,
    secure: true,
    sameSite: 'lax',
    path: '/',
    maxAge: 12 * 60 * 60,
  }));
}
function clearSessionCookie(res) {
  res.setHeader('Set-Cookie', cookie.serialize('acsession', '', {
    httpOnly: true, secure: true, sameSite: 'lax', path: '/', maxAge: 0,
  }));
}

function requireAdmin(req, res, next) {
  const session = getSession(req);
  if (session && session.isAdmin) return next();
  return res.status(401).json({ error: 'not_authenticated' });
}

// ---------- Helpers de API key ----------
function hashKey(rawKey) {
  return crypto.createHash('sha256').update(rawKey).digest('hex');
}
function generateApiKey() {
  return 'acsk_' + crypto.randomBytes(24).toString('hex');
}
const SEVERITY_ORDER = ['info', 'low', 'medium', 'high', 'critical'];
function normalizeSeverity(sev) {
  const s = String(sev || 'info').toLowerCase();
  return SEVERITY_ORDER.includes(s) ? s : 'info';
}

async function requireApiKey(req, res, next) {
  const rawKey = req.header('X-API-Key');
  if (!rawKey) return res.status(401).json({ error: 'missing_api_key' });

  const hash = hashKey(rawKey);
  const { data, error } = await supabase
    .from('api_keys')
    .select('*')
    .eq('key_hash', hash)
    .eq('revoked', false)
    .maybeSingle();

  if (error || !data) return res.status(403).json({ error: 'invalid_api_key' });

  req.apiKeyRow = data;
  next();
}

// ---------- Auth do dashboard ----------
router.post('/auth/login', (req, res) => {
  const { password } = req.body || {};
  if (password && password === ADMIN_PASSWORD) {
    setSessionCookie(res, { isAdmin: true });
    return res.json({ ok: true });
  }
  return res.status(401).json({ error: 'senha_invalida' });
});

router.post('/auth/logout', (req, res) => {
  clearSessionCookie(res);
  res.json({ ok: true });
});

router.get('/auth/me', (req, res) => {
  const session = getSession(req);
  res.json({ isAdmin: !!(session && session.isAdmin) });
});

// ---------- Gestão de API keys ----------
router.get('/keys', requireAdmin, async (req, res) => {
  const { data, error } = await supabase
    .from('api_keys')
    .select('id, name, key_preview, created_at, revoked')
    .order('created_at', { ascending: false });
  if (error) return res.status(500).json({ error: error.message });
  res.json(data);
});

router.post('/keys', requireAdmin, async (req, res) => {
  const { name } = req.body || {};
  if (!name || !name.trim()) return res.status(400).json({ error: 'nome_obrigatorio' });

  const rawKey = generateApiKey();
  const hash = hashKey(rawKey);
  const preview = rawKey.slice(0, 10) + '...' + rawKey.slice(-4);

  const { data, error } = await supabase
    .from('api_keys')
    .insert({ name: name.trim(), key_hash: hash, key_preview: preview })
    .select('id')
    .single();

  if (error) return res.status(500).json({ error: error.message });

  res.json({ id: data.id, name: name.trim(), key: rawKey, key_preview: preview });
});

router.delete('/keys/:id', requireAdmin, async (req, res) => {
  const { error } = await supabase
    .from('api_keys')
    .update({ revoked: true })
    .eq('id', req.params.id);
  if (error) return res.status(500).json({ error: error.message });
  res.json({ ok: true });
});

// ---------- Ingestão de scans (usado pelo app C#/C++) ----------
router.post('/scans', requireApiKey, async (req, res) => {
  const { device, os, scanId, severity, summary, detections } = req.body || {};

  const { data, error } = await supabase
    .from('scans')
    .insert({
      api_key_id: req.apiKeyRow.id,
      device_name: String(device || 'desconhecido').slice(0, 200),
      os_info: String(os || '').slice(0, 200),
      scan_id: String(scanId || '').slice(0, 200),
      severity: normalizeSeverity(severity),
      summary: String(summary || '').slice(0, 500),
      detections: Array.isArray(detections) ? detections : [],
    })
    .select('id')
    .single();

  if (error) return res.status(500).json({ error: error.message });
  res.status(201).json({ ok: true, id: data.id });
});

// ---------- Leitura de scans (dashboard) ----------
router.get('/scans', requireAdmin, async (req, res) => {
  const { severity, search, limit } = req.query;

  let query = supabase
    .from('scans')
    .select('*, api_keys(name)')
    .order('received_at', { ascending: false })
    .limit(Math.min(parseInt(limit) || 100, 500));

  if (severity && SEVERITY_ORDER.includes(severity)) {
    query = query.eq('severity', severity);
  }
  if (search) {
    query = query.or(
      `device_name.ilike.%${search}%,summary.ilike.%${search}%`
    );
  }

  const { data, error } = await query;
  if (error) return res.status(500).json({ error: error.message });

  const shaped = data.map((r) => ({
    ...r,
    key_name: r.api_keys ? r.api_keys.name : null,
  }));
  res.json(shaped);
});

router.get('/scans/stats', requireAdmin, async (req, res) => {
  const { count: total } = await supabase
    .from('scans')
    .select('*', { count: 'exact', head: true });

  const { data, error } = await supabase.from('scans').select('severity');
  if (error) return res.status(500).json({ error: error.message });

  const bySeverityMap = {};
  for (const row of data) {
    bySeverityMap[row.severity] = (bySeverityMap[row.severity] || 0) + 1;
  }
  const bySeverity = Object.entries(bySeverityMap).map(([severity, count]) => ({ severity, count }));

  res.json({ total: total || 0, bySeverity });
});

router.delete('/scans/:id', requireAdmin, async (req, res) => {
  const { error } = await supabase
    .from('scans')
    .delete()
    .eq('id', req.params.id);
  if (error) return res.status(500).json({ error: error.message });
  res.json({ ok: true });
});

router.delete('/scans', requireAdmin, async (req, res) => {
  const { error } = await supabase
    .from('scans')
    .delete()
    .neq('id', 0);
  if (error) return res.status(500).json({ error: error.message });
  res.json({ ok: true });
});

// A Vercel entrega o path completo (ex: /api/scans), então montamos em /api e também na raiz
app.use('/api', router);
app.use('/', router);

// Handler no formato que a Vercel espera: (req, res)
module.exports = (req, res) => app(req, res);
