using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Other
{
    [AutoloadEquip(EquipType.HandsOn)]
    public class ShadowDiamondRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 20;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().shadowDiamondRing = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 1);

            if (ModLoader.TryGetMod("MagicStorage", out Mod ms))
            {
                recipe.AddIngredient(ms.Find<ModItem>("ShadowDiamond"));
            }
            else
            {
                recipe.AddIngredient(ItemID.Diamond);
            }

            recipe.AddIngredient(ItemID.FallenStar);
            recipe.AddTile(TileID.Anvils);

            if (ms == null)
            {
                recipe.AddCondition(Condition.InGraveyard);
            }

            recipe.Register();
        }
    }
}
