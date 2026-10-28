using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuyuFisiltisi.Core.Actions;
using BuyuFisiltisi.LLM.Actions;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakDialogueSession
    {
        private readonly OakDialogueClient client;
        private readonly OakConversation conversation;
        private readonly GameActionFactory actions;
        private readonly List<string> deeds = new List<string>();

        public OakDialogueSession(OakDialogueClient client, OakTrust trust = null, OakConversation conversation = null, GameActionFactory actions = null)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            Trust = trust ?? new OakTrust();
            this.conversation = conversation ?? new OakConversation();
            this.actions = actions ?? new GameActionFactory();
        }

        public OakTrust Trust { get; }
        public IReadOnlyList<string> Deeds => deeds;
        public int HistoryCount => conversation.Count;

        public bool AddDeed(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return false;

            var text = description.Trim();
            if (deeds.Contains(text))
                return false;

            deeds.Add(text);
            return true;
        }

        public async Task<OakTurn> TalkAsync(string playerText, CancellationToken cancellationToken = default)
        {
            var text = OakConversation.Normalize(playerText);
            if (text.Length == 0)
                throw new ArgumentException("Cadının sözü boş olamaz.", nameof(playerText));

            var before = Trust.Current;
            var prompt = OakPersona.BuildSystemPrompt(Trust, deeds);
            var messages = conversation.BuildRequest(prompt, text);

            var result = await client.AskAsync(messages, cancellationToken);
            if (!result.IsOk)
                return new OakTurn(result.Status, result.Response.Reply, NoAction.Instance, before, before);

            var gated = OakResponseGate.ApplyTurn(result.Response, Trust);
            conversation.Record(text, gated);
            return new OakTurn(result.Status, gated.Reply, actions.Create(gated), before, Trust.Current);
        }
    }
}
