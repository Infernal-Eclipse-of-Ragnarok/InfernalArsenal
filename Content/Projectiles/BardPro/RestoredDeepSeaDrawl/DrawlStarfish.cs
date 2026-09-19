using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.RestoredDeepSeaDrawl
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class DrawlStarfish : BardProjectile
    {
        const int FRAME_TIME = 8;
        const float TAU_OVER_FIVE = MathHelper.TwoPi / 5;

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override void SetBardDefaults()
        {
            Projectile.damage = 50;
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.CritChance = 10;
            Projectile.friendly = true;
            Projectile.maxPenetrate = -1;

            // not sure if -1 works here
            Projectile.timeLeft = 180;

            Projectile.alpha = 0;

            // exists until it bounces twice
            Projectile.penetrate = 1;

            Main.projFrames[Type] = 4;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            //hitbox.X += 10;
            //hitbox.Y += 10;
        }
        public override void AI()
        {
            if (Projectile.timeLeft % FRAME_TIME == 0)
            {
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }

            Projectile.velocity.Y += 0.01f;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            // since starfish have 5 ends...?
            for (int i = 0; i < 5; i++)
            {
                float radianDefault = MathHelper.PiOver2;
                float centerOff = 5f;
                var directionVector = Vector2.UnitY * -centerOff;
                var rotDirVector = directionVector.RotatedBy(radianDefault + TAU_OVER_FIVE * i);
                var rotPos = Projectile.Center + rotDirVector;
                Projectile.NewProjectileDirect(Entity.GetSource_FromThis(), rotPos, rotDirVector, ModContent.ProjectileType<DrawlStarfishShard>(), Projectile.damage, Projectile.knockBack);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }
    }
}
