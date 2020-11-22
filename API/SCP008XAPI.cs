using Exiled.API.Features;
using SCP008X.Components;

namespace SCP008X.API
{
    public static class SCP008XAPI
    {
        public static bool Is008Infected(Player player)
        {
            if(player.ReferenceHub.gameObject.TryGetComponent(out SCP008 scp008))
            {
                return true;
            }
            return false;
        }
    }
}
