using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Materials
{
    public class DeepSeaDrawlShard2 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
