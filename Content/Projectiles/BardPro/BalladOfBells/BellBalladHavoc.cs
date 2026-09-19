using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladHavoc : BellBalladEleum
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public override void SetStaticDefaults()
        {
        }

        public override void SetBardDefaults()
        {
            base.SetBardDefaults();
            Projectile.width = 20;
            Projectile.height = 24;
        }

        public override void Shoot(int damage, float knockBack)
        {
            if(Projectile.owner == Main.myPlayer)
            {
                Vector2 velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(default) * 10f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BellBalladHavocSlash>(), Projectile.damage, Projectile.knockBack);
            }
        }
    }
}
