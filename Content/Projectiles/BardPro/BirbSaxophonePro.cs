using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
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
    // Adapted from CalamityMod.Projectiles.Boss.RedLightningFeather
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class BirbSaxophonePro : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override string Texture => ModContent.GetInstance<CalamityMod.Projectiles.Boss.RedLightningFeather>().Texture;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetBardDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
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

            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(0f - Projectile.velocity.Y, 0f - Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }

            Projectile.Opacity = MathHelper.Clamp(1f - (Projectile.timeLeft - 1170) / 30f, 0f, 1f);
            Lighting.AddLight(Projectile.Center, 0.7f * Projectile.Opacity, 0f, 0f);
            Projectile.ai[0] += 1f;

            if (Projectile.velocity.Length() < 10f)
            {
                Projectile.velocity *= 1.015f;
            }

            if (Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming)
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
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ModLoader.HasMod("CalamityMod"))
            {
                CalamityHelper.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1, TextureAssets.Projectile[Projectile.type].Value, drawCentered: false);
                return false;
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.position = Projectile.Center;
            Projectile.width = Projectile.height = 64;
            Projectile.position.X = Projectile.position.X - Projectile.width / 2;
            Projectile.position.Y = Projectile.position.Y - Projectile.height / 2;
            EmitDust();
            Projectile.Damage();
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

        public override bool? CanHitNPC(NPC target)
        {
            if (target.friendly)
                return false;

            return Projectile.Opacity > 0.1f;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Damage > 0 && Projectile.Opacity > 0.1f)
            {
                if (ModLoader.TryGetMod("CalamityMod", out Mod calamity)) 
                {
                    target.AddBuff(calamity.Find<ModBuff>("VermillionFlux").Type, 120);
                }
                else target.AddBuff(BuffID.Electrified, 60);
            }

            if (hit.Crit)
                EmitDust();
        }
    }
}