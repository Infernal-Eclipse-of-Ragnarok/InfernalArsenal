using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.Legacy
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class LegacyProSickle : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DemonSickle;

        public override void SetBardDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DemonSickle);
            AIType = ProjectileID.DemonSickle;
            Projectile.DamageType = ThoriumDamageBase<BardDamage>.Instance;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.penetrate = 2;
            Projectile.tileCollide = false;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 60);

            base.BardOnHitNPC(target, hit, damageDone);
        }
    }
}
