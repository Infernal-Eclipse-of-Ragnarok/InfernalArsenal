using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;
using Microsoft.Xna.Framework;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.RestoredDeepSeaDrawl
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class DrawlEel : BardProjectile
    {
        const int FRAME_TIME = 6;

        Vector2 attachOffset;
        NPC attachVictim;
        int attachTime;

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override void SetBardDefaults()
        {
            Projectile.damage = 50;
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.CritChance = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.maxPenetrate = -1;

            // not sure if -1 works here
            Projectile.timeLeft = 240;

            Projectile.alpha = 0;

            // exists until it bounces twice
            Projectile.penetrate = -1;

            Main.projFrames[Type] = 8;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var baseV = Vector2.UnitX * 30;
            var newPos = baseV.RotatedBy(Projectile.rotation);

            hitbox.X += 30;
            hitbox.Y += 3;

            hitbox.X += (int)newPos.X;
            hitbox.Y += (int)newPos.Y;
        }
        public override void AI()
        {
            // handle animation
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.timeLeft % FRAME_TIME == 0)
            {
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }

            if (attachVictim == null)
            {
                Projectile.velocity.Y += 0.05f;
                return;
            }

            Projectile.position = attachVictim.Center - attachOffset;

            if (attachTime % 10 == 0)
            {
                attachVictim.SimpleStrikeNPC(Projectile.damage, 0, Main.rand.NextBool(10), damageType: ModContent.GetInstance<BardDamage>(), damageVariation: true);
            }
            if (attachTime >= 180)
                Projectile.Kill();

            attachTime++;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (attachVictim != null) return;

            attachVictim = target;
            attachOffset = attachVictim.Center - Projectile.Center;
            Projectile.friendly = false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.Center, 10, 10, DustID.Ash, newColor: Color.Black);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}
