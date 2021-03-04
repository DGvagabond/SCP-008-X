using Exiled.API.Features;
using SCP_343.API;
using scp035.API;

namespace SCP008X
{
    public static class Extensions
    {
        public static bool Gun(this ItemType item)
        {
            switch (item)
            {
                case ItemType.GunCOM15:
                case ItemType.GunE11SR:
                case ItemType.GunLogicer:
                case ItemType.GunMP7:
                case ItemType.GunProject90:
                case ItemType.GunUSP:
                case ItemType.MicroHID:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSerpentsHand(this Player player) =>
            SerpentsHand.API.SerpentsHand.GetSHPlayers().Contains(player);

        public static bool IsScp035(this Player player) => Scp035Data.GetScp035() == player;
        
        public static bool IsScp999(this Player player) => SCP999API.GetScp999() == player;

        public static bool IsScp343(this Player player) => SCP_343Data.TryGet343() == player;
    }
}
