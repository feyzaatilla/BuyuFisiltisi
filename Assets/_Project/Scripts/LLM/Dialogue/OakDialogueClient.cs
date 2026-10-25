using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.LLM.Ollama;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakDialogueClient
    {
        private readonly IHttpTransport transport;
        private readonly OllamaSettings settings;

        public OakDialogueClient(IHttpTransport transport, OllamaSettings settings)
        {
            this.transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<OakDialogueResult> AskAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
            if (messages == null || messages.Count == 0)
                throw new ArgumentException("En az bir mesaj gerekli.", nameof(messages));

            var body = OllamaChatRequest.Build(settings, messages);

            HttpResult result;
            try
            {
                result = await transport.PostJsonAsync(settings.ChatUrl, body, settings.TimeoutSeconds, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return OakDialogueResult.Fail(OakDialogueStatus.Cancelled);
            }
            catch (Exception)
            {
                return OakDialogueResult.Fail(OakDialogueStatus.ServerUnavailable);
            }

            switch (result.Failure)
            {
                case HttpFailure.None:
                    break;
                case HttpFailure.Timeout:
                    return OakDialogueResult.Fail(OakDialogueStatus.Timeout);
                case HttpFailure.Cancelled:
                    return OakDialogueResult.Fail(OakDialogueStatus.Cancelled);
                default:
                    return OakDialogueResult.Fail(OakDialogueStatus.ServerUnavailable);
            }

            if (!OllamaChatEnvelope.TryReadContent(result.Body, out var content))
                return OakDialogueResult.Fail(OakDialogueStatus.InvalidResponse, result.Body);

            return OakResponseParser.TryParse(content, out var response)
                ? OakDialogueResult.Ok(response, content)
                : OakDialogueResult.Fail(OakDialogueStatus.InvalidResponse, content);
        }
    }
}
