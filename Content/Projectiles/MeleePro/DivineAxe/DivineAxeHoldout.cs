using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.DivineAxe
{
    public class DivineAxeHoldout : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/DivineAxe";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 112;
            Projectile.height = 122;
            Projectile.friendly = true;
            Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            Projectile.scale = player.GetAdjustedItemScale(player.HeldItem); // Adjust the projectile's scale based on the player's held item scale -Arkangel

            if (player.dead || !player.active || player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            float progress = 1f - ((float)player.itemAnimation / player.itemAnimationMax);
            float lerpValue = 0f;

            if (progress < 0.15f)
            {
                float localProgress = progress / 0.15f;
                lerpValue = MathHelper.Lerp(0f, -0.2f, (float)Math.Sin(localProgress * MathHelper.PiOver2));
            }
            else if (progress < 0.60f)
            {
                lerpValue = -0.2f;
            }
            else
            {
                float localProgress = (progress - 0.60f) / 0.40f;
                lerpValue = MathHelper.Lerp(-0.2f, 1f, (float)Math.Pow(localProgress, 3));
            }

            float swingDirection = Projectile.ai[0];
            float aimAngle = Projectile.velocity.ToRotation();
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

            float swingArc = MathHelper.Pi * 0.8f;
            float angleOffset = MathHelper.Lerp(-swingArc, swingArc, lerpValue) * swingDirection * player.direction;
            float visualRotation = aimAngle + angleOffset;

            Projectile.Center = player.MountedCenter;
            Projectile.rotation = visualRotation;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, visualRotation - MathHelper.PiOver2);

            if (progress >= 0.60f)
            {
                Vector2 bladePosition = Projectile.Center + visualRotation.ToRotationVector2() * (130f * Projectile.scale); // Adjusted blade position based on projectile scale -Arkangel

                for (int i = 0; i < 3; i++)
                {
                    Vector2 randomOffset = Main.rand.NextVector2Circular(20f, 20f);

                    Dust heatDust = Dust.NewDustPerfect(
                        bladePosition + randomOffset,
                        DustID.SolarFlare,
                        Vector2.Zero,
                        100,
                        Color.Gold,
                        Main.rand.NextFloat(1.2f, 1.8f)
                    );

                    heatDust.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.ai[0] == 1)
                SoundEngine.PlaySound(new SoundStyle("InfernalEclipseWeaponsDLC/Assets/Sounds/DemonSwordImpact1"), target.Center);
            else
                SoundEngine.PlaySound(new SoundStyle("InfernalEclipseWeaponsDLC/Assets/Sounds/DemonSwordImpact2"), target.Center);

            float distance = Vector2.Distance(player.Center, target.Center);
            if (distance > 80f)
            {
                SoundEngine.PlaySound(SoundID.Item14, target.Center);
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<DivineExplosion>(), damageDone / 2, 5f, player.whoAmI);
                }
                target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Player player = Main.player[Projectile.owner];
            SpriteEffects spriteEffects = (Projectile.ai[0] * player.direction > 0) ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 origin = (spriteEffects == SpriteEffects.FlipVertically) ? new Vector2(0, 0) : new Vector2(0, texture.Height);

            // Default draw rotation
            float drawRotation = Projectile.rotation + ((spriteEffects == SpriteEffects.FlipVertically) ? -MathHelper.PiOver4 : MathHelper.PiOver4);

            for (int k = 1; k < Projectile.oldRot.Length; k++)
            {
                if (Projectile.oldRot[k] == 0f) continue;

                float trailProgress = (float)k / Projectile.oldRot.Length;
                float alpha = 1f - trailProgress;

                Color trailColor = new Color(255, 150, 0, 0) * alpha * 0.6f;

                float oldDrawRotation = Projectile.oldRot[k] + ((spriteEffects == SpriteEffects.FlipVertically) ? -MathHelper.PiOver4 : MathHelper.PiOver4);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition,
                    null,
                    trailColor,
                    oldDrawRotation,
                    origin,
                    Projectile.scale,
                    spriteEffects,
                    0
                );
            }


            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, drawRotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;

            Vector2 end = Projectile.Center + Projectile.rotation.ToRotationVector2() * (150f * Projectile.scale);
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, end, 60f * Projectile.scale, ref collisionPoint); //added a scale multiplier for collision detection -Arkangel
        }
    }
}
