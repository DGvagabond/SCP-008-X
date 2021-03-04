using Exiled.API.Features;

namespace SCP008X
{
    public static class Scp008XApi
    {
        public static bool Is008Infected(Player player) => EventHandlers.Victims.Contains(player);
    }
}
