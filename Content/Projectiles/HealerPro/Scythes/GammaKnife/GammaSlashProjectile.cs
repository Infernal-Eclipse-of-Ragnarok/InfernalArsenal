using CalamityMod.Buffs.StatDebuffs;
using InfernalEclipseWeaponsDLC.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes.GammaKnife
{
    public class GammaSlashProjectile : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.width = 128;
            Projectile.height = 128;

            Projectile.penetrate = 15;
            Projectile.timeLeft = 20;  // lifespan of crescent
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.DamageType = ThoriumDamageBase<HealerDamage>.Instance;

            Projectile.ArmorPenetration = 15;
        }

        public override void AI()
        {
            float t = 1f - Projectile.timeLeft / 20f;

            // Scale down over time
            float scale = MathHelper.SmoothStep(1f, 0.3f, t);
            Projectile.scale = scale;

            // Reduce damage over time
            Projectile.damage = (int)(Projectile.damage * (1f - t * 0.05f));

            // Crescent parameters
            int dustCount = 12;                  // How many dust particles in the arc
            float radius = 128f * scale;        // Radius of crescent
            float arcStart = -MathHelper.PiOver4; // start angle
            float arcEnd = MathHelper.PiOver4;    // end angle

            // Get direction from velocity
            float velocityAngle = Projectile.velocity.ToRotation();

            for (int i = 0; i < dustCount; i++)
            {
                float lerp = i / (float)(dustCount - 1);
                float angle = MathHelper.Lerp(arcStart, arcEnd, lerp);

                // Apply velocity rotation so crescent faces movement direction
                angle += velocityAngle;

                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 dustPos = Projectile.Center + offset;

                Dust d = Dust.NewDustDirect(dustPos, 0, 0, ModContent.DustType<GammaDust>());
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(2f, 2.5f) * scale;
            }

            // Move projectile along velocity
            Projectile.position += Projectile.velocity;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
        }
    }
}
