using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Materials
{
    public class DeepSeaDrawlShard3 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.buyPrice(0, 22, 80, 0);
            Item.value = Item.buyPrice(0, 35);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
