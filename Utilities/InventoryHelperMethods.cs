using Terraria;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    public class InventoryHelperMethods
    {
        public static bool HasNeighborItem(Player player, int needleType, int neighborType)
        {
            int idx = FindInMainInventory(player, needleType);
            if (idx < 0) return false;

            return IsNeighborMatch(player, idx + 1, neighborType);
        }

        public static bool HasNeighborMatch(Player player, int needleType, System.Func<Item, bool> predicate)
        {
            int idx = FindInMainInventory(player, needleType);
            if (idx < 0) return false;

            return IsNeighborMatch(player, idx + 1, predicate);
        }

        public static int FindInMainInventory(Player player, int itemType)
        {
            for (int i = 0; i <= 57; i++)
                if (!player.inventory[i].IsAir && player.inventory[i].type == itemType)
                    return i;
            return -1;
        }

        // ---------- private helpers ----------
        private static bool IsNeighborMatch(Player player, int index, int requiredType)
            => InMainBounds(index) && !player.inventory[index].IsAir && player.inventory[index].type == requiredType;

        private static bool IsNeighborMatch(Player player, int index, System.Func<Item, bool> predicate)
            => InMainBounds(index) && !player.inventory[index].IsAir && predicate(player.inventory[index]);

        private static bool InMainBounds(int index) => index >= 0 && index <= 57;
    }
}
