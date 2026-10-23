using UnityEngine.AI;

namespace BuyuFisiltisi.Companion.Movement
{
    public static class AgentTypeLookup
    {
        public static bool TryFindId(string agentTypeName, out int agentTypeId)
        {
            var count = NavMesh.GetSettingsCount();
            for (var i = 0; i < count; i++)
            {
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                if (NavMesh.GetSettingsNameFromID(id) == agentTypeName)
                {
                    agentTypeId = id;
                    return true;
                }
            }

            agentTypeId = 0;
            return false;
        }
    }
}
