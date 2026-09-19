using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Buffs.StatDebuffs;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.DukeSynth
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class SulfurSpirit : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3; // Match your explosion's frame count
        }
        public override void SetBardDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = ThoriumDamageBase<BardDamage>.Instance;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            // Homing
            float homingRange = 400f;
            float lerpFrames = 20f; // Higher = slower turn
            NPC target = null;
            float minDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                toTarget *= 15f; // Speed
                Projectile.velocity = (Projectile.velocity * (lerpFrames - 1) + toTarget) / lerpFrames;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.8f, 1.0f);
            if (Main.rand.NextBool(3))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ghost, 0f, 0f, 100, default, 1f);
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        public void Explode()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                //int radius = 60;
                // Explosion damage
                Projectile.NewProjectile(
                    Projectile.GetSource_Death(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SulfurExplosionPro>(), // See below for the explosion
                    (int)(Projectile.damage * 0.75),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            for (int i = 0; i < 25; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ghost, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
            }
            SoundEngine.PlaySound(SoundID.NPCDeath39, Projectile.Center);
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / (float)Main.projFrames[Projectile.type] / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Rectangle for current frame
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);

            // Rotate by -90° (opposite direction from before)
            float rotation = Projectile.rotation - MathHelper.PiOver2;

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                sourceRectangle,
                Projectile.GetAlpha(lightColor),
                rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
}
