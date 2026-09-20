using CalamityMod.NPCs.TownNPCs;
using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.GlobalNPCs
{
    public class ShopNPCAdditions : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == ModContent.NPCType<Archmage>())
            {
                shop.Add(new NPCShop.Entry(ModContent.ItemType<DeepSeaDrawlShard3>(), Condition.DownedGolem));
            }
        }
    }
}
