using CalamityMod.Items;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class CatastrophicLongblade : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return !ModLoader.HasMod("SOTS");
        }

        public override void SetDefaults()
        {
            Item.width = 85;
            Item.height = 86;
            Item.damage = 9250;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 49;
            Item.useAnimation = 49;

            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;

            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 9f;
            Item.autoReuse = true;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();

            Item.shoot = ModContent.ProjectileType<CatastrophicLongbladeHoldout>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            if (InventoryHelperMethods.HasNeighborItem(
                player,
                Item.type,
                ModContent.ItemType<CataclysmicGauntlet>()))
            {
                Item.useTime = 24;
                Item.useAnimation = 24;
            }
            else
            {
                Item.useTime = 48;
                Item.useAnimation = 48;
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (!player.ownedProjectileCounts[type].Equals(0))
                return false;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SupremeCatastropheTrophy>()
                .AddTile<SCalAltar>()
                .Register();
        }
    }
}
