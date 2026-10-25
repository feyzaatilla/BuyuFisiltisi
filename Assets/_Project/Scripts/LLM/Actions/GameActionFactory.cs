using BuyuFisiltisi.Core.Actions;
using BuyuFisiltisi.LLM.Dialogue;

namespace BuyuFisiltisi.LLM.Actions
{
    public sealed class GameActionFactory
    {
        public const string SealedDoorId = "MuhurluKapi";
        public const int AttackDamage = 1;

        public IGameAction Create(OakResponse response)
        {
            if (response == null)
                return NoAction.Instance;

            switch (response.Action)
            {
                case OakAction.OpenDoor:
                    return new OpenDoorAction(SealedDoorId);
                case OakAction.GiveHint:
                    return new GiveHintAction(response.Reply);
                case OakAction.Attack:
                    return new AttackAction(AttackDamage);
                default:
                    return NoAction.Instance;
            }
        }
    }
}
