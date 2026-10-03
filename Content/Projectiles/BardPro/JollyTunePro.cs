using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class JollyTunePro : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        private const int TrailLength = 10;
        public const float OrbitRadius = 80f;
        public const float OrbitSpeed = 0.04f;
        private float HomingSpeed = 14f;
        private float HomingStrength = 0.08f;

        private int LaunchTime = 12;
        private float LaunchSpeed = 12f;

        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];

        public override void SetBardDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.timeLeft = (60 * 10);
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.hostile = false;

            Projectile.ignoreWater = false;
            Projectile.localNPCHitCooldown = 40;
            Projectile.usesLocalNPCImmunity = true;

            Projectile.extraUpdates = 2;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 300);
        }

        public override void AI()
        {
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }

            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.ai[2] == 0f)
            {
                UpdateOrbit(player);

                Projectile.rotation =
                    Projectile.Center.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2;
            }
            else if (Projectile.ai[2] == 2f)
            {
                UpdateOrbit(player);

                Projectile.ai[2] = 1f;

                Projectile.rotation =
                    Projectile.Center.DirectionTo(player.Center).ToRotation() - MathHelper.PiOver2;

                Projectile.netUpdate = true;
            }
            else
            {
                UpdateHoming();

                Projectile.rotation =
                    Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            }
        }

        private void UpdateOrbit(Player player)
        {
            int orbitCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (!projectile.active ||
                    projectile.type != Type ||
                    projectile.owner != Projectile.owner ||
                    projectile.ai[2] != 0f)
                    continue;

                orbitCount++;
            }

            if (orbitCount <= 0)
                return;

            int orbitIndex = GetOrbitIndex();

            float angleStep = MathHelper.TwoPi / orbitCount;

            float angle =
                Main.GameUpdateCount * OrbitSpeed +
                orbitIndex * angleStep;

            Vector2 targetPosition =
                player.Center +
                angle.ToRotationVector2() * OrbitRadius;

            Projectile.Center = Vector2.Lerp(
                Projectile.Center,
                targetPosition,
                0.25f
            );

            Projectile.velocity = Vector2.Zero;
        }

        private int GetOrbitIndex()
        {
            int index = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (!projectile.active ||
                    projectile.type != Type ||
                    projectile.owner != Projectile.owner ||
                    projectile.ai[2] != 0f)
                    continue;

                if (projectile.whoAmI == Projectile.whoAmI)
                    return index;

                index++;
            }

            return 0;
        }

        private void UpdateHoming()
        {
            Player player = Main.player[Projectile.owner];

            // Briefly release outward from the player.
            if (Projectile.localAI[0] < LaunchTime)
            {
                Projectile.localAI[0]++;

                Vector2 outwardDirection =
                    Projectile.Center - player.Center;

                if (outwardDirection != Vector2.Zero)
                {
                    outwardDirection.Normalize();
                    Projectile.velocity = outwardDirection * LaunchSpeed;
                }

                return;
            }

            // Begin homing after the initial release.
            NPC target = FindTarget();

            if (target == null)
                return;

            Vector2 desiredVelocity =
                Projectile.Center.DirectionTo(target.Center) * HomingSpeed;

            Projectile.velocity = Vector2.Lerp(
                Projectile.velocity,
                desiredVelocity,
                HomingStrength
            );
        }

        private NPC FindTarget()
        {
            NPC closestTarget = null;
            float closestDistance = 1000f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.CanBeChasedBy())
                    continue;

                float distance = Vector2.Distance(
                    Projectile.Center,
                    npc.Center
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = npc;
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = tex.Height / 2;

            int frame = (int)Projectile.ai[0];

            Rectangle sourceRectangle = new Rectangle(
                0,
                frame * frameHeight,
                tex.Width,
                frameHeight
            );

            Vector2 origin = new Vector2(
                tex.Width / 2f,
                frameHeight / 2f
            );

            for (int i = TrailLength - 1; i >= 0; i--)
            {
                if (oldPos[i] == Vector2.Zero)
                    continue;

                float alpha = ((float)(TrailLength - i) / TrailLength) * 0.5f;
                Color drawColor = Color.White * alpha;

                Main.spriteBatch.Draw(
                    tex,
                    oldPos[i] - Main.screenPosition,
                    sourceRectangle,
                    drawColor,
                    oldRot[i],
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition,
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }
}