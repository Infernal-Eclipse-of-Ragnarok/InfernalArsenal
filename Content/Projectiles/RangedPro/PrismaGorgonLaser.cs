using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Buffs;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.RangedPro
{
    public class PrismaGorgonLaser : ModProjectile
    {
        public static int ExplosionDebuff => ModContent.BuffType<Petrify>();
        public const int DebuffDuration = 300;
        public const float ExplosionRadius = 120f;
        private bool detonated;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurpleLaser;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.dedServ)
                return;

            // Bright inner laser trail.
            for (int i = 0; i < 2; i++)
            {
                Vector2 position = Projectile.Center - Projectile.velocity * Main.rand.NextFloat();

                Dust dust = Dust.NewDustPerfect(position, DustID.IceTorch, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Color.Cyan, Main.rand.NextFloat(1.1f, 1.6f));

                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.05f, 0.55f, 0.95f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Detonate();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Detonate();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Detonate if the laser expires naturally.
            Detonate();
        }

        private void Detonate()
        {
            if (detonated)
                return;

            detonated = true;

            Vector2 center = Projectile.Center;

            // Explosion damage and debuffs are server-authoritative.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int explosionType = ModContent.ProjectileType<PrismaGorgonExplosion>();

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), center, Vector2.Zero, explosionType,  Projectile.damage * 2, Projectile.knockBack * 2f, Projectile.owner);
            }

            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(SoundID.Item14, center);

                // Outer explosion.
                for (int i = 0; i < 65; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 11f);

                    int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.BlueTorch;

                    Dust dust = Dust.NewDustPerfect(center, dustType, velocity, 0, Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(1.3f, 2.2f));

                    dust.noGravity = true;
                }

                // Bright inner flash.
                for (int i = 0; i < 25; i++)
                {
                    Dust dust = Dust.NewDustPerfect(center, DustID.IceTorch, Main.rand.NextVector2Circular(5f, 5f), 0, Color.White, Main.rand.NextFloat(1.5f, 2.5f));

                    dust.noGravity = true;
                }
            }

            if (Projectile.active)
                Projectile.Kill();
        }
    }

    public class PrismaGorgonExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurpleLaser;

        public override void SetDefaults()
        {
            Projectile.width = 240;
            Projectile.height = 240;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 3;
            Projectile.hide = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closestPoint = new Vector2(MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right), MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            return Vector2.DistanceSquared(Projectile.Center, closestPoint) <= PrismaGorgonLaser.ExplosionRadius *  PrismaGorgonLaser.ExplosionRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.boss)
                target.AddBuff(PrismaGorgonLaser.ExplosionDebuff, PrismaGorgonLaser.DebuffDuration);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
