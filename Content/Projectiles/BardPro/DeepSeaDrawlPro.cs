using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using ThoriumMod.Projectiles.Bard;
using Terraria.ModLoader;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class DeepSeaDrawlPro : BardProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override void SetBardDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 540;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.direction = MathF.Sign(Projectile.velocity.X);

            if (Projectile.velocity.X != 0f)
                Projectile.direction = (Projectile.spriteDirection = -Math.Sign(Projectile.velocity.X));

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 2)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;

            if (!Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
            {
                Projectile.alpha -= 30;
                if (Projectile.alpha < 60)
                    Projectile.alpha = 60;
            }
            else
            {
                Projectile.alpha += 30;
                if (Projectile.alpha > 150)
                    Projectile.alpha = 150;
            }

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BubbleBurst_White, 0, 0, 100, default, Main.rand.NextFloat(0.8f, 1.4f));
                dust.color = Color.CornflowerBlue;
                dust.color = Color.Lerp(dust.color, Color.White, 0.3f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Asset<Texture2D> texture = TextureAssets.Projectile[Type];
                int frame = ((Projectile.frame + i) % Main.projFrames[Type]);
                Rectangle sourceRect = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: frame);
                float rotation = Projectile.oldRot[i];
                Vector2 position = Projectile.oldPos[i] - Main.screenPosition + new Vector2(0, Projectile.height / 2);
                if (Projectile.direction > 0)
                    position.X += Projectile.width;
                Color color = lightColor * progress * Projectile.Opacity;
                Main.spriteBatch.Draw(texture.Value, position, sourceRect, color, rotation, sourceRect.Size() / 2f, Projectile.scale * progress, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BubbleBurst_White, Projectile.direction * 2, 0f, 100, default, 1.4f);
                dust.color = Color.CornflowerBlue;
                dust.color = Color.Lerp(dust.color, Color.White, 0.3f);
                dust.noGravity = true;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
                target.AddBuff(cal.Find<ModBuff>("RiptideDebuff").Type, 180);
        }
    }
}