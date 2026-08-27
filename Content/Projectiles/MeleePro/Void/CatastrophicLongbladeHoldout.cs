using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void
{
    public class CatastrophicLongbladeHoldout : BaseCustomUseStyleProjectile, ILocalizedModType
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/Void/CatastrophicLongblade";
        public override int AssignedItemID => ModContent.ItemType<CatastrophicLongblade>();

        public override Vector2 HitboxSize => new Vector2(140f, 140f);
        public override float HitboxOutset => 60f;
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45f);
        public override Vector2 SpriteOrigin => new Vector2(-8f, 90f);

        private bool firedMidSwing;

        // Swing state flags
        private bool doSwing = true;
        private bool postSwing;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void WhenSpawned()
        {
            var mp = Owner.GetModPlayer<CatastrophicSwingPlayer>();
            //mp.swingParity ^= 1;
            Projectile.ai[1] = mp.swingParity == 0 ? 1f : -1f;
            FlipAsSword = Owner.direction == -1;
        }

        public override void UseStyle()
        {
            Projectile.Center = Owner.MountedCenter;
            Projectile.velocity = Vector2.Zero;

            float useAnimMax = Owner.itemAnimationMax;
            AnimationProgress = Animation % useAnimMax;
            float progress = AnimationProgress / useAnimMax;

            // Determine direction based on ai[1]
            //FlipAsSword = Projectile.ai[1] < 0;

            // Reset swing at the beginning of a new cycle
            if (progress < 0.33f && !doSwing)
            {
                doSwing = true;
                postSwing = false;
                Projectile.ai[1] = -Projectile.ai[1];

                for (int i = 0; i < Main.maxNPCs; i++)
                    Projectile.localNPCImmunity[i] = 0;

                Projectile.numHits = 0;
                CanHit = false;

                firedMidSwing = false;
            }

            // Basher-style swing rotation
            float eased = CalamityUtils.ExpInOutEasing(progress, 1);
            float startRot = 150f * Projectile.ai[1] * Owner.direction;
            float endRot = 120f * -Projectile.ai[1] * Owner.direction;
            RotationOffset = MathHelper.Lerp(RotationOffset, MathHelper.ToRadians(MathHelper.Lerp(startRot, endRot, eased)), 0.2f);

            // Smooth rotation towards mouse
            Projectile.rotation = Utils.AngleLerp(
                Projectile.rotation,
                Owner.AngleTo(Main.MouseWorld) + MathHelper.ToRadians(45f),
                0.15f
            );

            // Activate hitboxes during main swing window
            if (progress > 0.25f && progress < 0.75f)
            {
                CanHit = true;
                postSwing = true;
            }
            else
            {
                CanHit = false;
            }

            bool swingingDown = Projectile.ai[1] * Owner.direction > 0;

            //Projectiles
            if (!firedMidSwing && progress >= 0.5f)
            {
                firedMidSwing = true;

                if (Main.myPlayer != Projectile.owner)
                    return;

                bool nextTo = InventoryHelperMethods.HasNeighborItem(
                    Owner,
                    AssignedItemID,
                    ModContent.ItemType<CataclysmicGauntlet>()
                );

                Vector2 direction = (Main.MouseWorld - Owner.Center)
                    .SafeNormalize(Vector2.UnitX);

                // === Spawn slash projectile ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Owner.Center,
                    direction * 5,
                    ModContent.ProjectileType<SupremeCatastropheSlash>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner,
                    0f,
                    swingingDown ? 1f : 0f
                );

                // === Pellet logic (ported from Shoot) ===
                if (nextTo)
                {
                    int bulletType = ModContent.ProjectileType<SupremeCataclysmFist>();
                    float baseSpeed = 5f;

                    float halfSpread = MathHelper.ToRadians(5f);
                    float[] angles = { -halfSpread, 0f, halfSpread };

                    for (int i = 0; i < 2; i++)
                    {
                        float randMul = 0.95f + 0.10f * Main.rand.NextFloat();
                        Vector2 dir = direction.RotatedBy(angles[i]);
                        Vector2 vel = dir * baseSpeed * randMul;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Owner.Center,
                            vel,
                            bulletType,
                            200,
                            10f,
                            Projectile.owner,
                            0f,
                            Main.rand.Next(0, 2),
                            1f
                        );
                    }
                }

                // === Swing sound ===
                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/ExobladeBeamSlash"), Projectile.position);
            }


            // Mark swing as finished
            if (progress >= 0.75f)
            {
                doSwing = false;
            }

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }

        public override void ResetStyle()
        {
        }
    }

    public class CatastrophicSwingPlayer : ModPlayer
    {
        public int swingParity;

        public override void ResetEffects()
        {
            // No persistent reset needed here
        }
    }
}
