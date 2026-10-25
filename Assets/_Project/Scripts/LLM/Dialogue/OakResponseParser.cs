using System;
using UnityEngine;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakResponseParser
    {
        [Serializable]
        private sealed class Payload
        {
            public string reply;
            public string action;
            public int trust_delta;
        }

        public static bool TryParse(string raw, out OakResponse response)
        {
            response = OakResponse.Fallback;

            var json = ExtractObject(raw);
            if (json == null)
                return false;

            Payload payload;
            try
            {
                payload = JsonUtility.FromJson<Payload>(json);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (payload == null || string.IsNullOrWhiteSpace(payload.reply))
                return false;

            response = new OakResponse(payload.reply, ParseAction(payload.action), payload.trust_delta);
            return true;
        }

        public static OakAction ParseAction(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return OakAction.None;

            switch (value.Trim().ToLowerInvariant())
            {
                case "open_door":
                    return OakAction.OpenDoor;
                case "give_hint":
                    return OakAction.GiveHint;
                case "attack":
                    return OakAction.Attack;
                default:
                    return OakAction.None;
            }
        }

        private static string ExtractObject(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            return start >= 0 && end > start ? raw.Substring(start, end - start + 1) : null;
        }
    }
}
