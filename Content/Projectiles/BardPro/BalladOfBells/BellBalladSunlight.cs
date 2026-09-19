using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles.Bard;
using Microsoft.Xna.Framework;
using ThoriumMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladSunlight : BellBalladEleum
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public override void SetStaticDefaults()
        {
        }

        public override void SetBardDefaults()
        {
            base.SetBardDefaults();
            Projectile.width = 22;
            Projectile.height = 26;
        }

        public override void Shoot(int damage, float knockBack)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(default) * 10f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BellBalladSunlightLaser>(), Projectile.damage, Projectile.knockBack);
            }
        }
    }
}
