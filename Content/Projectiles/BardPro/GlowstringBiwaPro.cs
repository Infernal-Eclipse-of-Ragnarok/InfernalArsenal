using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Utilities;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    public class GlowstringBiwaPro : ModProjectile
    {
        public override string Texture => "CalamityMod/Particles/Light";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;

            // Make the trail work
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2; // smooth trailing
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 900;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1; // Smooth movement and line
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 80;
        }

        public int TileBounces = 2;

        private Vector2 originalVelocity; // store the base velocity
        private bool storedVelocity = false; // flag to store velocity once

        private float oscillationTimer = 0f; // per projectile

        private float originalSpeed; // store magnitude of initial velocity

        public override void AI()
        {
            // Store original speed once
            if (!storedVelocity)
            {
                originalSpeed = Projectile.velocity.Length();
                storedVelocity = true;
            }

            // Update per-projectile timer
            oscillationTimer += 1f / 120f;

            // Smooth oscillation: 0.1 → 1 → 0.1
            float minFactor = 0.1f;
            float maxFactor = 1f;
            float speedFactor = minFactor + (maxFactor - minFactor) * (MathF.Sin(oscillationTimer * MathHelper.TwoPi) * 0.5f + 0.5f);

            // Apply speed factor while preserving current direction
            if (Projectile.velocity != Vector2.Zero)
            {
                Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                Projectile.velocity = currentDir * originalSpeed * speedFactor;
            }

            // Rotation
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Lighting
            Lighting.AddLight(Projectile.Center, Color.Cyan.ToVector3() * 0.6f);
        }

        private Projectile FindPartner()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active &&
                    proj.owner == Projectile.owner &&
                    proj.type == Projectile.type &&
                    proj.ai[1] == Projectile.ai[1] && // same pair
                    proj.whoAmI != Projectile.whoAmI)
                {
                    return proj;
                }
            }
            return null;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TileBounces--;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            return TileBounces < 0;
        }

        public override void OnKill(int timeLeft)
        {
            int dustAmount = 15; // number of dust particles

            for (int i = 0; i < dustAmount; i++)
            {
                // Random direction and speed
                Vector2 velocity = Main.rand.NextVector2Circular(0.2f, 0.5f);
                float scale = Main.rand.NextFloat(0.8f, 1.3f);

                // Choose color randomly: blue, green, or white
                Color dustColor;
                int choice = Main.rand.Next(3);
                switch (choice)
                {
                    case 0:
                        dustColor = new Color(140, 200, 255); // blue
                        break;
                    case 1:
                        dustColor = new Color(200, 255, 140); // green
                        break;
                    default:
                        dustColor = Color.White;              // white
                        break;
                }

                // Use a generic dust type with custom color
                int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Cloud, velocity.X, velocity.Y, 0, dustColor, scale);
                Dust dust = Main.dust[dustIndex];
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;

            float time = (float)Main.GlobalTimeWrappedHourly;
            float degrees = Projectile.whoAmI * 18f;

            // --- BASE COLOR PULSE ---
            float pulse = MathF.Sin(time * 3f + MathHelper.ToRadians(degrees)) * 0.5f + 0.5f; // 0 → 1
            Color vibrant = Color.Lerp(new Color(140, 200, 255), new Color(200, 255, 140), pulse);
            vibrant.A = 255;

            // --- WHITE INTENSITY PULSE ---
            float pulseWhite = (MathF.Sin(time * 5f + MathHelper.ToRadians(degrees)) * 0.5f + 0.5f);
            float dynamicWhiteTint = MathHelper.Lerp(0.15f, 0.6f, pulseWhite); // more white range (15% → 60%)
            float brightnessBoost = MathHelper.Lerp(0.8f, 1.8f, pulseWhite);   // stronger brightness pulse
            float outerAlpha = MathHelper.Lerp(0.5f, 1f, pulseWhite);          // alpha also pulses

            float scaleMult = Projectile.scale * 0.5f;
            float alphaMult = 0.05f + 0.95f * Projectile.ai[1];
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // --- TRAIL (behind core) ---
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 oldPos = Projectile.oldPos[k];
                if (oldPos == Vector2.Zero) continue;

                float fade = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;
                Vector2 trailPos = oldPos + Projectile.Size * 0.5f - Main.screenPosition;

                Color trailColor = Color.Lerp(vibrant, Color.White, dynamicWhiteTint) * (0.4f * fade * alphaMult);
                trailColor *= brightnessBoost;

                Main.spriteBatch.Draw(
                    tex,
                    trailPos,
                    null,
                    trailColor,
                    Projectile.rotation,
                    origin,
                    scaleMult * fade,
                    SpriteEffects.None,
                    0f
                );
            }

            // --- OUTER WHITE GLOW (subtle base) ---
            Main.spriteBatch.Draw(
                tex,
                drawPos,
                null,
                Color.White * 0.15f,
                Projectile.rotation,
                origin,
                scaleMult * 1.25f,
                SpriteEffects.None,
                0f
            );

            // --- OUTER LAYER (now visibly pulsing white) ---
            Color outerColor = Color.Lerp(vibrant, Color.White, dynamicWhiteTint) * outerAlpha;
            outerColor *= brightnessBoost; // makes it flare
            Main.spriteBatch.Draw(
                tex,
                drawPos,
                null,
                outerColor,
                Projectile.rotation,
                origin,
                scaleMult,
                SpriteEffects.None,
                0f
            );

            // --- MAIN CORE ---
            float innerScale = scaleMult * 0.5f;
            Main.spriteBatch.Draw(
                tex,
                drawPos,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                innerScale,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}
