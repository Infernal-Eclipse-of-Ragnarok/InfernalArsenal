using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladSunlightLaser : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public override void SetBardDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 2;
            Projectile.scale = 1.2f;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.friendly = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                SoundEngine.PlaySound(SoundID.Item12, Projectile.position);
            }

            Projectile.localAI[1] += 1f;
            if (Projectile.localAI[1] > 2f)
            {
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 15;

                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            Lighting.AddLight(Projectile.Center, new Color(252, 255, 61).ToVector3());
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity * 2f, Projectile.width, Projectile.height, DustID.Ichor, Projectile.velocity.X, Projectile.velocity.Y, 100);
                dust.velocity = Vector2.Zero;
                dust.position -= Projectile.velocity / 5f * i;
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.noLight = true;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
                target.AddBuff(cal.Find<ModBuff>("Crumbling").Type, 60 * 2);
            else
                target.AddBuff(BuffID.Ichor, 60 * 2);
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}
