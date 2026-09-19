using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladEleumBolt : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FrostBoltStaff}";

        public override void SetBardDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.alpha = 255;
            Projectile.penetrate = 2;
            Projectile.friendly = true;
            Projectile.coldDamage = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Frost);
                if (!Main.rand.NextBool(3))
                {
                    dust.velocity /= 2f;
                    dust.noGravity = true;
                    dust.scale *= 1.75f;
                }
                else
                {
                    dust.scale *= 0.5f;
                }
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 60 * 4);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
