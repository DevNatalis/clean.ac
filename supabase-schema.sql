-- Rode isso no SQL Editor do seu projeto Supabase antes do primeiro deploy.

create table if not exists api_keys (
  id bigint generated always as identity primary key,
  name text not null,
  key_hash text not null unique,
  key_preview text not null,
  created_at timestamptz not null default now(),
  revoked boolean not null default false
);

create table if not exists scans (
  id bigint generated always as identity primary key,
  api_key_id bigint references api_keys(id),
  device_name text,
  os_info text,
  scan_id text,
  severity text not null default 'info',
  summary text,
  detections jsonb not null default '[]'::jsonb,
  received_at timestamptz not null default now()
);

create index if not exists idx_scans_received_at on scans (received_at desc);
create index if not exists idx_scans_severity on scans (severity);

-- Row Level Security: as tabelas só são acessadas pelas Netlify Functions,
-- usando a service role key (SUPABASE_SECRET_KEY), que ignora RLS.
-- Ativamos RLS sem policies para bloquear qualquer acesso direto via chave pública.
alter table api_keys enable row level security;
alter table scans enable row level security;
