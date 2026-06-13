using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod.Projectiles.Scythe;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.BarrenGarden
{
    public class BarrenGardenPro : ModProjectile
    {
        public override void SetDefaults()
        {
            ((ModProjectile)this).Projectile.DamageType = (DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance;
            ((Entity)((ModProjectile)this).Projectile).width = 18;
            ((Entity)((ModProjectile)this).Projectile).height = 14;
            ((ModProjectile)this).Projectile.aiStyle = -1;
            ((ModProjectile)this).Projectile.friendly = true;
            ((ModProjectile)this).Projectile.penetrate = 2;
            ((ModProjectile)this).Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 150) * (1f * ((ModProjectile)this).Projectile.Opacity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frozen, 60);
        }

        public override void AI()
        {
            Projectile.velocity *= 0.97f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
        }


        public override void OnKill(int timeLeft)
        {
            // Smaller burst of Frostburn dust on death
            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Frost,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    0,
                    default,
                    1f
                );
                Main.dust[dust].noGravity = true;
            }
        }
    }
}
