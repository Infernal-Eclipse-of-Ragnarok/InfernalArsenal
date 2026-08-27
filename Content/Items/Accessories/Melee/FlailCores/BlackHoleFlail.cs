using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using ThoriumMod.Items.BasicAccessories;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using CalamityMod.Items;
using CalamityMod.Rarities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.FlailCores
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class BlackHoleFlail : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.FlailCores;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().blackholeFlail = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<IronFlailCore>(1)
                .AddIngredient(ModContent.ItemType<ShadowspecBar>(), 5)
                .AddIngredient(ModContent.ItemType<DarkPlasma>(), 15)
                .AddTile(ModContent.TileType<DraedonsForge>())
                .Register();
        }
    }
}