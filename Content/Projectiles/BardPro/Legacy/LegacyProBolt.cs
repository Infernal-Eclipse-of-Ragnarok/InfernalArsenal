using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.Legacy
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class LegacyProBolt : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;
        public override string Texture => "Terraria/Images/Item_" + ItemID.None;

        public override void SetBardDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.DamageType = ThoriumDamageBase<BardDamage>.Instance;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, 0f, 0f, 150);
                dust.velocity = dust.velocity * 0.5f + Projectile.velocity * 0.5f;
                dust.velocity *= 0.5f;
                dust.scale = (5 - i) * 0.4f;
                dust.noGravity = true;
                dust.position = Projectile.Center + Projectile.velocity * 2f * (i + 1);
            }

            CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 400f, 10f, 20f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 60);

            base.BardOnHitNPC(target, hit, damageDone);
        }
    }
}
