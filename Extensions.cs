namespace SCP008X
{
    public static class Extensions
    {
        public static bool NonHuman(this RoleType role, bool onlyScPs)
        {
            if (onlyScPs)
            {
                switch (role)
                {
                    case RoleType.Scp049:
                    case RoleType.Scp0492:
                    case RoleType.Scp079:
                    case RoleType.Scp096:
                    case RoleType.Scp106:
                    case RoleType.Scp173:
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                switch (role)
                {
                    case RoleType.Scp049:
                    case RoleType.Scp0492:
                    case RoleType.Scp079:
                    case RoleType.Scp096:
                    case RoleType.Scp106:
                    case RoleType.Scp173:
                    case RoleType.Scp93953:
                    case RoleType.Scp93989:
                    case RoleType.Spectator:
                    case RoleType.None:
                        return true;
                    default:
                        return false;
                }
            }
        }
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
    }
}
