using CalamityMod;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro
{
    public class CorrodedCanePro : ModProjectile
    {
        public static readonly SoundStyle SpawnSound = OldDukeVortex.SpawnSound;
        public override string Texture => ModContent.GetInstance<OldDukeVortex>().Texture;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public Player Player => Main.player[Projectile.owner];

        public ref float MaxScale => ref Projectile.ai[0];
        public ref float AI_Timer => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 408;
            Projectile.height = 408;
            Projectile.scale = 0.004f;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            MaxScale = 0.4f;
            AI_Timer++;

            if (Player.channel && Player.CheckMana(10, pay: false))
            {
                if (Projectile.Opacity < 1f)
                    Projectile.Opacity += 0.01f;

                if (Projectile.scale < MaxScale)
                {
                    Projectile.scale += 0.005f;

                    if (Projectile.scale > MaxScale)
                        Projectile.scale = MaxScale;

                    Projectile.width = (Projectile.height = (int)(408f * Projectile.scale));
                }
                else
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        if (AI_Timer % 40 == 0)
                        {
                            Vector2 velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(default) * 20f;
                            Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<CorrodedCaneShark>(), Projectile.damage, Projectile.knockBack);
                        }
                    }
                }

                Projectile.timeLeft = 120;
            }
            else
            {
                if (Projectile.Opacity > 0f)
                    Projectile.Opacity -= 0.1f;

                if (Projectile.scale > 0)
                    Projectile.scale -= 0.012f;

                if (Projectile.Opacity <= 0f || Projectile.scale <= 0f)
                    Projectile.Kill();

                Projectile.width = (Projectile.height = (int)(408f * Projectile.scale));
            }

            Projectile.position.Y = MathHelper.Lerp(Projectile.position.Y, Player.position.Y - 200, 0.04f);
            Projectile.position.X = Player.Center.X - Projectile.width / 2; 
            Projectile.rotation -= 0.1f * (float)(1.0 - (double)Projectile.alpha / 255.0);

            float lightAmt = 2f * Projectile.scale;
            Lighting.AddLight(Projectile.Center, lightAmt, lightAmt * 2f, lightAmt);

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 174;
                SoundEngine.PlaySound(in SpawnSound, Projectile.Center);
            }

            int numDust = (int)(300f * Projectile.scale);
            float angleIncrement = (float)Math.PI * 2f / (float)numDust;
            Vector2 dustOffset = new(220f * Projectile.scale, 0f);
            dustOffset = dustOffset.RotatedByRandom(6.2831854820251465);
            int var = (int)(408f * Projectile.scale);
            for (int j = 0; j < numDust; j++)
            {
                if (Main.rand.NextBool(var))
                {
                    dustOffset = dustOffset.RotatedBy(angleIncrement);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.CursedTorch);
                    dust.position = Projectile.Center + dustOffset;
                    dust.noGravity = true;
                    dust.velocity = Vector2.Normalize(Projectile.Center - dust.position) * 4f;
                    dust.scale = Projectile.scale * 3f;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor);
            return false;
        }

        public override bool? CanHitNPC(NPC target) => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;
    }
}
