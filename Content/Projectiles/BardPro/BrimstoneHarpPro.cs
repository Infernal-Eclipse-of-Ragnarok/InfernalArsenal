using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    public class BrimstoneHarpPro : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public int TileBounces = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }

        public override void SetBardDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;

            Projectile.direction = Math.Sign(Projectile.velocity.X);
            Projectile.spriteDirection = -Projectile.direction;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.direction < 0)
               Projectile.rotation += MathHelper.Pi;

            Projectile.Opacity = MathHelper.Clamp(1f - (Projectile.timeLeft - 1170) / 30f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, new Color(220, 83, 99).ToVector3() * Projectile.Opacity);

            /*
            // homing, disabled for now
            if (false)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy(Projectile.owner) && Vector2.DistanceSquared(npc.Center, Projectile.Center) < 500 * 500)
                    {
                        Vector2 vector = npc.Center - Projectile.Center;
                        float num4 = Projectile.velocity.Length();
                        vector.Normalize();
                        vector *= num4;
                        Projectile.velocity = (Projectile.velocity * 19f + vector) / 20f;
                        Projectile.velocity.Normalize();
                        Projectile.velocity *= num4;
                        break;
                    }
                }
            }
            */
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TileBounces--;

            // Reflect velocity (basic bounce)
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            SpawnFanProjectiles();

            return TileBounces < 0;
        }

        private NPC FindNearestNPC(float maxDistance = 600f)
        {
            NPC target = null;
            float distance = (maxDistance + 200);

            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float d = Vector2.Distance(npc.Center, Projectile.Center);
                    if (d < distance)
                    {
                        distance = d;
                        target = npc;
                    }
                }
            }
            return target;
        }

        private void SpawnFanProjectiles()
        {
            NPC target = FindNearestNPC();
            if (target == null)
                return;

            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);

            float spread = MathHelper.ToRadians(20f); // fan angle
            int count = 3;

            for (int i = 0; i < count; i++)
            {
                float offset = MathHelper.Lerp(-spread, spread, i / (float)(count - 1));
                Vector2 velocity = direction.RotatedBy(offset) * 8f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<BrimstonePetalPro>(),
                    Projectile.damage / 2,
                    0f,
                    Main.myPlayer
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int length = Projectile.oldPos.Length;
            Asset<Texture2D> texture = TextureAssets.Projectile[Type];
            Rectangle sourceRect;
            for (int i = 1; i < length; i++)
            {
                sourceRect = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
                float progress = i / (float)length;
                float wave = MathF.Sin(((length - i) + Projectile.timeLeft)) * (2f + 1f * progress);
                Vector2 waveOffset = Math.Abs(Projectile.velocity.X) > Math.Abs(Projectile.velocity.Y) ? new Vector2(0, wave) : new Vector2(wave, 0);
                Vector2 drawPos = Projectile.oldPos[i];
                if (i > 0) drawPos = Vector2.Lerp(Projectile.oldPos[i], Projectile.position, 0.3f);
                drawPos += Projectile.Size / 2f + waveOffset - Main.screenPosition;
                Color trailColor = new Color(220, 83, 99, 0) * Projectile.Opacity * 0.75f;
                float rotation = Projectile.velocity.X < 0 ? MathHelper.Pi + Projectile.oldRot[i] : Projectile.oldRot[i];
                float scale = Projectile.scale * 1.2f * (1f - progress);

                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, drawPos, sourceRect, trailColor, rotation, Projectile.Size / 2f, (float)scale, effects, 0f);
            }

            sourceRect = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.position - Main.screenPosition + Projectile.Size / 2f, sourceRect, Color.White * Projectile.Opacity, Projectile.rotation, Projectile.Size / 2f, Projectile.scale, effects, 0f);
            return false;

        }

        public override void OnKill(int timeLeft)
        {
            EmitDust();
        }

        private void EmitDust()
        {
            SoundEngine.PlaySound(in SoundID.Item109, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
            }

            for (int j = 0; j < 10; j++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 0, default, 2.5f);
                dust.noGravity = true;
                dust.velocity *= 3f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                dust.velocity *= 2f;
                dust.noGravity = true;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            EmitDust();
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
        }
    }
}