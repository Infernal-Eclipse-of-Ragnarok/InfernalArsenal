using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NecrooticChorus
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class NecroticChorusWisp : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public sealed override void SetBardDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 26;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.timeLeft = 300;
            Projectile.Opacity = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public int TargetNPC
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public bool Despawn
        {
            get => Projectile.ai[2] > 0f;
            set => Projectile.ai[2] = value ? 1 : -1;
        }

        public Player Player => Main.player[Projectile.owner];

        public override bool? CanCutTiles() => false;

        // The AI of this minion is split into multiple methods to avoid bloat. This method just passes values between calls actual parts of the AI.
        public override void AI()
        {
            Projectile.knockBack = 0f;
            Player owner = Main.player[Projectile.owner];

            if (CheckActive(owner))
            {
                GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
                SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
                Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
            }

            Visuals();
        }

        private bool CheckActive(Player owner)
        {
            if (Despawn)
            {
                Projectile.Opacity -= 0.05f;
                return false;
            }

            if (Projectile.Opacity < 1f)
                Projectile.Opacity += 0.1f;

            /*
            if (Projectile.owner == Main.myPlayer)
            {
                bool chorusEquipped = owner.HeldItem.type == ModContent.ItemType<NecroticChorus>() || Main.mouseItem.type == ModContent.ItemType<NecroticChorus>();
                if (chorusEquipped)
                {
                    Projectile.timeLeft = 300;
                }
                else
                {
                    
                }
            }
            */

            if (owner.dead || !owner.active || Projectile.timeLeft <= 1)
            {
                Despawn = true;
                Projectile.timeLeft = 20;
                Projectile.netUpdate = true;
            }

            return true;
        }

        public void RefreshLifetime()
        {
            Projectile.timeLeft = 300;
            Despawn = false;
            Projectile.Opacity = 1f;
            Projectile.netUpdate = true;
        }

        private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
        {
            int index = Main.projectile.Where(p => p.type == Type).Select(p => p.whoAmI).Order().ToList().IndexOf(Projectile.whoAmI);
            Vector2 idlePosition = owner.Center;
            idlePosition.Y -= 48f;
            float minionPositionOffsetX = (10 + index * 40) * -owner.direction;
            idlePosition.X += minionPositionOffsetX;
            vectorToIdlePosition = idlePosition - Projectile.Center;
            distanceToIdlePosition = vectorToIdlePosition.Length();

            if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
            {
                Projectile.position = idlePosition;
                Projectile.velocity *= 0.1f;
                Projectile.netUpdate = true;
            }

            float overlapVelocity = 0.04f;
            foreach (var other in Main.ActiveProjectiles)
            {
                if (other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
                {
                    if (Projectile.position.X < other.position.X)
                    {
                        Projectile.velocity.X -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.X += overlapVelocity;
                    }

                    if (Projectile.position.Y < other.position.Y)
                    {
                        Projectile.velocity.Y -= overlapVelocity;
                    }
                    else
                    {
                        Projectile.velocity.Y += overlapVelocity;
                    }
                }
            }
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = 700f;
            targetCenter = Projectile.position;
            foundTarget = false;

            if (TargetNPC > 0 && TargetNPC < Main.maxNPCs)
            {
                NPC npc = Main.npc[TargetNPC];

                if (!npc.active)
                {
                    TargetNPC = -1;
                }
                else
                {
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    if (between < 2000f)
                    {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }
            else
            {
                TargetNPC = -1;
            }

            if (!foundTarget)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy(Projectile))
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        bool closeThroughWall = between < 100f;

                        if ((closest && inRange || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            Projectile.friendly = foundTarget;
        }

        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
        {
            float speed = 30f;
            float inertia = 4f;

            if (foundTarget)
            {
                if (distanceFromTarget > 40f)
                {
                    Vector2 direction = targetCenter - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
            }
            else
            {
                if (distanceToIdlePosition > 600f)
                {
                    speed = 12f;
                    inertia = 60f;
                }
                else
                {
                    speed = 4f;
                    inertia = 80f;
                }

                if (distanceToIdlePosition > 20f)
                {
                    vectorToIdlePosition.Normalize();
                    vectorToIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
                }
                else if (Projectile.velocity == Vector2.Zero)
                {
                    Projectile.velocity.X = -0.15f;
                    Projectile.velocity.Y = -0.05f;
                }
            }
        }

        private void Visuals()
        {
            Projectile.rotation = Projectile.velocity.X * 0.05f;

            int frameSpeed = 5;
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }

            if (Main.rand.NextBool(10))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Scale: 1.5f);
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDustDirect(target.position, target.width, target.height, DustID.IceTorch);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texture = TextureAssets.Projectile[Type];
            for (int k = 1; k < 7; k++)
            {
                int frameY = (Projectile.frame + k) % Main.projFrames[Type];
                Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: frameY);
                ulong seed = (ulong)(Projectile.whoAmI + k);
                float offsetX = Utils.RandomInt(ref seed, -5, 6) * 0.15f;
                float offsetY = Utils.RandomInt(ref seed, -10, 1) * 0.35f;
                float scale = Projectile.scale - (k - 1) * 0.05f;
                Color color = Color.White * 0.25f * Projectile.Opacity;
                color.A = 0;
                Main.spriteBatch.Draw(texture.Value, Projectile.Center + new Vector2(offsetX, offsetY) - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
