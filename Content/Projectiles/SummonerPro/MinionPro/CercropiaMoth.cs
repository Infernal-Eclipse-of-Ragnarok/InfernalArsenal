using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Summons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.MinionPro
{
    public class CercropiaMoth : ModProjectile
    {
        private const float TargetRange = 900f;
        private const float FollowRange = 1600f;

        private const float IdleSpeed = 14f;
        private const float ChaseSpeed = 24f;
        private const float DashSpeed = 13f;

        private const int HoverTime = 23;
        private const int DashTime = 12;
        private const int RecoveryTime = 15;

        private enum AttackState
        {
            Hover,
            Dash,
            Recovery
        }

        private ref float State => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];

        private NPC Target;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;

            Main.projPet[Type] = true;

            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 36;

            Projectile.minion = true;
            Projectile.minionSlots = 1f;

            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = true;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.netImportant = true;
            Projectile.timeLeft = 2;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanDamage()
        {
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active) 
                player.ClearBuff(ModContent.BuffType<CecropiaMothBuff>());

            if (player.HasBuff(ModContent.BuffType<CecropiaMothBuff>())) 
                Projectile.timeLeft = 2;

            FindTarget(player);

            if (Vector2.Distance(Projectile.Center, player.Center) > FollowRange)
            {
                Projectile.Center = player.Center;
                Projectile.velocity = Vector2.Zero;

                ChangeState(AttackState.Hover);
                Projectile.netUpdate = true;
            }

            if (Target == null)
            {
                IdleMovement(player);
            }
            else
            {
                AttackTarget();
            }

            UpdateVisuals();
        }

        private void FindTarget(Player player)
        {
            Target = null;

            float closestDistance = TargetRange;

            // Prioritize enemies selected with a summon whip.
            if (player.HasMinionAttackTargetNPC)
            {
                int index = player.MinionAttackTargetNPC;

                if (index >= 0 && index < Main.maxNPCs)
                {
                    NPC npc = Main.npc[index];

                    if (npc.CanBeChasedBy(Projectile) && Vector2.Distance(Projectile.Center, npc.Center) < TargetRange)
                    {
                        Target = npc;
                        return;
                    }
                }
            }

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile))
                    continue;

                float distance = Vector2.Distance(Projectile.Center, npc.Center);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    Target = npc;
                }
            }
        }

        private void IdleMovement(Player player)
        {
            if ((AttackState)(int)State != AttackState.Hover)
                ChangeState(AttackState.Hover);

            int minionIndex = 0;

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.owner != Projectile.owner || projectile.type != Type)
                    continue;

                if (projectile.whoAmI == Projectile.whoAmI)
                    break;

                minionIndex++;
            }

            float time = Main.GlobalTimeWrappedHourly;

            // Each moth follows a slightly different hovering pattern.
            float phase = minionIndex * 1.5f;

            float horizontalOffset = (float)Math.Sin(time * 1.5f + phase) * 90f;
            float verticalOffset = (float)Math.Cos(time * 2f + phase) * 35f;

            Vector2 idlePosition = player.Center + new Vector2(horizontalOffset, -85f + verticalOffset);

            MoveTowards(idlePosition, IdleSpeed, 25f);

            // Slow down when close to the desired hovering position.
            if (Vector2.DistanceSquared(Projectile.Center, idlePosition) < 20f * 20f)
                Projectile.velocity *= 0.95f;
        }

        private void AttackTarget()
        {
            Timer++;

            switch ((AttackState)(int)State)
            {
                case AttackState.Hover:
                    HoverAboveTarget();
                    break;

                case AttackState.Dash:
                    PerformDash();
                    break;

                case AttackState.Recovery:
                    RecoverFromDash();
                    break;
            }
        }

        private void HoverAboveTarget()
        {
            // Hover above and slightly to the side of the enemy.
            float side = Projectile.Center.X < Target.Center.X ? -1f : 1f;

            Vector2 hoverPosition = Target.Center + new Vector2(side * 180f, -130f);

            MoveTowards(hoverPosition, ChaseSpeed, 12f);

            if (Timer >= HoverTime)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 direction = (Target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);

                    Projectile.velocity = direction * DashSpeed;

                    ChangeState(AttackState.Dash);
                    Projectile.netUpdate = true;
                }
            }
        }

        private void PerformDash()
        {
            // Maintain the initial charge velocity.
            // Mothron-like attacks commit to their dash
            // instead of continuously homing.
            if (Timer >= DashTime)
            {
                ChangeState(AttackState.Recovery);
            }
        }

        private void RecoverFromDash()
        {
            Projectile.velocity *= 0.94f;

            if (Timer >= RecoveryTime)
            {
                ChangeState(AttackState.Hover);
            }
        }

        private void MoveTowards(Vector2 destination, float speed, float inertia)
        {
            Vector2 direction = destination - Projectile.Center;

            float distance = direction.Length();

            if (distance < 20f)
            {
                Projectile.velocity *= 0.9f;
                return;
            }

            Vector2 desiredVelocity = direction.SafeNormalize(Vector2.Zero) * Math.Min(speed, distance / 10f + 2f);

            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desiredVelocity) / inertia;
        }

        private void ChangeState(AttackState newState)
        {
            State = (float)newState;
            Timer = 0f;
            Projectile.netUpdate = true;
        }

        private void UpdateVisuals()
        {
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0f ? -1 : 1;
            }

            Projectile.rotation = MathHelper.Clamp(Projectile.velocity.X * 0.025f, -0.45f, 0.45f);

            Projectile.frameCounter++;

            int frameSpeed = (AttackState)(int)State == AttackState.Dash ? 4 : 6;

            if (Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;

                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
            }
        }
    }
}
