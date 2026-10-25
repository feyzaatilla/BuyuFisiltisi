using System.Threading;
using System.Threading.Tasks;

namespace BuyuFisiltisi.LLM.Ollama
{
    public interface IHttpTransport
    {
        Task<HttpResult> PostJsonAsync(string url, string json, float timeoutSeconds, CancellationToken cancellationToken);
    }
}
