using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.RestoredDeepSeaDrawl
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class DrawlStarfishShard : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override void SetBardDefaults()
        {
            Projectile.damage = 45;
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.CritChance = 10;
            Projectile.friendly = true;
            Projectile.maxPenetrate = -1;

            // not sure if -1 works here
            Projectile.timeLeft = 360;

            Projectile.alpha = 0;

            // exists until it bounces twice
            Projectile.penetrate = 1;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            //hitbox.X -= 10;
            //hitbox.Y -= 10;
        }
        public override void AI()
        {
            var targetId = Projectile.FindTargetWithLineOfSight();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            // behavior w/ no target
            if (targetId < 0)
            {
                Projectile.velocity *= 0.998f;
                return;
            }
            // behavior w/ target

            var target = Main.npc[targetId];

            var diff = target.Center - Projectile.Center;

            Projectile.velocity += Vector2.Normalize(diff) * 0.1f;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }
    }
}
