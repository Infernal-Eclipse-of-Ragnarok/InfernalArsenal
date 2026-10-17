using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.AbyssalAncor
{
    public class AbyssCreature : ModProjectile
    {
        const float MaxTime = 120;
        public float Timer => MaxTime - Projectile.timeLeft;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 76;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = (int)MaxTime;
        }

        public override void AI()
        {
            // Advance the creature animation every 6 ticks.
            if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            if (Timer / MaxTime > 0.25f)
            {
                Projectile.velocity *= 0.95f;
            }

            if (Main.rand.NextBool(3))
            {
                int dustType = DustID.Water;
                Dust.NewDust(Projectile.Center, 14, 14, dustType, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.3f);
            }

            if (Projectile.velocity.Length() < 2f && Projectile.timeLeft < (int)(MaxTime / 2f))
            {
                Projectile.Kill();
                return;
            }

            Projectile.spriteDirection = Projectile.velocity.X < 0f ? -1 : 1;

            // Tilt slightly upward or downward while swimming.
            float targetRotation = MathHelper.Clamp(Projectile.velocity.Y * 0.025f, -0.35f, 0.35f);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation * Projectile.spriteDirection, 0.15f);

            HomeInOnNPC(Projectile, !Projectile.tileCollide, 350f, 12f, 20f);
        }


        public static void HomeInOnNPC(Projectile projectile, bool ignoreTiles, float range, float speed, float inertia)
        {
            NPC target = null;
            float closestDistance = range;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(projectile))
                    continue;

                float distance = Vector2.Distance(projectile.Center, npc.Center);

                if (distance >= closestDistance)
                    continue;

                if (!ignoreTiles && !Collision.CanHitLine(projectile.Center, 1, 1, npc.Center, 1, 1))
                    continue;

                closestDistance = distance;
                target = npc;
            }

            if (target == null)
                return;

            Vector2 desiredVelocity = (target.Center - projectile.Center).SafeNormalize(Vector2.Zero) * speed;

            projectile.velocity = (projectile.velocity * (inertia - 1f) + desiredVelocity) / inertia;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;

            int frameHeight = texture.Height / Main.projFrames[Type];

            Rectangle frame = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);

            Vector2 origin = frame.Size() * 0.5f;

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    int dustType = DustID.Blood;
                    Dust.NewDust(Projectile.Center, 14, 14, dustType, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.3f); ;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
                target.AddBuff(cal.Find<ModBuff>("CrushDepth").Type, 180);
        }
    }
}
