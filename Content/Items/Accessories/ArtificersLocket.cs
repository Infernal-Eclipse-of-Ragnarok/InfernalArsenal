using System;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.BasicAccessories;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class ArtificersLocket : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = Item.sellPrice(0, 0, 75, 0);
            Item.rare = ItemRarityID.LightRed;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            float statPower = (float)Math.Round(0.25f * Utils.GetLerpValue(1, 0.5f, player.statLife / (float)player.statLifeMax2, true), 2);
            player.GetThoriumPlayer().accReducedKnockback = true;
            player.GetModPlayer<InfernalWeaponsPlayer>().ArtLocket = true;
            player.statDefense += 2 * (player.GetThoriumPlayer().statEnchantersEnergy / 15);
            player.lifeRegen += 1 * (player.GetThoriumPlayer().statEnchantersEnergy / 25);
            player.GetDamage<GenericDamageClass>() += statPower;
            player.moveSpeed += statPower;
            player.GetThoriumPlayer().statEnchantersEnergyTimer *= 1 + (int)statPower / 100;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(1);
            recipe.AddIngredient(ModContent.ItemType<GladiatorsLocket>(), 1);
            recipe.AddIngredient(ModContent.ItemType<ArtificersShield>(), 1);
            recipe.AddIngredient(ModContent.ItemType<PurifiedGel>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        }
}