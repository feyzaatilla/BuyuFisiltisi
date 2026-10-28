using System;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public static class OakResponseGate
    {
        public static OakResponse ApplyTurn(OakResponse response, OakTrust trust)
        {
            if (trust == null)
                throw new ArgumentNullException(nameof(trust));

            if (response == null)
                return OakResponse.Fallback;

            trust.Apply(response.TrustDelta);

            var action = response.Action;
            if (action == OakAction.OpenDoor && !trust.CanOpenDoor)
                action = OakAction.None;
            else if (action == OakAction.Attack && !trust.MayAttack)
                action = OakAction.None;

            return action == response.Action ? response : new OakResponse(response.Reply, action, response.TrustDelta);
        }
    }
}
