using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BrimstonePetalPro : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;
        public ref float Time => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetBardDefaults()
        {
            Projectile.width = Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 8)
            {
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
                Projectile.frameCounter = 0;
            }

            Time++;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                target.AddBuff(cal.Find<ModBuff>("BrimstoneFlames").Type, 180);
            }
            else target.AddBuff(BuffID.OnFire3, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[((ModProjectile)this).Projectile.type].Value;
            Rectangle sourceRectangle = Utils.Frame(texture, 1, Main.projFrames[((ModProjectile)this).Projectile.type], 0, ((ModProjectile)this).Projectile.frame, 0, 0);
            for (int i = 0; i < 6; i++)
            {
                Color magicAfterimageColor = Color.White * ((ModProjectile)this).Projectile.Opacity * 0.3f;
                magicAfterimageColor.A = 0;
                Vector2 drawPosition = ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition + Utils.ToRotationVector2((float)Math.PI * 2f * (float)i / 6f) * ((ModProjectile)this).Projectile.Opacity * 4f;
                Main.spriteBatch.Draw(texture, drawPosition, (Rectangle?)sourceRectangle, magicAfterimageColor, ((ModProjectile)this).Projectile.rotation, Utils.Size(texture) * 0.5f, ((ModProjectile)this).Projectile.scale, (SpriteEffects)0, 0f);
            }
            Main.spriteBatch.Draw(texture, ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition, (Rectangle?)sourceRectangle, ((ModProjectile)this).Projectile.GetAlpha(lightColor), ((ModProjectile)this).Projectile.rotation, Utils.Size(texture) * 0.5f, ((ModProjectile)this).Projectile.scale, (SpriteEffects)0, 0f);
            return false;
        }
    }
}