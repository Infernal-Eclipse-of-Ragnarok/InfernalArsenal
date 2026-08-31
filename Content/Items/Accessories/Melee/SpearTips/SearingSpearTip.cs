using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Items;
using ThoriumMod.Items.BasicAccessories;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.SpearTips
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class SearingSpearTip : ThoriumItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("ThoriumMod");
        }

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            accDamage = Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.SearingSpearTip");
            Item.width = 36;
            Item.height = 36;
            Item.value = Item.buyPrice(0, 60);
            Item.rare = ItemRarityID.Yellow;
            Item.accessory = true;
            accessoryType = AccessoryType.SpearTip;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().spearSearing = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient<MoltenSpearTip>()
                .AddTile(TileID.MythrilAnvil)
                .Register();

            if (ModIntegrationsSystem.Calamity.Loaded)
            {
                recipe.AddIngredient(ModIntegrationsSystem.Calamity.Mod.Find<ModItem>("ScoriaBar").Type, 6);
            }
            else
            {
                recipe.AddIngredient(ItemID.FragmentSolar, 4);
            }
        }
    }
}
