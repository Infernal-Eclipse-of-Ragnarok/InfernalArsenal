using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using InfernalEclipseWeaponsDLC.Content.Buffs;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged
{
    public class GloomSwitch : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 4;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = Item.buyPrice(0, 2);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.Bullet;

            Item.width = 48;
            Item.height = 30;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            int buffIndex = player.FindBuffIndex(BuffID.ManaSickness);
            if (buffIndex != -1)
            {
                int remaining = player.buffTime[buffIndex]; // in ticks
                float reductionMultiplier = 0.25f * (remaining / 300f);

                damage *= 1f - reductionMultiplier;
            }
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<GloomSwitchPlayer>();
            var rushPlayer = player.GetModPlayer<DarkRushPlayer>();

            modPlayer.shotCounter++;

            // Adjust spread based on whether Overclock is active
            float maxSpread = modPlayer.lastShotConsumedMana ? 5f : 25f;
            Vector2 perturbedVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(maxSpread));

            // Spawn position calculations
            Vector2 aimDir = velocity.SafeNormalize(Vector2.UnitX);
            float muzzleLength = 30f;
            if (player.direction == -1) muzzleLength += 6f;
            Vector2 muzzleOffset = aimDir * muzzleLength;
            Vector2 perp = aimDir.RotatedBy(MathHelper.PiOver2);
            if (perp.Y * player.gravDir >= 0f) perp = -perp;
            Vector2 verticalOffset = perp * 4f;
            Vector2 spawnPos = player.MountedCenter + muzzleOffset + verticalOffset;

            if (modPlayer.shotCounter >= 15)
            {
                int projIndex = Projectile.NewProjectile(
                    source,
                    spawnPos,
                    perturbedVelocity,
                    ProjectileID.CursedFlameFriendly,
                    (int)(damage * 1.25f),
                    knockback,
                    player.whoAmI
                );

                if (projIndex >= 0 && Main.projectile[projIndex].active)
                {
                    Projectile proj = Main.projectile[projIndex];
                    proj.DamageType = DamageClass.Ranged;
                    proj.penetrate = 2;
                }

                // Dust effect
                for (int i = 0; i < 20; i++)
                {
                    float angle = Main.rand.NextFloat(-MathHelper.ToRadians(maxSpread), MathHelper.ToRadians(maxSpread));
                    Vector2 dustDir = aimDir.RotatedBy(angle);
                    Vector2 dustVel = dustDir * Main.rand.NextFloat(1f, 4f);
                    int dustIndex = Dust.NewDust(spawnPos, 0, 0, DustID.CorruptTorch, dustVel.X * 2.5f, dustVel.Y * 2.5f, 0, default, Main.rand.NextFloat(2f, 3f));
                    Main.dust[dustIndex].noGravity = true;
                }

                modPlayer.shotCounter = 0;
            }
            else
            {
                Projectile.NewProjectile(source, spawnPos, perturbedVelocity, type, damage, knockback, player.whoAmI);

                for (int i = 0; i < 5; i++)
                {
                    float angle = Main.rand.NextFloat(-MathHelper.ToRadians(maxSpread), MathHelper.ToRadians(maxSpread));
                    Vector2 dustDir = aimDir.RotatedBy(angle);
                    Vector2 dustVel = dustDir * Main.rand.NextFloat(1f, 2.5f);
                    int dustIndex = Dust.NewDust(spawnPos, 0, 0, DustID.Blood, dustVel.X, dustVel.Y, 0, default, Main.rand.NextFloat(0.5f, 1f));
                    Main.dust[dustIndex].noGravity = true;
                }
            }

            return false; // prevent default shooting
        }

        public override bool AltFunctionUse(Player player)
        {
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            // Check if player has enough mana for the "rush" effect
            var rushPlayer = player.GetModPlayer<DarkRushPlayer>();
            var modPlayer = player.GetModPlayer<GloomSwitchPlayer>();
            if (player.statMana >= 10)
            {
                // Consume 10 mana
                player.statMana -= 10;
                player.ManaEffect(10);

                rushPlayer.ActivateRush(20);
                modPlayer.lastShotConsumedMana = true;

                player.manaRegenDelay = 60;
                player.manaRegen = 0;
            }

            return base.CanUseItem(player); // normal left-click use
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-2f, 2f); // adjust as needed
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemoniteBar, 10)
                .AddIngredient(ItemID.ShadowScale, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class GloomSwitchPlayer : ModPlayer
    {
        public int shotCounter;
        public bool lastShotConsumedMana;

        public override void ResetEffects()
        {
            lastShotConsumedMana = false;
        }
    }

}
