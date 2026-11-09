using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.GlobalItems
{
    public class PatreonGlobalItem : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                if (item.type == cal.Find<ModItem>("StarterBag").Type)
                {
                    AddPatreonItemsToStarterBag(item, itemLoot);
                }
            }

            if (ModLoader.TryGetMod("FargowiltasSouls", out Mod fargos))
            {
                if (item.type == fargos.Find<ModItem>("DevianttStarterBag").Type)
                {
                    AddPatreonItemsToStarterBag(item, itemLoot);
                }
            }
        }

        public static void AddPatreonItemsToStarterBag(Item item, ItemLoot itemLoot)
        {
            Player player = Main.LocalPlayer;

            ModLoader.TryGetMod("CalamityMod", out Mod cal);

            switch (player.name)
            {
                case "Shanakor":
                case "Shanny":
                    if (cal != null)
                        itemLoot.Add(ItemDropRule.Common(cal.Find<ModItem>("TrustyOldRod").Type));
                    break;
                case "REDWORT":
                    itemLoot.Add(ItemDropRule.Common(ItemID.UnluckyYarn, 1, 2));
                    break;
                default:
                    break;
            }
        }
    }
}
