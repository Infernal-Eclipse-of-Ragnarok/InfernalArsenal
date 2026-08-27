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
using SOTS.Void;
using System.Reflection;
using InfernalEclipseWeaponsDLC.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void
{
    [ExtendsFromMod("SOTS")]
    public class CatastrophicLongbladeVoid : VoidItem
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/Void/CatastrophicLongblade";
        public override void SafeSetDefaults()
        {
            Item.width = 85;
            Item.height = 86;
            Item.damage = 10250;

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

            Item.shoot = ModContent.ProjectileType<CatastrophicLongbladeHoldoutVoid>();
            Item.shootSpeed = 0f;
        }

        public override bool BeforeUseItem(Player player)
        {
            if (InventoryHelperMethods.HasNeighborItem(player, Item.type, ModContent.ItemType<CataclysmicGauntletVoid>()))
            {
                Item.useTime = 24;
                Item.useAnimation = 24;
            }
            else
            {
                Item.useTime = 48;
                Item.useAnimation = 48;
            }
            return base.BeforeUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            bool nextTo = InventoryHelperMethods.HasNeighborItem(player, Item.type, ModContent.ItemType<CataclysmicGauntletVoid>());
            if (nextTo)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, Main.rand.Next(0, 2), 1f);

                int bulletType = ModContent.ProjectileType<SupremeCataclysmFist>();
                float baseSpeed = 5f;

                float halfSpread = MathHelper.ToRadians(5f);
                float[] angles = { -halfSpread, 0f, halfSpread };

                // Slight random speed variance per pellet
                for (int i = 0; i < 2; i++)
                {
                    float randMul = 0.95f + 0.10f * Main.rand.NextFloat();
                    Vector2 dir = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(angles[i]);
                    Vector2 vel = dir * baseSpeed * randMul;

                    int bulletDmg = 200;

                    Projectile.NewProjectile(source, position, vel, bulletType, bulletDmg, 10f, player.whoAmI, 0f, Main.rand.Next(0, 2), 1f);
                }
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, Main.rand.Next(0, 2));
            }
            return false;
            */
            if (!player.ownedProjectileCounts[type].Equals(0))
                return false;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override int GetVoid(Player player)
        {
            return 12;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SupremeCatastropheTrophy>()
                .AddTile<SCalAltar>()
                .Register();
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            /*
            var state = player.GetModPlayer<BladeSwingState>();

            if (player.itemAnimation == player.itemAnimationMax)
            {
                state.swingDirection *= -1;
                state.pendingShoot = true;
            }

            state.useDirection = (Main.MouseWorld.X >= player.Center.X) ? 1 : -1;
            player.direction = state.useDirection;

            state.useRotation = player.Center.AngleTo(Main.MouseWorld);

            float x = 1f - (float)player.itemAnimation / player.itemAnimationMax;
            float lerp = x < 0.5f ? 4f * x * x * x : 1f - (float)Math.Pow(-2f * x + 2f, 3) / 2f;

            float arcMult = 1f;
            if (player.itemAnimationMax > 20)
                arcMult += 0.4f * (player.itemAnimationMax - 20) / 30f;
            arcMult = MathHelper.Clamp(arcMult, 1f, 1.3f);

            float arcStart = -110f * arcMult;
            float arcEnd = 90f * arcMult;

            player.itemRotation =
                state.useRotation
                + MathHelper.ToRadians(state.useDirection == 1 ? 45 : 135)
                + MathHelper.ToRadians(
                    MathHelper.Lerp(arcStart, arcEnd, state.swingDirection == 1 ? lerp : 1f - lerp)
                    * state.useDirection);

            if (player.gravDir == -1f)
                player.itemRotation = -player.itemRotation;

            if (state.pendingShoot && player.itemAnimation == player.itemAnimationMax)
            {
                state.pendingShoot = false;
                VanillaShoot = true;
                MiItemCheckShoot?.Invoke(player, new object[] { player.whoAmI, Item, player.GetWeaponDamage(Item) });
                VanillaShoot = false;
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                player.itemRotation + MathHelper.ToRadians(-135f * state.useDirection));
            player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full,
                player.itemRotation + MathHelper.ToRadians(-135f * state.useDirection));

            base.UseStyle(player, heldItemFrame);
            */
        }
    }
}
