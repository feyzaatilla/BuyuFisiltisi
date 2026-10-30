using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Ollama
{
    public static class OllamaWarmup
    {
        public const string KeepAlive = "30m";
        public const float MinTimeoutSeconds = 120f;

        [Serializable]
        private sealed class Body
        {
            public string model;
            public string keep_alive;
        }

        public static string BuildRequest(OllamaSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return JsonUtility.ToJson(new Body { model = settings.Model, keep_alive = KeepAlive });
        }

        public static async Task<bool> RunAsync(IHttpTransport transport, OllamaSettings settings, CancellationToken cancellationToken = default)
        {
            if (transport == null)
                throw new ArgumentNullException(nameof(transport));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            try
            {
                var result = await transport.PostJsonAsync(settings.ChatUrl, BuildRequest(settings), Math.Max(settings.TimeoutSeconds, MinTimeoutSeconds), cancellationToken);
                return result.IsSuccess;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
