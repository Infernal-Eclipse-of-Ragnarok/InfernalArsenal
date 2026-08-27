using CalamityMod.Rarities;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Items.Donate;
using CalamityMod.Items.Materials;
using InfernalEclipseWeaponsDLC.Core.NewFolder;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.Yoyo
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class BlazeScourgeBag : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.YoyoPouches;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.value = CalamityMod.Items.CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().scourgeBag = true;

            player.yoyoGlove = true;
            player.yoyoString = true;
            player.counterWeight = 561;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.YoyoBag)
                .AddIngredient<BlizzardPouch>()
                .AddIngredient<UnholyEssence>(15)
                .AddIngredient<DivineGeode>(10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
