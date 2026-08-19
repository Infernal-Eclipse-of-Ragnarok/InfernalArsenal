using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static CatalystMod.Effects.Begin;
using static Daybreak.Common.Features.Hooks.GlobalItemHooks;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro
{
    public class ArckaneStaffHoldout : ModProjectile
    {
        public ref float HoldTimer => ref Projectile.ai[0];
        public ref float ShootTimer => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 62;
            Projectile.aiStyle = ProjAIStyleID.HeldProjectile;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Magic;
            DrawOffsetX = 20;
            DrawOriginOffsetY = 16;

        }

        public override bool? CanDamage() => false;
        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }
        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);

            HoldTimer += 1f;
            --Projectile.soundDelay;

            if (Projectile.soundDelay <= 0)
            {
                if (HoldTimer >= 25f)
                {
                    SoundEngine.PlaySound(SoundID.Item43, Projectile.position);
                    Projectile.soundDelay = 50;
                }
            }

            Projectile.frameCounter += 1;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
            }

            Projectile.timeLeft = 60;
            player.SetDummyItemTime(25);

            ShootTimer += 1f;
            bool shouldShootArrow = false;
            int initialShootDelay = 24;
            int shootDelayAdjustmentRate = 6;

            if (ShootTimer >= initialShootDelay - shootDelayAdjustmentRate)
            {
                ShootTimer = 0f;
                shouldShootArrow = true;
                Main.LocalPlayer.CheckMana(Main.LocalPlayer.HeldItem, -1, true, false);
            }
            else if (ShootTimer <= 1 && HoldTimer <=1)
            {
                shouldShootArrow = true;
            }

            if (shouldShootArrow && Main.myPlayer == Projectile.owner)
            {
                if (player.channel)
                {
                    float holdoutDistance = ArckaneStaff.HoldoutDistance * Projectile.scale;
                    Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);
                    if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y)
                    {
                        Projectile.netUpdate = true;
                    }
                    Projectile.velocity = holdoutOffset;

                    for (int j = 0; j < 1; j++)
                    {
                        var spawnLocation = playerCenter + holdoutOffset + Main.rand.NextVector2Circular(6, 6);
                        var source = player.GetSource_ItemUse(player.HeldItem);
                        var projectileA = Projectile.NewProjectile(source, spawnLocation, Vector2.Normalize(Projectile.velocity) * (float)Math.Exp(2.5), ModContent.ProjectileType<ArckaneGemPro>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                        Projectile.NewProjectile(source, spawnLocation, Vector2.Normalize(Projectile.velocity) * (float)Math.Exp(2.5), ModContent.ProjectileType<ArckaneScythe1Pro>(), 50, Projectile.knockBack, Projectile.owner, ai2: projectileA);
                        Projectile.NewProjectile(source, spawnLocation, Vector2.Normalize(Projectile.velocity) * (float)Math.Exp(2.5), ModContent.ProjectileType<ArckaneScythe2Pro>(), 50, Projectile.knockBack, Projectile.owner, ai2: projectileA);
                    }
                }
                else
                {
                    Projectile.Kill();
                    return false;
                }
            }
            if (Main.LocalPlayer.CheckMana(Main.LocalPlayer.HeldItem, -1) == false && HoldTimer >= 15)
            {
                Projectile.Kill();
                return false;
            }

            if (Projectile.velocity.X > 0f)
            {
                player.ChangeDir(1);
            }
            else if (Projectile.velocity.X < 0f)
            {
                player.ChangeDir(-1);
            }

            Projectile.spriteDirection = Projectile.direction;
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = playerCenter + Projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2) * 20;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < ArckaneGemPro.dusts.Length; i++)
                    {
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ArckaneGemPro.dusts[i], 0, 0, 100, default, 2f).noGravity = true;
                    }
                }

            }
        }
    }
}
