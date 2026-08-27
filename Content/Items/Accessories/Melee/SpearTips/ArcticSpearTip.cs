using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Terraria.Localization;
using ThoriumMod.Items.BasicAccessories;
using ThoriumMod.Items;
using InfernalEclipseWeaponsDLC.Core;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.SpearTips
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class ArcticSpearTip : ThoriumItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("ThoriumMod");
        }

        public override void SetDefaults()
        {
            accDamage = Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.ArcticSpearTip");
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(0, 35);
            Item.rare = ItemRarityID.LightPurple;
            Item.accessory = true;
            accessoryType = AccessoryType.SpearTip;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().spearArctic = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient<CrystalSpearTip>()
                .AddTile(TileID.MythrilAnvil)
                .Register();

            if (ModIntegrationsSystem.Calamity.Loaded)
            {
                recipe.AddIngredient(ModIntegrationsSystem.Calamity.Mod.Find<ModItem>("CryonicBar").Type, 6);
            }
            else
            {
                recipe.AddIngredient(ItemID.FrostCore);
            }
        }
    }
}
