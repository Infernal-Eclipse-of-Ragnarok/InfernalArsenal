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
    public class DrawlIsopod : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override void SetBardDefaults()
        {
            Projectile.damage = 60;
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.CritChance = 10;
            Projectile.friendly = true;
            Projectile.maxPenetrate = -1;

            // not sure if -1 works here
            Projectile.timeLeft = 180;

            Projectile.alpha = 0;

            // exists until it bounces twice
            Projectile.penetrate = 3;

            Projectile.knockBack = 10;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {

        }
        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water, newColor: Color.Purple);
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }
        public override void OnKill(int timeLeft)
        {
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}
