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
        public OakClaimVerdict LastVerdict { get; private set; } = OakClaimVerdict.Honest;
        public OakTone LastTone { get; private set; } = OakTone.Neutral;
        public int PraiseCeiling => OakToneGuard.PraiseCeiling(Trust, OakDeedCatalog.MissingHints(deeds).Count == 0);

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

            var verdict = OakClaimGuard.Judge(text, deeds);
            var checkedResponse = OakClaimGuard.Apply(OakReplyTrim.Apply(result.Response), verdict);
            var tone = verdict == OakClaimVerdict.Honest ? OakToneGuard.Judge(text) : OakTone.Neutral;
            if (verdict == OakClaimVerdict.Honest)
                checkedResponse = OakToneGuard.Apply(checkedResponse, tone);
            if (tone == OakTone.Kind && Trust.Current >= PraiseCeiling)
                checkedResponse = new OakResponse(OakDeedCatalog.MissingHints(deeds).Count > 0 ? OakToneGuard.PraiseCeilingReply : checkedResponse.Reply, checkedResponse.Action, 0);

            var missing = OakDeedCatalog.MissingHints(deeds);
            var calm = verdict == OakClaimVerdict.Honest && !OakToneGuard.IsAngry(tone);
            if (calm && OakDoorHint.IsDoorRequest(text))
                checkedResponse = OakDoorHint.Request(checkedResponse, Trust.Current + checkedResponse.TrustDelta >= Trust.DoorThreshold, missing);
            else if (calm && OakQuestionGuide.TryAnswer(text, deeds, Trust.CanOpenDoor, out var guide))
                checkedResponse = OakQuestionGuide.Apply(checkedResponse, guide);

            if (checkedResponse.Action == OakAction.OpenDoor && missing.Count > 0)
                checkedResponse = OakDoorHint.Request(checkedResponse, Trust.Current + checkedResponse.TrustDelta >= Trust.DoorThreshold, missing);

            checkedResponse = OakToneGuard.Escalate(checkedResponse, tone, Trust.Current, Trust.AttackThreshold, Trust.Min);
            var gated = OakResponseGate.ApplyTurn(checkedResponse, Trust);
            conversation.Record(text, gated);
            LastVerdict = verdict;
            LastTone = tone;
            return new OakTurn(result.Status, gated.Reply, actions.Create(gated), before, Trust.Current);
        }
    }
}
