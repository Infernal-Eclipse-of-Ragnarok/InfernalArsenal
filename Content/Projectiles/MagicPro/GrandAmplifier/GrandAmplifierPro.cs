using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro.GrandAmplifier
{
    public class GrandAmplifierPro : ModProjectile
    {
        public override string Texture => "ThoriumMod/Projectiles/Boss/ThunderSpark";

        public override void SetStaticDefaults()
        {
            Main.projFrames[((ModProjectile)this).Projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 0.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 300, false);
        }

        public override void AI()
        {
            // Dust
            int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MagicMirror, 0f, 0f, 100, default, 1f);
            Main.dust[dust].velocity *= 0.2f;
            Main.dust[dust].noGravity = true;

            // Rotation to velocity
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }

            if (Projectile.frame >= Main.projFrames[Projectile.type])
            {
                Projectile.frame = 0;
            }
        }
    }
}
