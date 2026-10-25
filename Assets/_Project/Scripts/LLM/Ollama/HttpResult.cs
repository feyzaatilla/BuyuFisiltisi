namespace BuyuFisiltisi.LLM.Ollama
{
    public enum HttpFailure
    {
        None,
        ConnectionFailed,
        Timeout,
        HttpError,
        Cancelled
    }

    public readonly struct HttpResult
    {
        public HttpResult(HttpFailure failure, long statusCode, string body)
        {
            Failure = failure;
            StatusCode = statusCode;
            Body = body;
        }

        public HttpFailure Failure { get; }
        public long StatusCode { get; }
        public string Body { get; }
        public bool IsSuccess => Failure == HttpFailure.None;

        public static HttpResult Ok(string body) => new HttpResult(HttpFailure.None, 200, body);
        public static HttpResult Fail(HttpFailure failure, long statusCode = 0) => new HttpResult(failure, statusCode, null);
    }
}
