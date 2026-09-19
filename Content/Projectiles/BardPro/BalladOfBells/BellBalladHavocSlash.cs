using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladHavocSlash : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DD2SquireSonicBoom}";

        public override void SetBardDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 5;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.timeLeft = 60;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.alpha -= 40;
            if (Projectile.alpha < 0)
                Projectile.alpha = 0;

            Projectile.spriteDirection = Projectile.direction;
            Projectile.localAI[0] += 1f;
            for (int i = 0; i < 1; i++)
            {
                Vector2 spinningPoint = Utils.RandomVector2(Main.rand, -0.5f, 0.5f) * new Vector2(20f, 80f);
                spinningPoint = spinningPoint.RotatedBy(Projectile.velocity.ToRotation());
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch);
                dust.alpha = 127;
                dust.fadeIn = 1.5f;
                dust.scale = 1.3f;
                dust.velocity *= 0.3f;
                dust.position = Projectile.Center + spinningPoint;
                dust.noGravity = true;
                dust.noLight = true;
                dust.color = new Color(255, 255, 255, 0);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2f;

            Lighting.AddLight(Projectile.Center, 1.1f, 0.3f, 0.4f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(-MathHelper.PiOver2) * Projectile.scale;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - direction * 40f, Projectile.Center + direction * 40f, 16f * Projectile.scale, ref collisionPoint))
                return true;

            return false;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CalamityMod.Buffs.DamageOverTime.BrimstoneFlames>(), 60 * 2);
        }

        public override void OnKill(int timeLeft)
        {
            int count = Main.rand.Next(15, 25);
            for (int i = 0; i < count; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, 0f, 0f, 100, new Color(255, 255, 255, 0), 1.3f);
                dust.velocity *= 8f * (0.3f + 0.7f * Main.rand.NextFloat());
                dust.fadeIn = 1.3f + Main.rand.NextFloat() * 0.2f;
                dust.noLight = true;
                dust.noGravity = true;
                dust.position += dust.velocity * 4f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(255, 255, 255, 200) * ((255f - Projectile.alpha) / 255f);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            int numFrames = texture.Height / Main.projFrames[Type];
            int frameX = numFrames * Projectile.frame;
            Rectangle sourceRect = new(0, frameX, texture.Width, numFrames);
            Vector2 origin = sourceRect.Size() / 2f;
            float opacity = (Projectile.localAI[0] * ((float)Math.PI * 2f) / 30f).ToRotationVector2().X;
            Color color = new(220, 40, 30, 40);
            color *= 0.75f + 0.25f * opacity;

            SpriteEffects dir = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                dir = SpriteEffects.FlipHorizontally;

            float count = 12 * (1f - Projectile.alpha / 255f);
            for (int n = (int)count; n >= 0; n--)
            {
                float progress = MathHelper.Clamp(n / count, 0, 1);
                Color trailColor = color * (1f - progress);
                Vector2 trailPosition = Projectile.Center - Projectile.velocity.SafeNormalize(default) * n * 6f;
                Main.EntitySpriteDraw(texture, trailPosition - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), null, trailColor, Projectile.rotation, sourceRect.Size() / 2, Projectile.scale, dir, 0);
            }

            for (int i = 0; i < 8; i++)
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) + Projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 4f * i) * (4f + 1f * opacity), sourceRect, color, Projectile.rotation, origin, Projectile.scale, dir);

            color = Projectile.GetAlpha(lightColor);
            color.A = 127;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), sourceRect, color, Projectile.rotation, origin, Projectile.scale, dir);

            return false;
        }
    }
}
