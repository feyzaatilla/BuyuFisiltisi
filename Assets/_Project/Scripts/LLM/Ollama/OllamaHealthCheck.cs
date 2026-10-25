using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Ollama
{
    public sealed class OllamaHealthCheck
    {
        [Serializable]
        private sealed class ShowBody
        {
            public string model;
        }

        private const long NotFound = 404;
        private const float MaxTimeoutSeconds = 5f;

        private readonly IHttpTransport transport;
        private readonly OllamaSettings settings;

        public OllamaHealthCheck(IHttpTransport transport, OllamaSettings settings)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string ShowUrl => settings.BaseUrl + "/api/show";
        public float TimeoutSeconds => Mathf.Min(settings.TimeoutSeconds, MaxTimeoutSeconds);

        public async Task<OllamaHealth> CheckAsync(CancellationToken cancellationToken = default)
        {
            var body = JsonUtility.ToJson(new ShowBody { model = settings.Model });

            HttpResult result;
            try
            {
                result = await transport.PostJsonAsync(ShowUrl, body, TimeoutSeconds, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return OllamaHealth.Unknown;
            }
            catch (Exception)
            {
                return OllamaHealth.Unreachable;
            }

            return Interpret(result);
        }

        public static OllamaHealth Interpret(HttpResult result)
        {
            switch (result.Failure)
            {
                case HttpFailure.None:
                    return OllamaHealth.Ready;
                case HttpFailure.Cancelled:
                    return OllamaHealth.Unknown;
                case HttpFailure.HttpError when result.StatusCode == NotFound:
                    return OllamaHealth.ModelMissing;
                default:
                    return OllamaHealth.Unreachable;
            }
        }
    }
}
