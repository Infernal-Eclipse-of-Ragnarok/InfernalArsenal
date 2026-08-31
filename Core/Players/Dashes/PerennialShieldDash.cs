using System;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace InfernalEclipseWeaponsDLC.Core.Players.Dashes
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class PerennialShieldDash : PlayerDashEffect
    {
        public static new string ID { get; private set; }

        public override DashCollisionType CollisionType => DashCollisionType.ShieldSlam;

        public override bool IsOmnidirectional => false;

        public override void Load()
        {
            ID = DashID;
        }

        public override float CalculateDashSpeed(Player player) => 16.9f;

        public override void DashStartupEffects(Player player)
        {
            player.velocity *= 0.9f;
        }

        public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
        {
            if (DashTimeAdjustedForStartup <= 12)
                for (int d = 0; d < 3; d++)
                {
                    Dust toxicDust = Dust.NewDustPerfect(
                        player.Center + new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-15f, 15f)) - (player.velocity * 1.7f),
                        DustID.Poisoned, // <-- changed here
                        -player.velocity.RotatedByRandom(MathHelper.ToRadians(10f)) * Main.rand.NextFloat(0.1f, 0.8f),
                        0,
                        default,
                        Main.rand.NextFloat(0.6f, 0.8f)
                    );
                    toxicDust.shader = GameShaders.Armor.GetSecondaryShader(player.cShield, player);
                    toxicDust.noGravity = true;
                    toxicDust.fadeIn = 0.5f;
                }

            // Dash at a faster speed than the default value.
            dashSpeed = 12.5f;
        }

        public override void OnHitEffects(Player player, NPC npc, IEntitySource source, ref DashHitContext hitContext)
        {
            if (DashTimeAdjustedForStartup > 12)
                return;
            // Define hit context variables.
            int hitDirection = player.direction;
            if (player.velocity.X != 0f)
                hitDirection = Math.Sign(player.velocity.X);
            hitContext.HitDirection = hitDirection;
            hitContext.PlayerImmunityFrames = OrnateShield.ShieldSlamIFrames;

            // Define damage parameters.
            hitContext.damageClass = DamageClass.Melee;
            hitContext.BaseDamage = 75;
            hitContext.BaseKnockback = 4;

            npc.AddBuff(BuffID.Venom, 180); //Great, Frostbite is ACTUALLY called Frostburn 2
        }
    }
}
