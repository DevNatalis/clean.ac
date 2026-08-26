// ScanReporter.cs
// Exemplo de integração do seu app scanner (C#) com o dashboard SENTINEL.
// Requer: System.Net.Http.Json (built-in a partir do .NET 5+)

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AntiCheatScanner
{
    public class Detection
    {
        public string Name { get; set; }        // ex: "cheatengine.dll"
        public string Type { get; set; }         // ex: "injected_module", "hooked_function", "known_signature"
        public string Path { get; set; }         // caminho do arquivo/processo, se aplicável
        public double? Confidence { get; set; }  // 0.0 a 1.0
    }

    public class ScanReport
    {
        public string Device { get; set; }
        public string Os { get; set; }
        public string ScanId { get; set; }
        public string Severity { get; set; } // "info" | "low" | "medium" | "high" | "critical"
        public string Summary { get; set; }
        public List<Detection> Detections { get; set; } = new List<Detection>();
    }

    public class ScanReporter
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        // baseUrl exemplo: "https://seu-servidor.com" ou "http://localhost:3000" em dev
        public ScanReporter(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKey = apiKey;
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> SendReportAsync(ScanReport report)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/scans");
            request.Headers.Add("X-API-Key", _apiKey);
            request.Content = JsonContent.Create(report);

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Falha ao enviar scan: {(int)response.StatusCode} - {body}");
                return false;
            }

            return true;
        }
    }

    // ---------------- Exemplo de uso ----------------
    public static class Program
    {
        public static async Task Main()
        {
            var reporter = new ScanReporter(
                baseUrl: "http://localhost:3000",           // troque pela URL do seu servidor
                apiKey: "acsk_SUA_CHAVE_GERADA_NO_PAINEL"   // gerada na aba "Chaves de API"
            );

            var report = new ScanReport
            {
                Device = Environment.MachineName,
                Os = Environment.OSVersion.ToString(),
                ScanId = Guid.NewGuid().ToString(),
                Severity = "high",
                Summary = "2 módulos suspeitos detectados em processo do jogo",
                Detections = new List<Detection>
                {
                    new Detection
                    {
                        Name = "cheatengine.dll",
                        Type = "injected_module",
                        Path = @"C:\Users\Player\AppData\Local\Temp\cheatengine.dll",
                        Confidence = 0.94
                    },
                    new Detection
                    {
                        Name = "aimbot_hook",
                        Type = "hooked_function",
                        Path = null,
                        Confidence = 0.81
                    }
                }
            };

            bool ok = await reporter.SendReportAsync(report);
            Console.WriteLine(ok ? "Relatório enviado com sucesso." : "Erro ao enviar relatório.");
        }
    }
}
