using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace CLEAN
{
    public partial class Form1 : Form
    {
        private readonly List<Particle> particles = new List<Particle>();
        private readonly Random rnd = new Random();
        private Timer animationTimer;
        private const int MaxParticles = 120;

        private const string ApiUrl = "https://anticheat-vercel.vercel.app/api/scans";
        private const string ApiKey = "acsk_da7cafa19422b94d92cd3994cb49530d0f73ce45b85eebfc";

        Dictionary<string, string> nomesPersonalizadosExplorer = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosLsass = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosDps = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosDnscache = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosSysmain = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosDiagtrack = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosPcasvc = new Dictionary<string, string>();
        Dictionary<string, string> nomesPersonalizadosHistorico = new Dictionary<string, string>();

        private void AdicionarItemAoDicionario(Dictionary<string, string> dict, string key, string value)
        {
            if (!dict.ContainsKey(key))
                dict[key] = value;
        }

        private void AdicionarItemAoDicionario(Dictionary<string, string> dict, params string[] keysAndValue)
        {
            if (keysAndValue == null || keysAndValue.Length < 2) return;
            string value = keysAndValue[keysAndValue.Length - 1];
            for (int i = 0; i < keysAndValue.Length - 1; i++)
            {
                string key = keysAndValue[i];
                if (!dict.ContainsKey(key))
                    dict[key] = value;
            }
        }

        private static IEnumerable<string> SafeEnumerateFiles(string path, string pattern)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                string dir = queue.Dequeue();
                string[] files;
                try { files = Directory.GetFiles(dir, pattern); }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { continue; }
                foreach (string file in files) yield return file;
                string[] subdirs;
                try { subdirs = Directory.GetDirectories(dir); }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { continue; }
                foreach (string subdir in subdirs) queue.Enqueue(subdir);
            }
        }

        private void CarregarDicionarios()
        {
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "dControl.exe", "Defender Desativado 1");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Defender Control", "Defender Desativado 2");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "imdisk0", "ImDisk Found");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "cfg.latest", "Possivel Susano");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "favorites.cfg", "Possivel Susano");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZltWMtLL5xBgZ2M", "Possivel Susano");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "USBDeview", "Skript Found USBDeview");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "USBDeview.dll", "zetta", "zetta.exe", "Skript DLL Found USBDeview");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "9m0Ixhet", "Skript Config Found");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "AppCrash_notepad.exe", "Possiveel Gosth (notepadcrash)");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ReSetup.exe", "Project ReSetup Download");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "basic.asi", "MNL DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "nvidia.dll", "Monkeyware DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "OZIWAREPUBLIC.dll", "Oziware DLL Client [Downloaded ]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Luas_menu_free.zip", "Pack Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "parazetamol-crack", "Parazetamol Cracked [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "PozeRP.lua", "PozeRP Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "projectloader.exe", "Project Loader [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "projectloader.zip", "Project Loader [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "WeedCord.dll", "Project Weed DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "YX.FREE", "ProjectYX [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "RaffattMenuV2.dll", "RaffatMenu DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "RedEngine.dll", "Red Engine DLL Cracked [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "RenatoGarciaMenu.lua", "Renato Garcia Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "renovamenuattbeta_1.lua", "Renova Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "renovamenucripatt.lua", "Renova Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "free_fivem_cheat.dll", "ASpaceYX DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Tapatio.lua", "Tapatio Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "TapatioV24.5.lua", "Tapatio Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "TapatioV24.51.lua", "Tapatio Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "tikimenu.lua", "Tiki Menu Lua [Downloaded] Severe");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "tiki_menu.lua", "Tiki Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "vitormanu.lua", "Vitor Menu Lua [Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "VietnaMenuv2.lua", "Vietna Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "WinApi99.dll", "Weavy DLL Client [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZephyrMenu.lua", "Zephyr Menu Lua [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "sensy.bat", "Generic Bypass bat [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "NEM_DEUS_PEGA.bat", "AGeneric Bypass bat[Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "bek.bat", "AGeneric Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Bypass_Ghost_Cleaner.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "senhas.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "D3D10.dll", "d3d10, Ver em plugins o inject");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "a8a953c01e2d3139.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Win.zip.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "dollynscott.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ghost.bat_1.bat", "Generic Bypass bat Downloaded");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "SpacialoBACKUP.bat", "Generic Bypass bat Downloaded in Avast Browser");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "SconzaFps.bat", "Generic Bypass bat Downloaded in Avast Browser");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "1_tiro_by_cz.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "add_weapon_pistol.50.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ai.rar", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ai.zip", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "CR-fastRun.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "damageboost-1.5X-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "damageboost-10.0X-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "DopeAmbulance.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "DopeBmx_.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "DopeTaxi.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "fastladder-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "FAST_STRAFE_BY_OSTEN_1.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "FAST_STRAFE_BY_OSTEN_1.rar", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "handling.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "HardAmmo-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "infinitestaminafastreload.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "IR.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "maxrange.rar", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "municao_infinita.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "norecoil-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "quickenter.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "quickEnter-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "rage.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Remove_Roll.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "stamina-DDW.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "varartirobybruxo.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Varartirobyfrz.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Versao_Nova_Citizen_1_tiro_by_frz.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "WeaponVehicles.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "youngtheuz_skills.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-bulletPenetration.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-damageBoost_1.5X.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-damageBoost_10X.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-increasedRange.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-infiniteAmmo.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-stamina.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-softAim.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-softAim.rar", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZC-weaponModifier.rpf", "Suspicious download of Modified RPF");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "pedaccuracy.meta", "Suspicious download of Modified META");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "loadouts.meta", "Suspicious download of Modified META");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "loader.data", "Cutiehook Config in PC");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "password_is_eulen.rar", "Eulen RAR in PC [password_is_eulen.rar]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "abc.abc", "Gosth File Detect");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "loader.vmp.exe", "Monster EXE in PC [loader.vmp.exe]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "senha_monster.rar", "Monster RAR in PC [senha_monster.rar]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "p5m_free.ini", "Project Loader Config Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "settings.cock", "RedEngine Settings");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "public.zip", "Red Engine ZIP in PC [Public.zip]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "debug_logs", "Skript Archive Login in PC [debug_logs]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "x64a.rpf", "Possivel X64 Silent");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "imgui.ini", "Possivel ImGui Found");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Component.dll", "Project Component.dll");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "A-R.exe", "Asgard Reborn A-R.EXE");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "bypass.exe", "Bypass Generic Asgard bypass.exe");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "redengine.eu", "Red Engine Lsass");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "redengine.eu/clientarea/download?", "Red Engine [Downloaded]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "launcher.exe", "Cheat Launcher Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "17bb5a22c407a7a80c62b286495c5559", "Cheat MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "75f86416739835c01aea788a9f84c3d0aa6408da92afba04846951553d9c5458", "Cheat SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "2eb3d1653dcfdec60f3264edd96e69d0", "Gosth MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "f9da0661a7047e7391f3e143e40d046c80414d276212212e1fe147389cf7d150", "Gosth SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Logitech G hub.exe", "Gosth Fake Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "xit free fire.exe", "Gosth Fake Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "abimasetup.exe", "Gosth Fake Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "explorador.exe", "Gosth Fake Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "54d73ddfee21b41a66a6e92824e75be7", "External Cheat MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "bba1b9d8012ab877b434af4cfa775180fc1214e4c9125e90dc30cd759b45cb7c", "External Cheat SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "External.exe", "External Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "bolinha33.exe", "External Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "a76c77160df219a18da7f963967964c8", "FlyNow Cheat MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "22f9974be693ee0ed1b4c5589414fb820f2349f3a1218f8f21168415d4703fd0", "FlyNow Cheat SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "FLYNOW.exe", "FlyNow Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "a5d9377db6469eba4b045c4da8a103e0", "Lightshot Cheat MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "0f04402f89707f64efd92c7c2e8bef27b824c761a2cda30389abc2e851fb211d", "Lightshot Cheat SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "setup-lightshot.exe", "Lightshot Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "267c5db7a7268786e350399b59c8ac98", "Wexize Cheat MD5 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "aa0529ee0d005802d2ede58e1f0d3c5be59cf0f0ff39bddaf818baa86c725729", "Wexize Cheat SHA256 Detected [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Wexize Revamp.exe", "Wexize Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "7338ce0d5d7fe4785bc22d001ff50ec3", "Loader Cheat MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "cf476eee1c83dbe206c6f93b85078ff56127b977295254eaeb6bcf8f616a7401", "Loader Cheat SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "d1b4e95eb8053554ef949653158c92b1", "ZeroM$ External MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "6104f2147f9be720f3caaa19590ee466091b7f6bfc2be5d0c2129ef9f20737ea", "ZeroM$ External SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "ZeroM$ External.exe", "ZeroM$ External Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "f4faac7f466a6a7267c7ca44db6840ab", "Netflix Cheat MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "1bd9868b28ed473789fcef18c8774e4265ff5e4459db1f7e80c220c1a016ce74", "Netflix Cheat SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Netflix.exe", "Netflix Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "41eff1ce1935ab82fb912dedaf98ec69", "GregumFree MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "4468c4090a3fdff38d9a1c5ed4fe430bbfa2a1a96e29c50a116c4cd5bc891bc9", "GregumFree SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "GregumFree.exe", "GregumFree Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "2557b16cad22bfbacdac576b4ec78e64", "MarceliN External MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "cb7590ebab9bdb7379f7fb104375ff3694fda3b3f20731bb7a0f5339ebaaac84", "MarceliN External SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "44570587c2b19b1b69873e10b5b08f5c", "RockstarGames Cheat MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "6aadefd10ced6c57aaf133f119fc5a6fa3007e46f9590bebffb2892016cea572", "RockstarGames Cheat SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "RockstarGames.exe", "RockstarGames Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Menu booster.exe", "Internal Booster Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "5f1a5cd5597d1d979d952c6f229a7e12", "MysticLoader MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "e036d598ffd0e4f606757936e8d3fd83c2c3ab5880e2c30836df41dc78d049ba", "MysticLoader SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "MysticLoader.exe", "MysticLoader Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "SharkLoader.exe", "SharkLoader Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "fd956132bffc2e822ed659e2047ef4de", "ProjectLoader Setup MD5 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "8f44b0c05a2d9f7bb4fe14dab9866bca8768f7fc4e412c0331c222007aa849bd", "ProjectLoader Setup SHA256 [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "SharkLoader.exe", "SharkLoader Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "MarceliN External.exe", "MarceliN External Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "discord.exe", "Discord Cheat Name [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "Public.zip", "RedEngine ZIP / Gosth Public.zip [Explorer]");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///A", "Disk A Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///B", "Disk B Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///F", "Disk F Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///G", "Disk G Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///H", "Disk H Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///I", "Disk I Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///J", "Disk J Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///K", "Disk K Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///L", "Disk L Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///M", "Disk M Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///N", "Disk N Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///O", "Disk O Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///P", "Disk P Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///Q", "Disk Q Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///R", "Disk R Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///S", "Disk S Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///T", "Disk T Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///U", "Disk U Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///V", "Disk V Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///W", "Disk W Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///X", "Disk X Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///Y", "Disk Y Detectado");
            AdicionarItemAoDicionario(nomesPersonalizadosExplorer, "file:///Z", "Disk Z Detectado");

            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "skript.gg", "Skript.gg Lsass(1)");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "skript.gg0", "Skript.gg0 Lsass(2)");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n801", "skript OCSP 1");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "skript OCSP 2");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "20231219164333Z0t0r0J0", "skript numbers");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "s.k.r.i.p.t...g.g.", "skript unicode");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "vps-32704700.vps.ovh.ca", "Skript VPS");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "pedrin.cc", "Gosth pedrin.cc");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "three.pedrin", "Gosth three.pedrin");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "pedrin.ovh", "Gosth Pedrin.ovh");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "pedrin.cc0", "Gosth Pedrin.cc");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "pedrin.cc0!", "Gosth Pedrin.cc0");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "gosth.gg", "Gosth gosth.gg");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "api-three.pedrin.cc", "Gosth - api-three.pedrin.cc");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "ovh-01.pedrin.cc", "Gosth - ovh-01");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "131.196.198.50", "gosth IP");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "api.projectcheats.com", "Project API Acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "projectcheats.com", "Project Lsass Acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "stoppedbypass", "stopped bypass acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "stoppedbypass.com", "stopped bypass acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "api.monesy.dev", "Monesy API 1");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "monesy.dev", "Monesy API 2");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "api.idandev.xyz", "Tracks API Acessed 1");
            AdicionarItemAoDicionario(nomesPersonalizadosLsass, "idandev.xyz", "Tracks API Acessed 2");

            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2023/01/22:01:40:53!0!", "Gosth DPS");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2023/04/12:19:24:40!", "Monesy Bypass DPS");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2099/01/19:13:33:15!36ac9!", "Bypass generico");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "2023/06/04:19:28:48", "TZ Project [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2026/02/28:01:41:17", "Cheat Launcher Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2026/07/27:18:33:28", "FlyNow Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2018/06/14:13:27:46", "Lightshot Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/09/15:23:26:07", "Wexize Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2026/03/02:01:13:26", "Loader Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2026/04/11:23:58:16", "ZeroM$ External Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2026/04/25:06:16:26", "Netflix Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/11/30:06:54:44", "GregumFree Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/12/02:21:23:50", "MarceliN External Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/08/18:01:49:24", "RockstarGames Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/08/13:02:34:35", "MysticLoader Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/11/21:00:41:19", "Discord Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2023/03/11:14:46:52", "SharkLoader Generic DPS");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/03/04:22:35:08", "SharkLoader Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2024/02/15:19:05:08", "USBDeview Cheat Detected [DPS]");
            AdicionarItemAoDicionario(nomesPersonalizadosDps, "!2025/09/26:14:36:36", "ProjectLoader Setup Detected [DPS]");

            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "skript.gg", "Skript.gg Dnscache (1)");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "skript.gg0", "Skript.gg0 Dnscache (2)");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n801", "skript OCSP 1 Dnscache");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "skript OCSP 2 Dnscache");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "20231219164333Z0t0r0J0", "skript numbers Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "s.k.r.i.p.t...g.g.", "skript unicode Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "vps-32704700.vps.ovh.ca", "Skript VPS Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "pedrin.cc", "Gosth pedrin.cc Dnscache");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "three.pedrin", "Gosth three.pedrin Dnscache");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "pedrin.ovh", "Gosth Pedrin.ovh Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "pedrin.cc0", "Gosth Pedrin.cc Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "pedrin.cc0!", "Gosth Pedrin.cc0 Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "gosth.gg", "Gosth gosth.gg Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "api-three.pedrin.cc", "Gosth - api-three.pedrin.cc Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "ovh-01.pedrin.cc", "Gosth - ovh-01 Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "131.196.198.50", "gosth IP Dnscache ");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "api.projectcheats.com", "Project API Dnscache Acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "projectcheats.com", "Project Dnscache Acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "api.monesy.dev", "Monesy API Dnscache  1");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "monesy.dev", "Monesy API Dnscache  2");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "api.idandev.xyz", "Tracks API Dnscache  Acessed 1");
            AdicionarItemAoDicionario(nomesPersonalizadosDnscache, "idandev.xyz", "Tracks API Dnscache  Acessed 2");

            AdicionarItemAoDicionario(nomesPersonalizadosSysmain, "TASKKILL.EXE", "Taskkill Executed");
            AdicionarItemAoDicionario(nomesPersonalizadosSysmain, "CMD.EXE", "CMD Executed");
            AdicionarItemAoDicionario(nomesPersonalizadosSysmain, "REG.EXE", "REGEDIT Executed");
            AdicionarItemAoDicionario(nomesPersonalizadosSysmain, "DISPART.EXE", "DISKPART Executed");
            AdicionarItemAoDicionario(nomesPersonalizadosSysmain, "FSUTIL.EXE", "FSUTIL Executed");

            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "17bb5a22c407a7a80c62b286495c5559", "Cheat MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "75f86416739835c01aea788a9f84c3d0aa6408da92afba04846951553d9c5458", "Cheat SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "2eb3d1653dcfdec60f3264edd96e69d0", "Gosth MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "f9da0661a7047e7391f3e143e40d046c80414d276212212e1fe147389cf7d150", "Gosth SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "54d73ddfee21b41a66a6e92824e75be7", "External Cheat MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "bba1b9d8012ab877b434af4cfa775180fc1214e4c9125e90dc30cd759b45cb7c", "External Cheat SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "a76c77160df219a18da7f963967964c8", "FlyNow Cheat MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "22f9974be693ee0ed1b4c5589414fb820f2349f3a1218f8f21168415d4703fd0", "FlyNow Cheat SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "a5d9377db6469eba4b045c4da8a103e0", "Lightshot Cheat MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "0f04402f89707f64efd92c7c2e8bef27b824c761a2cda30389abc2e851fb211d", "Lightshot Cheat SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "267c5db7a7268786e350399b59c8ac98", "Wexize Cheat MD5 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "aa0529ee0d005802d2ede58e1f0d3c5be59cf0f0ff39bddaf818baa86c725729", "Wexize Cheat SHA256 Detected [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "7338ce0d5d7fe4785bc22d001ff50ec3", "Loader Cheat MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "cf476eee1c83dbe206c6f93b85078ff56127b977295254eaeb6bcf8f616a7401", "Loader Cheat SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "d1b4e95eb8053554ef949653158c92b1", "ZeroM$ External MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "6104f2147f9be720f3caaa19590ee466091b7f6bfc2be5d0c2129ef9f20737ea", "ZeroM$ External SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "f4faac7f466a6a7267c7ca44db6840ab", "Netflix Cheat MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "1bd9868b28ed473789fcef18c8774e4265ff5e4459db1f7e80c220c1a016ce74", "Netflix Cheat SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "41eff1ce1935ab82fb912dedaf98ec69", "GregumFree MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "4468c4090a3fdff38d9a1c5ed4fe430bbfa2a1a96e29c50a116c4cd5bc891bc9", "GregumFree SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "2557b16cad22bfbacdac576b4ec78e64", "MarceliN External MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "cb7590ebab9bdb7379f7fb104375ff3694fda3b3f20731bb7a0f5339ebaaac84", "MarceliN External SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "44570587c2b19b1b69873e10b5b08f5c", "RockstarGames Cheat MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "6aadefd10ced6c57aaf133f119fc5a6fa3007e46f9590bebffb2892016cea572", "RockstarGames Cheat SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "5f1a5cd5597d1d979d952c6f229a7e12", "MysticLoader MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "e036d598ffd0e4f606757936e8d3fd83c2c3ab5880e2c30836df41dc78d049ba", "MysticLoader SHA256 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "fd956132bffc2e822ed659e2047ef4de", "ProjectLoader Setup MD5 [Diagtrack]");
            AdicionarItemAoDicionario(nomesPersonalizadosDiagtrack, "8f44b0c05a2d9f7bb4fe14dab9866bca8768f7fc4e412c0331c222007aa849bd", "ProjectLoader Setup SHA256 [Diagtrack]");

            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x4c7d000", "Cheat Launcher PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x7bd000", "FlyNow Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x3963000", "Lightshot Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x2d8000", "Wexize Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x1b8f000", "Loader Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x45d000", "ZeroM$ External PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x2ad7000", "Netflix Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x79b000", "GregumFree PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x792000", "MarceliN External PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x28f000", "RockstarGames Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x476000", "External 4M PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x2f8000", "Internal Booster PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x1ce5000", "MysticLoader PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x133d000", "Discord Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x164000", "Generic Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x2abe000", "SharkLoader PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x2b1000", "USBDeview Cheat PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0x45000", "ProjectLoader Setup PcaSvc Detected");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "17bb5a22c407a7a80c62b286495c5559", "Cheat MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "75f86416739835c01aea788a9f84c3d0aa6408da92afba04846951553d9c5458", "Cheat SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "2eb3d1653dcfdec60f3264edd96e69d0", "Gosth MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "f9da0661a7047e7391f3e143e40d046c80414d276212212e1fe147389cf7d150", "Gosth SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "54d73ddfee21b41a66a6e92824e75be7", "External Cheat MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "bba1b9d8012ab877b434af4cfa775180fc1214e4c9125e90dc30cd759b45cb7c", "External Cheat SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "a76c77160df219a18da7f963967964c8", "FlyNow Cheat MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "22f9974be693ee0ed1b4c5589414fb820f2349f3a1218f8f21168415d4703fd0", "FlyNow Cheat SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "a5d9377db6469eba4b045c4da8a103e0", "Lightshot Cheat MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "0f04402f89707f64efd92c7c2e8bef27b824c761a2cda30389abc2e851fb211d", "Lightshot Cheat SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "267c5db7a7268786e350399b59c8ac98", "Wexize Cheat MD5 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "aa0529ee0d005802d2ede58e1f0d3c5be59cf0f0ff39bddaf818baa86c725729", "Wexize Cheat SHA256 Detected [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "7338ce0d5d7fe4785bc22d001ff50ec3", "Loader Cheat MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "cf476eee1c83dbe206c6f93b85078ff56127b977295254eaeb6bcf8f616a7401", "Loader Cheat SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "d1b4e95eb8053554ef949653158c92b1", "ZeroM$ External MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "6104f2147f9be720f3caaa19590ee466091b7f6bfc2be5d0c2129ef9f20737ea", "ZeroM$ External SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "f4faac7f466a6a7267c7ca44db6840ab", "Netflix Cheat MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "1bd9868b28ed473789fcef18c8774e4265ff5e4459db1f7e80c220c1a016ce74", "Netflix Cheat SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "41eff1ce1935ab82fb912dedaf98ec69", "GregumFree MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "4468c4090a3fdff38d9a1c5ed4fe430bbfa2a1a96e29c50a116c4cd5bc891bc9", "GregumFree SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "2557b16cad22bfbacdac576b4ec78e64", "MarceliN External MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "cb7590ebab9bdb7379f7fb104375ff3694fda3b3f20731bb7a0f5339ebaaac84", "MarceliN External SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "44570587c2b19b1b69873e10b5b08f5c", "RockstarGames Cheat MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "6aadefd10ced6c57aaf133f119fc5a6fa3007e46f9590bebffb2892016cea572", "RockstarGames Cheat SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "5f1a5cd5597d1d979d952c6f229a7e12", "MysticLoader MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "e036d598ffd0e4f606757936e8d3fd83c2c3ab5880e2c30836df41dc78d049ba", "MysticLoader SHA256 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "fd956132bffc2e822ed659e2047ef4de", "ProjectLoader Setup MD5 [PcaSvc]");
            AdicionarItemAoDicionario(nomesPersonalizadosPcasvc, "8f44b0c05a2d9f7bb4fe14dab9866bca8768f7fc4e412c0331c222007aa849bd", "ProjectLoader Setup SHA256 [PcaSvc]");

            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "stoppedbypass.com/products", "stopped bypass acessed");
            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "gosth.gg", "Gosth Site acessado");
            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "skript.gg/favicon.png", "Skript Favicon Baixado");
            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "skript.gg", "Skript Site Acessado");
            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "cdn.gosth.ltd", "Gosth Instalado");
            AdicionarItemAoDicionario(nomesPersonalizadosHistorico, "projectdow.com/data/bypass/bypass", "Project Bypass Acessed");
        }

        public Form1()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            this.Load += Form1_Load;
            this.Resize += Form1_Resize;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CarregarDicionarios();

            particles.Clear();
            for (int i = 0; i < MaxParticles; i++)
                particles.Add(CreateParticle());

            animationTimer = new Timer();
            animationTimer.Interval = 16;
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            try { this.guna2ProgressBar1.Visible = false; } catch { }
            try { this.percentLabel.Visible = false; } catch { }
            try { this.guna2Button1.Visible = true; } catch { }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            this.guna2Button1.Visible = false;
            this.guna2ProgressBar1.Value = 0;
            this.guna2ProgressBar1.Visible = true;
            this.percentLabel.Text = "0%";
            try { this.label1.Text = "0%"; } catch { }
            this.percentLabel.Visible = true;

            EnviarLogScanAsync();
        }

        private void UpdateProgress(int percent)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((Action)(() => UpdateProgress(percent)));
                return;
            }
            if (this.guna2ProgressBar1.Value < percent)
                this.guna2ProgressBar1.Value = percent;
            this.percentLabel.Text = percent + "%";
            try { this.label1.Text = percent + "%"; } catch { }
        }

        private async void EnviarLogScanAsync()
        {
            try
            {
                var detections = await Task.Run(() => RealizarScan());

                UpdateProgress(100);

                string severity = detections.Count > 0 ? "high" : "info";
                string summary = detections.Count > 0
                    ? detections.Count + " ameaça(s) detectada(s)"
                    : "Varredura concluída sem ameaças";

                string json = "{" +
                    "\"device\":\"" + JsonEscape(Environment.MachineName) + "\"," +
                    "\"os\":\"" + JsonEscape(Environment.OSVersion.ToString()) + "\"," +
                    "\"scanId\":\"" + Guid.NewGuid().ToString() + "\"," +
                    "\"severity\":\"" + severity + "\"," +
                    "\"summary\":\"" + JsonEscape(summary) + "\"," +
                    "\"detections\":[" + string.Join(",", detections) + "]" +
                "}";

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(8);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Key", ApiKey);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await client.PostAsync(ApiUrl, content);
                }
            }
            catch { }

            await Task.Delay(500);
            this.Close();
        }

        private List<string> RealizarScan()
        {
            var detections = new List<string>();

            UpdateProgress(5);
            ScanFileSystem(detections);
            UpdateProgress(15);

            ScanPrefetchSysmain(detections);
            UpdateProgress(25);

            ScanDnsCache(detections);
            UpdateProgress(35);

            ScanLsassMemory(detections);
            UpdateProgress(48);

            ScanDPS(detections);
            UpdateProgress(58);

            ScanDiagtrack(detections);
            UpdateProgress(68);

            ScanPcaSvc(detections);
            UpdateProgress(78);

            ScanBrowserHistory(detections);
            UpdateProgress(88);

            ScanProcesses(detections);
            UpdateProgress(98);

            return detections;
        }

        private void ScanFileSystem(List<string> detections)
        {
            try
            {
                string[] searchPaths = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                    Path.GetTempPath(),
                };

                string[] extensions = { "*.exe", "*.dll", "*.asi", "*.lua", "*.bat", "*.rar", "*.zip", "*.rpf", "*.meta", "*.ini", "*.cfg", "*.data", "*.cock" };

                foreach (string basePath in searchPaths)
                {
                    if (!Directory.Exists(basePath)) continue;

                    foreach (string ext in extensions)
                    {
                        try
                        {
                            foreach (string filePath in SafeEnumerateFiles(basePath, ext))
                            {
                                string fileName = Path.GetFileName(filePath);

                                if (nomesPersonalizadosExplorer.TryGetValue(fileName, out string desc))
                                {
                                    detections.Add(DetectionJson(desc, "file_found", filePath, 0.95));
                                    continue;
                                }

                                if (fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                {
                                    try
                                    {
                                        byte[] fileBytes = File.ReadAllBytes(filePath);
                                        string md5 = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(fileBytes)).Replace("-", "").ToLower();
                                        string sha256 = BitConverter.ToString(new SHA256CryptoServiceProvider().ComputeHash(fileBytes)).Replace("-", "").ToLower();

                                        if (nomesPersonalizadosExplorer.TryGetValue(md5, out string md5Desc))
                                        {
                                            detections.Add(DetectionJson(md5Desc, "hash_match", filePath, 0.99));
                                        }
                                        else if (nomesPersonalizadosExplorer.TryGetValue(sha256, out string sha256Desc))
                                        {
                                            detections.Add(DetectionJson(sha256Desc, "hash_match", filePath, 0.99));
                                        }
                                        else if (fileBytes.Length == 28191232)
                                        {
                                            detections.Add(DetectionJson("Gosth Cheat (size match 26.89MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 19459028)
                                        {
                                            detections.Add(DetectionJson("External Cheat (size match 18.56MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 8094720)
                                        {
                                            detections.Add(DetectionJson("FlyNow Cheat (size match 7.72MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 31717376)
                                        {
                                            detections.Add(DetectionJson("Lightshot Cheat (size match 30.25MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 2951168)
                                        {
                                            detections.Add(DetectionJson("Wexize Cheat (size match 2.81MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 16384000)
                                        {
                                            detections.Add(DetectionJson("Loader Cheat (size match 15.63MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 4284928)
                                        {
                                            detections.Add(DetectionJson("ZeroM$ External (size match 4.09MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 25722880)
                                        {
                                            detections.Add(DetectionJson("Netflix Cheat (size match 24.53MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 4379136)
                                        {
                                            detections.Add(DetectionJson("GregumFree (size match 4.18MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 4359168)
                                        {
                                            detections.Add(DetectionJson("MarceliN External (size match 4.16MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 34179072)
                                        {
                                            detections.Add(DetectionJson("RockstarGames Cheat (size match 32.60MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 4650496)
                                        {
                                            detections.Add(DetectionJson("External 4M Cheat (size match 4.43MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 3082752)
                                        {
                                            detections.Add(DetectionJson("Internal Booster Cheat (size match 2.94MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 17646592)
                                        {
                                            detections.Add(DetectionJson("MysticLoader (size match 16.83MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 11216400)
                                        {
                                            detections.Add(DetectionJson("Discord Cheat (size match 10.70MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 10818560)
                                        {
                                            detections.Add(DetectionJson("Internal Cheat (size match 10.32MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 24797184)
                                        {
                                            detections.Add(DetectionJson("SharkLoader (size match 23.65MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 2779136)
                                        {
                                            detections.Add(DetectionJson("USBDeview Cheat (size match 2.65MB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 256512)
                                        {
                                            detections.Add(DetectionJson("ProjectLoader Setup (size match 250KB)", "size_match", filePath, 0.80));
                                        }
                                        else if (fileBytes.Length == 43145728)
                                        {
                                            detections.Add(DetectionJson("Cheat Launcher (size match 43MB)", "size_match", filePath, 0.80));
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void ScanPrefetchSysmain(List<string> detections)
        {
            try
            {
                string prefetchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                if (!Directory.Exists(prefetchPath)) return;

                foreach (string key in nomesPersonalizadosSysmain.Keys)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(prefetchPath))
                        {
                            string upperFile = Path.GetFileName(file).ToUpperInvariant();
                            if (upperFile.Contains(key.ToUpperInvariant()))
                            {
                                detections.Add(DetectionJson(nomesPersonalizadosSysmain[key], "prefetch", file, 0.70));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanDnsCache(List<string> detections)
        {
            try
            {
                var psi = new ProcessStartInfo("ipconfig", "/displaydns")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    foreach (string key in nomesPersonalizadosDnscache.Keys)
                    {
                        if (output.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            detections.Add(DetectionJson(nomesPersonalizadosDnscache[key], "dns_cache", null, 0.85));
                        }
                    }
                }
            }
            catch { }
        }

        private void ScanLsassMemory(List<string> detections)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", "/c \"tasklist /m /fi \"\"imagename eq lsass.exe\"\"\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    foreach (string key in nomesPersonalizadosLsass.Keys)
                    {
                        if (output.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            detections.Add(DetectionJson(nomesPersonalizadosLsass[key], "lsass_module", null, 0.90));
                        }
                    }
                }

                var psi2 = new ProcessStartInfo("cmd.exe", "/c \"tasklist /m /fi \"\"imagename eq explorer.exe\"\"\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc2 = Process.Start(psi2))
                {
                    string output2 = proc2.StandardOutput.ReadToEnd();
                    proc2.WaitForExit();

                    foreach (string key in nomesPersonalizadosLsass.Keys)
                    {
                        if (output2.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            detections.Add(DetectionJson(nomesPersonalizadosLsass[key] + " [explorer]", "explorer_module", null, 0.85));
                        }
                    }
                }
            }
            catch { }
        }

        private void ScanDPS(List<string> detections)
        {
            try
            {
                string prefetchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                if (!Directory.Exists(prefetchPath)) return;

                foreach (string file in Directory.GetFiles(prefetchPath, "*.pf"))
                {
                    try
                    {
                        string content = File.ReadAllText(file, Encoding.ASCII);

                        foreach (string key in nomesPersonalizadosDps.Keys)
                        {
                            if (content.Contains(key))
                            {
                                detections.Add(DetectionJson(nomesPersonalizadosDps[key], "dps_prefetch", file, 0.85));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanDiagtrack(List<string> detections)
        {
            try
            {
                string diagtrackPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "DiagTrack");
                if (Directory.Exists(diagtrackPath))
                {
                    foreach (string file in Directory.GetFiles(diagtrackPath))
                    {
                        try
                        {
                            string content = File.ReadAllText(file, Encoding.UTF8);

                            foreach (string key in nomesPersonalizadosDiagtrack.Keys)
                            {
                                if (content.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    detections.Add(DetectionJson(nomesPersonalizadosDiagtrack[key], "diagtrack", file, 0.80));
                                }
                            }
                        }
                        catch { }
                    }
                }

                string autoLoggerPath = Path.Combine(@"C:\ProgramData", "Microsoft", "DiagTrack", "DiagTrackListener.etl");
                if (File.Exists(autoLoggerPath))
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(autoLoggerPath);
                        string hex = BitConverter.ToString(bytes).Replace("-", "");

                        foreach (string key in nomesPersonalizadosDiagtrack.Keys)
                        {
                            if (hex.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                detections.Add(DetectionJson(nomesPersonalizadosDiagtrack[key], "diagtrack_etl", autoLoggerPath, 0.85));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanPcaSvc(List<string> detections)
        {
            try
            {
                string pcaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "AppCompat", "Programs");
                if (Directory.Exists(pcaPath))
                {
                    foreach (string file in Directory.GetFiles(pcaPath, "*.ini"))
                    {
                        try
                        {
                            string content = File.ReadAllText(file, Encoding.UTF8);

                            foreach (string key in nomesPersonalizadosPcasvc.Keys)
                            {
                                if (content.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    detections.Add(DetectionJson(nomesPersonalizadosPcasvc[key], "pcasvc", file, 0.80));
                                }
                            }
                        }
                        catch { }
                    }
                }

                string amCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "AppCompat", "Programs", "Amcache.hve");
                if (File.Exists(amCachePath))
                {
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(amCachePath);
                        string hex = BitConverter.ToString(bytes).Replace("-", "");

                        foreach (string key in nomesPersonalizadosPcasvc.Keys)
                        {
                            if (hex.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                detections.Add(DetectionJson(nomesPersonalizadosPcasvc[key], "amcache", amCachePath, 0.85));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanBrowserHistory(List<string> detections)
        {
            try
            {
                string[] historyFiles = {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "History"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "History")
                };

                foreach (string historyFile in historyFiles)
                {
                    if (!File.Exists(historyFile)) continue;

                    try
                    {
                        string tempCopy = Path.GetTempFileName();
                        File.Copy(historyFile, tempCopy, true);

                        byte[] bytes = File.ReadAllBytes(tempCopy);
                        string content = Encoding.UTF8.GetString(bytes);

                        foreach (string key in nomesPersonalizadosHistorico.Keys)
                        {
                            if (content.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                detections.Add(DetectionJson(nomesPersonalizadosHistorico[key], "browser_history", null, 0.90));
                            }
                        }
                        File.Delete(tempCopy);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void ScanProcesses(List<string> detections)
        {
            try
            {
                Process[] allProcesses = Process.GetProcesses();
                string[] suspiciousProcessNames = { "launcher", "bypass", "inject", "cheat", "mod menu", "x64a" };

                foreach (var proc in allProcesses)
                {
                    try
                    {
                        string procName = proc.ProcessName.ToLowerInvariant();

                        foreach (string susp in suspiciousProcessNames)
                        {
                            if (procName.Contains(susp))
                            {
                                detections.Add(DetectionJson("Suspicious Process Running: " + procName, "process", null, 0.60));
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static string DetectionJson(string name, string type, string path, double confidence)
        {
            return "{" +
                "\"name\":\"" + JsonEscape(name) + "\"," +
                "\"type\":\"" + JsonEscape(type) + "\"," +
                "\"path\":" + (string.IsNullOrEmpty(path) ? "null" : "\"" + JsonEscape(path) + "\"") + "," +
                "\"confidence\":" + confidence.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            "}";
        }

        private static string JsonEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ') sb.Append("\\u" + ((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i].Position.X > this.ClientSize.Width || particles[i].Position.Y > this.ClientSize.Height)
                    particles[i] = CreateParticle();
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            UpdateParticles();
            this.Invalidate();
        }

        private void UpdateParticles()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                p.Position = new PointF(p.Position.X + p.Velocity.X, p.Position.Y + p.Velocity.Y);
                p.Life -= 1;

                if (p.Life <= 0 || p.Position.X < -50 || p.Position.X > this.ClientSize.Width + 50 || p.Position.Y < -50 || p.Position.Y > this.ClientSize.Height + 50)
                {
                    particles[i] = CreateParticle();
                }
                else
                {
                    particles[i] = p;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var brush = new LinearGradientBrush(this.ClientRectangle, Color.FromArgb(20, 10, 10, 30), Color.Black, 90f))
            {
                g.FillRectangle(brush, this.ClientRectangle);
            }

            for (int i = 0; i < particles.Count; i++)
            {
                var p = particles[i];
                int alpha = (int)(255 * Math.Max(0.0f, Math.Min(1.0f, p.Life / (float)p.InitialLife)));
                Color c = Color.FromArgb(alpha, p.Color);
                using (var b = new SolidBrush(c))
                {
                    float size = p.Size;
                    g.FillEllipse(b, p.Position.X - size / 2f, p.Position.Y - size / 2f, size, size);
                }
            }

            for (int i = 0; i < particles.Count; i++)
            {
                for (int j = i + 1; j < particles.Count; j++)
                {
                    var a = particles[i];
                    var bP = particles[j];
                    float dx = a.Position.X - bP.Position.X;
                    float dy = a.Position.Y - bP.Position.Y;
                    float dist2 = dx * dx + dy * dy;
                    if (dist2 < 9000)
                    {
                        float dist = (float)Math.Sqrt(dist2);
                        int alpha = (int)(120 * (1.0f - dist / 100.0f));
                        if (alpha > 0)
                        {
                            using (var pen = new Pen(Color.FromArgb(alpha, 200, 180, 255), 1f))
                            {
                                g.DrawLine(pen, a.Position, bP.Position);
                            }
                        }
                    }
                }
            }
        }

        private Particle CreateParticle()
        {
            float x = (float)rnd.NextDouble() * this.ClientSize.Width;
            float y = (float)rnd.NextDouble() * this.ClientSize.Height;
            float vx = (float)(rnd.NextDouble() * 1.2 - 0.6);
            float vy = (float)(rnd.NextDouble() * 1.2 - 0.6);
            float size = (float)(rnd.NextDouble() * 6.0 + 1.5);
            int life = rnd.Next(80, 240);
            Color color = Color.FromArgb(255, 180 + rnd.Next(75), 160 + rnd.Next(95), 255);
            return new Particle
            {
                Position = new PointF(x, y),
                Velocity = new PointF(vx, vy),
                Size = size,
                Life = life,
                InitialLife = life,
                Color = color
            };
        }

        private struct Particle
        {
            public PointF Position;
            public PointF Velocity;
            public float Size;
            public int Life;
            public int InitialLife;
            public Color Color;
        }

        private void guna2ProgressBar1_ValueChanged(object sender, EventArgs e)
        {
        }
    }
}
