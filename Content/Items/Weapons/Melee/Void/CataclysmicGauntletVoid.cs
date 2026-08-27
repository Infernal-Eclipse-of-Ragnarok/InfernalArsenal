using CalamityMod.Items;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void;
using SOTS.Void;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.Audio;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void
{
    [ExtendsFromMod("SOTS")]
    public class CataclysmicGauntletVoid : VoidItem
    {
        public const int OnHitIFrames = 15;
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/Void/CataclysmicGauntlet";
        public override void SafeSetDefaults()
        {
            Item.height = 80;
            Item.width = 70;
            Item.damage = 750;
            Item.noMelee = true;
            Item.useTurn = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 5;
            Item.useTime = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.shootSpeed = 5f;
            Item.shoot = ModContent.ProjectileType<SupremeCataclysmFist>();
            Item.DamageType = DamageClass.Melee;
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit += 10;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool nextTo = InventoryHelperMethods.HasNeighborItem(player, Item.type, ModContent.ItemType<CatastrophicLongbladeVoid>());
            if (nextTo)
            {
                float spawnPos = Main.rand.Next(0, 18);
                Projectile.NewProjectile(source, position + new Vector2(0, spawnPos), velocity - (velocity / 20), type, damage, knockback, player.whoAmI, 0f, Main.rand.Next(0, 2), 1f);

                InfernalWeaponsPlayer mp = player.GetModPlayer<InfernalWeaponsPlayer>();
                if (mp.CataclysmFistShotCount >= 15)
                {
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBeamSlash"), player.position);
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SupremeCatastropheSlash>(), 800, 9f, player.whoAmI, 0f, Main.rand.Next(0, 2));
                    mp.CataclysmFistShotCount = 0;
                }
                mp.CataclysmFistShotCount++;
            }
            else
            {
                float spawnPos = Main.rand.Next(0, 18);
                Projectile.NewProjectile(source, position + new Vector2(0, spawnPos), velocity, type, damage, knockback, player.whoAmI, 0f, Main.rand.Next(0, 2));
            }
            return false;
        }

        public override int GetVoid(Player player)
        {
            return 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SupremeCataclysmTrophy>()
                .AddTile<SCalAltar>()
                .Register();
        }
    }
}
