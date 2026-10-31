using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro
{
    public class MagicPurpleBouncyBall : ModProjectile
    {
        private const int MaxBounces = 4;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1; // needed for bouncing
            Projectile.timeLeft = 240;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            Projectile.rotation += 0.27f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= MaxBounces)
            {
                Projectile.Kill();
                return false;
            }

            // Bounce logic
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = (-oldVelocity.X * 2.07f);

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = (-oldVelocity.Y * 2.07f);

            SoundEngine.PlaySound(SoundID.Item56, Projectile.Center);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.Kill();
        }
    }
}
