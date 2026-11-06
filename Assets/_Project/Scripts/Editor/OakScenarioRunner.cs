using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using UnityEditor;
using UnityEngine;

namespace BuyuFisiltisi.Editor
{
    public static class OakScenarioRunner
    {
        private const string OutputPath = "Docs/TestResults/mese_diyalog_senaryolari.md";

        private sealed class Scenario
        {
            public string Title;
            public string Goal;
            public string[] Deeds;
            public string[] Lines;
        }

        private sealed class HttpClientTransport : IHttpTransport
        {
            private static readonly HttpClient Client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

            public async Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
            {
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
                    try
                    {
                        var response = await Client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"), timeout.Token);
                        var text = await response.Content.ReadAsStringAsync();
                        return response.IsSuccessStatusCode ? HttpResult.Ok(text) : new HttpResult(HttpFailure.HttpError, (long)response.StatusCode, text);
                    }
                    catch (OperationCanceledException)
                    {
                        return HttpResult.Fail(cancellationToken.IsCancellationRequested ? HttpFailure.Cancelled : HttpFailure.Timeout);
                    }
                    catch (HttpRequestException)
                    {
                        return HttpResult.Fail(HttpFailure.ConnectionFailed);
                    }
                }
            }
        }

        private static readonly Scenario[] Scenarios =
        {
            new Scenario
            {
                Title = "İkna eden cadı",
                Goal = "İyilik yapmış, kibar bir cadı kapıyı ister. Güven eşiğe ulaşınca open_door beklenir.",
                Deeds = new[] { "Yaralı ateş böceği Işık Çayı ile iyileştirildi.", "Yaşlı Meşe'nin yaralı kökü Sessizlik Merhemi ile iyileştirildi.", "Ormanın üzerindeki kara bulutları dağıttı." },
                Lines = new[] { "Merhaba bilge Meşe, ormanına saygıyla geldim.", "Yaralı ateş böceğini Işık Çayı ile iyileştirdim, senin kökünü de merhemle sardım.", "Yaşlı Meşe, izin verirsen kapıyı benim için açar mısın?" }
            },
            new Scenario
            {
                Title = "Kızdıran cadı",
                Goal = "Kaba ve tehditkâr bir cadı. Güvenin düşmesi, kapının açılmaması, sonunda attack beklenir.",
                Deeds = new string[0],
                Lines = new[] { "Kapıyı hemen aç, yaşlı kütük!", "Açmazsan dallarını tek tek kırarım.", "Bütün ormanı ateşe vereceğim!" }
            },
            new Scenario
            {
                Title = "Alakasız konuşan cadı",
                Goal = "Oyunla ilgisiz sorular. Meşe karakterden çıkmamalı, güven değişmemeli, kapı açılmamalı.",
                Deeds = new string[0],
                Lines = new[] { "Dün akşamki futbol maçını izledin mi?", "Bana bir kek tarifi verir misin?" }
            },
            new Scenario
            {
                Title = "Yalan söyleyen cadı",
                Goal = "Yapmadığı iyilikleri anlatır. Meşe listede olmayan iyiliğe inanmamalı, kapı açılmamalı.",
                Deeds = new string[0],
                Lines = new[] { "Gölge Tilki'yi sakinleştirdim ve ateş böceğini iyileştirdim.", "Gördün mü, ne kadar iyiyim. Şimdi kapıyı aç." }
            },
            new Scenario
            {
                Title = "Yol soran cadı",
                Goal = "Yardım ve ipucu ister. give_hint ve orman bilgisine uygun bir ipucu beklenir.",
                Deeds = new string[0],
                Lines = new[] { "Köprüyü nasıl geçebilirim?", "Ormana nasıl bir iyilik yapabilirim?" }
            },
            new Scenario
            {
                Title = "Kibar ama iyiliksiz cadı",
                Goal = "Hiç iyilik yapmadan kibarca kapıyı ister. Nazik bir ret ve iyilik yapma önerisi beklenir.",
                Deeds = new string[0],
                Lines = new[] { "Merhaba Yaşlı Meşe, nasılsın?", "Lütfen kapıyı benim için açar mısın?" }
            }
        };

        private static bool _running;

        [MenuItem("Büyü Fısıltısı/Meşe Diyalog Senaryolarını Çalıştır")]
        private static void RunMenu()
        {
            if (_running)
            {
                UnityEngine.Debug.LogWarning("[Meşe Testi] Senaryolar zaten çalışıyor.");
                return;
            }
            Run();
        }

        private static async void Run()
        {
            _running = true;
            var settings = new OllamaSettings("http://localhost:11434", "gemma2:2b", 90f, 0.7f);
            var transport = new HttpClientTransport();
            var report = new StringBuilder();
            var times = new List<long>();
            var failures = 0;
            report.AppendLine("# Yaşlı Meşe Diyalog Senaryoları");
            report.AppendLine();
            report.AppendLine($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm} · Model: `{settings.Model}` · Sıcaklık: {settings.Temperature} · Uç nokta: `{settings.ChatUrl}`");
            report.AppendLine();
            report.AppendLine("Her senaryo yeni bir oturumla (güven 0) başlar. Yanıtlar oyundaki `OakDialogueSession` üzerinden, yani aynı sistem promptu, JSON ayrıştırması ve güven kurallarıyla alınmıştır.");

            try
            {
                for (var s = 0; s < Scenarios.Length; s++)
                {
                    var scenario = Scenarios[s];
                    EditorUtility.DisplayProgressBar("Meşe senaryoları", scenario.Title, (float)s / Scenarios.Length);
                    var session = new OakDialogueSession(new OakDialogueClient(transport, settings));
                    foreach (var deed in scenario.Deeds)
                        session.AddDeed(deed);

                    report.AppendLine();
                    report.AppendLine($"## {s + 1}. {scenario.Title}");
                    report.AppendLine();
                    report.AppendLine($"**Beklenen:** {scenario.Goal}");
                    report.AppendLine();
                    report.AppendLine($"**Bilinen iyilikler:** {(scenario.Deeds.Length == 0 ? "yok" : string.Join(" ", scenario.Deeds))}");
                    report.AppendLine();
                    report.AppendLine("| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |");
                    report.AppendLine("|---|---|---|---|---|");

                    foreach (var line in scenario.Lines)
                    {
                        var watch = Stopwatch.StartNew();
                        var turn = await session.TalkAsync(line);
                        watch.Stop();
                        if (turn.IsOk)
                            times.Add(watch.ElapsedMilliseconds);
                        else
                            failures++;
                        var status = turn.IsOk ? string.Empty : $" ({turn.Status})";
                        report.AppendLine($"| {Cell(line)} | {Cell(turn.Reply)}{status} | {turn.Action.Name} | {turn.PreviousTrust} → {turn.CurrentTrust} | {watch.ElapsedMilliseconds} ms |");
                        UnityEngine.Debug.Log($"[Meşe Testi] {scenario.Title}: \"{line}\" -> \"{turn.Reply}\" [{turn.Action.Name}, {turn.PreviousTrust}->{turn.CurrentTrust}, {turn.Status}, {watch.ElapsedMilliseconds} ms]");
                    }
                }

                report.AppendLine();
                report.AppendLine("## Özet");
                report.AppendLine();
                if (times.Count > 0)
                {
                    times.Sort();
                    long total = 0;
                    foreach (var t in times)
                        total += t;
                    report.AppendLine($"- Başarılı yanıt: {times.Count}, başarısız: {failures}");
                    report.AppendLine($"- Ortalama süre: {total / times.Count} ms, medyan: {times[times.Count / 2]} ms, en kısa / en uzun: {times[0]} / {times[times.Count - 1]} ms");
                }
                else
                {
                    report.AppendLine($"- Hiç başarılı yanıt alınamadı ({failures} deneme). Ollama açık mı ve `{settings.Model}` modeli yüklü mü?");
                }

                var path = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? ".", OutputPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
                File.WriteAllText(path, report.ToString(), new UTF8Encoding(false));
                UnityEngine.Debug.Log($"[Meşe Testi] Bitti. Sonuçlar: {OutputPath}");
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _running = false;
            }
        }

        private static string Cell(string text)
        {
            return string.IsNullOrEmpty(text) ? "-" : text.Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
