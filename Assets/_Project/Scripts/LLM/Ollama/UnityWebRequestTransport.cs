using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace BuyuFisiltisi.LLM.Ollama
{
    public sealed class UnityWebRequestTransport : IHttpTransport
    {
        public async Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return HttpResult.Fail(HttpFailure.Cancelled);

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json ?? string.Empty)),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = Math.Max(1, (int)Math.Ceiling(timeoutSeconds))
            };
            request.SetRequestHeader("Content-Type", "application/json");

            var completion = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() =>
            {
                request.Abort();
                completion.TrySetResult(false);
            }))
            {
                var operation = request.SendWebRequest();
                operation.completed += _ => completion.TrySetResult(true);
                await completion.Task;
            }

            if (cancellationToken.IsCancellationRequested)
                return HttpResult.Fail(HttpFailure.Cancelled);

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    return HttpResult.Ok(request.downloadHandler.text);
                case UnityWebRequest.Result.ProtocolError:
                    return HttpResult.Fail(HttpFailure.HttpError, request.responseCode);
                default:
                    return HttpResult.Fail(IsTimeout(request.error) ? HttpFailure.Timeout : HttpFailure.ConnectionFailed, request.responseCode);
            }
        }

        private static bool IsTimeout(string error)
        {
            return !string.IsNullOrEmpty(error) && error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
