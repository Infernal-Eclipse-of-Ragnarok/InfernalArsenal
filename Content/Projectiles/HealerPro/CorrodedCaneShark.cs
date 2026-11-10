using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.NPCs.OldDuke;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro
{
    public class CorrodedCaneShark : ModProjectile
    {
        public override string Texture => ModContent.GetInstance<SulphurousSharkron>().Texture;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            AIType = -1;
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60 * 5;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.direction = Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
            if (Projectile.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 origin = new(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type] / 2);
            int count = 10;
            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 1; i < count; i += 2)
                {
                    Color color = lightColor;
                    color = Color.Lerp(color, Color.Lime, 0.5f);
                    color = Projectile.GetAlpha(color);
                    color *= (float)(count - i) / 15f;
                    Vector2 trailPos = Projectile.oldPos[i] + new Vector2(Projectile.width, Projectile.height) / 2f - Main.screenPosition;
                    trailPos -= new Vector2(texture.Width, texture.Height / Main.projFrames[Projectile.type]) * Projectile.scale / 2f;
                    trailPos += origin * Projectile.scale + new Vector2(0f, Projectile.gfxOffY);
                    Main.EntitySpriteDraw(texture, trailPos, frame, color, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
                }
            }
            Vector2 position = Projectile.Center - Main.screenPosition;
            position -= new Vector2(texture.Width, texture.Height / Main.projFrames[Projectile.type]) * Projectile.scale / 2f;
            position += origin * Projectile.scale + new Vector2(0f, Projectile.gfxOffY);
            Main.EntitySpriteDraw(texture, position, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(in SoundID.NPCDeath12, Projectile.position);
            Projectile.position.X = Projectile.position.X + (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (float)(Projectile.height / 2);
            Projectile.width = (Projectile.height = 96);
            Projectile.position.X = Projectile.position.X - (float)(Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (float)(Projectile.height / 2);
            for (int i = 0; i < 15; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.CursedTorch, 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity.Y *= 6f;
                Main.dust[dustIndex].velocity.X *= 3f;
                if (Main.rand.NextBool(2))
                {
                    Main.dust[dustIndex].scale = 0.5f;
                    Main.dust[dustIndex].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                }
            }
            for (int i = 0; i < 30; i++)
            {
                int dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default(Color), 3f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].velocity.Y *= 10f;
                dustIndex = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default(Color), 2f);
                Main.dust[dustIndex].velocity.X *= 2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 300);
        }
    }
}
