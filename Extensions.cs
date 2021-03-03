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
    }
}
