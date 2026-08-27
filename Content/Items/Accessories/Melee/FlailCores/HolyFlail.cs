using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Terraria.ID;
using ThoriumMod.Items.BasicAccessories;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.FlailCores
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class HolyFlail : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.FlailCores;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().holyFlail = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<IronFlailCore>(1)
                .AddIngredient<DivineGeode>(10)
                .AddIngredient<UnholyEssence>(15)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}