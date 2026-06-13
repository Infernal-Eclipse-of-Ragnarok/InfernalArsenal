using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs.Healer;
using ThoriumMod.Items.HealerItems;
using CalamityMod.Buffs.DamageOverTime;
using InfernalEclipseWeaponsDLC.Utilities;
using System.Collections.Generic;
using Terraria.Enums;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.ExecutionersSword
{
    public class ExecutionersSwordSlashPro : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_982"; // Excalibur slash

        private HashSet<int> healedPlayers = new HashSet<int>(); // track who got healed

        private bool hasHealedThisSwing = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.ownerHitCheckDistance = 300f;
            Projectile.usesOwnerMeleeHitCD = true;
            Projectile.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
            Projectile.aiStyle = -1; // Custom AI for swinging
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player target = Main.player[i];
                if (target.active && !target.dead && CanHitPlayer(target))
                {
                    Rectangle playerHitbox = target.Hitbox;
                    if (Colliding(Projectile.Hitbox, playerHitbox) == true)
                    {
                        HealTeammateThorium(player, target, baseHeal: 0);
                    }
                }
            }

            // Track animation progress
            float progress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);
            float dir = player.direction;

            // Swing rotation like BigBertha
            float swingArc = MathHelper.Pi; // 180° swing
            float swingRotation = dir * (-swingArc / 2f + progress * swingArc);
            Projectile.rotation = swingRotation + player.fullRotation - MathHelper.PiOver2;

            // Anchor projectile to player
            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter, false, true);

            // Dynamic scale
            Projectile.scale = (player.GetAdjustedItemScale(player.HeldItem)) + progress * 0.3f;

            // Dust along swing (BigBertha style)
            if (Main.rand.NextFloat() < Projectile.Opacity * 0.5f)
            {
                // Original offset
                Vector2 dustOffset = Projectile.rotation.ToRotationVector2() * 120f * Projectile.scale;
                Vector2 dustPos = Projectile.Center + dustOffset; // no extra 90° rotation

                // Random velocity along swing direction
                Vector2 dustVel = Projectile.rotation.ToRotationVector2().RotatedByRandom(MathHelper.PiOver4) * 1.5f;

                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Enchanted_Gold, dustVel, 100, Color.White, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f + Main.rand.NextFloat() * 0.3f;
            }

            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            CutTiles();

            // Light at tip
            Lighting.AddLight(Projectile.Center, 0.8f, 0.7f, 0.2f);

            // Kill when swing is done
            if (player.itemAnimation <= 1)
            {
                healedPlayers.Clear();
                Projectile.Kill();
            }
        }


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float coneLength = 120f * Projectile.scale;
            float maxAngle = MathHelper.PiOver4;
            float coneRot = Projectile.rotation;

            return Utils.IntersectsConeSlowMoreAccurate(targetHitbox, Projectile.Center, coneLength, coneRot, maxAngle);
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;

            float swingLength = 120f * Projectile.scale;
            float step = MathHelper.ToRadians(4f); // 3°–5° works great
            float arcRange = MathHelper.ToRadians(20f); // total ± arc range to cover
            float cutWidth = Math.Max(Projectile.width * Projectile.scale * 1.4f, 24f);

            // Sweep a short arc ahead of current rotation
            for (float offset = -arcRange; offset <= arcRange; offset += step)
            {
                float rot = Projectile.rotation + offset;
                Vector2 lineStart = Projectile.Center - rot.ToRotationVector2() * (swingLength * 0.1f);
                Vector2 lineEnd = lineStart + rot.ToRotationVector2() * swingLength;

                Utils.PlotTileLine(
                    lineStart,
                    lineEnd,
                    cutWidth,
                    DelegateMethods.CutTiles
                );
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner]; // <-- Add this line

            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300); // 5 seconds

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(target.width / 2f, target.height / 2f);
                Vector2 spawnPos = target.Center + offset;
                Vector2 vel = offset.SafeNormalize(Vector2.UnitY) * 2f;

                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.Enchanted_Gold, vel, 150, Color.White, 1.5f);
                dust.noGravity = true;
            }

            if (!hasHealedThisSwing)
            {
                int healAmount = 5;
                player.statLife += healAmount;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;

                player.HealEffect(healAmount);

                hasHealedThisSwing = true; // mark that we've healed for this swing
            }
        }


        private void HealTeammateThorium(Player healer, Player target, int baseHeal)
        {
            if (healer.whoAmI != Main.myPlayer) return;
            if (healer == target) return;
            if (healer.team == 0 || healer.team != target.team) return;

            if (healedPlayers.Contains(target.whoAmI))
                return;

            if (baseHeal <= 0 && healer.GetModPlayer<ThoriumPlayer>().healBonus <= 0)
                return; // Nothing to heal

            HealerHelper.HealPlayer(
                healer,
                target,
                healAmount: baseHeal, // <-- don't add healBonus here
                recoveryTime: 60,
                healEffects: true,
                extraEffects: p => p.AddBuff(ModContent.BuffType<Cured>(), 30, true, false)
            );

            healedPlayers.Add(target.whoAmI);

            // Optional dust visuals
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(target.width / 2f, target.height / 2f);
                Dust.NewDustPerfect(target.Center + offset, DustID.Enchanted_Gold, Vector2.Zero, 150, Color.White, 1.2f).noGravity = true;
            }
        }


        public override bool CanHitPlayer(Player target)
        {
            Player owner = Main.player[Projectile.owner];
            // Only hit teammates (but not self)
            return owner.team != 0 && target.team == owner.team && target.whoAmI != owner.whoAmI;
        }


        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Player player = Main.player[Projectile.owner];

            // Only heal teammates, not self
            if (player.team != 0 && player.team == target.team && target.whoAmI != player.whoAmI)
            {
                HealTeammateThorium(player, target, baseHeal: 0); // base 0 + Thorium bonus
            }

            // Only apply debuff if this is a PvP hit
            bool isPvPHit = player.whoAmI != target.whoAmI    // Not self
                            && player.team != 0             // Teams are enabled
                            && player.team != target.team;  // Not same team

            if (isPvPHit)
            {
                target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);

                if (!hasHealedThisSwing)
                {
                    int healAmount = 5;
                    player.statLife += healAmount;
                    if (player.statLife > player.statLifeMax2)
                        player.statLife = player.statLifeMax2;

                    player.HealEffect(healAmount);

                    hasHealedThisSwing = true; // mark that we've healed for this swing
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(target.width / 2f, target.height / 2f);
                    Dust dust = Dust.NewDustPerfect(target.Center + offset, DustID.Enchanted_Gold, Vector2.Zero, 150, Color.White, 1.2f);
                    dust.noGravity = true;
                }
            }
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D slashTexture = TextureAssets.Projectile[982].Value;
            Texture2D tipTexture = TextureAssets.Extra[ExtrasID.SharpTears].Value;

            float progress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);
            float scale = Projectile.scale;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Slash origin: pivot at base
            Vector2 slashOrigin = new Vector2(slashTexture.Width / 2, slashTexture.Height / 8);

            // Tip origin: center
            Vector2 tipOrigin = tipTexture.Size() / 2f;

            // Mirror rotation for left swing
            float drawRotation = Projectile.rotation;
            //if (player.direction == -1)
            //    drawRotation = MathHelper.Pi - drawRotation;

            // Only flip horizontally if your art actually needs it
            SpriteEffects slashEffects = SpriteEffects.None;

            // Base slash
            Color baseColor = Color.Yellow * 0.6f * progress;
            Main.EntitySpriteDraw(slashTexture, drawPos, Utils.Frame(slashTexture, 1, 4, 0, 0),
                baseColor, drawRotation, slashOrigin, scale * 1.1f, slashEffects, 0f);

            // Glow overlay
            Color glow = Color.LightYellow * 0.5f * progress;
            Main.EntitySpriteDraw(slashTexture, drawPos, Utils.Frame(slashTexture, 1, 4, 0, 0),
                glow, drawRotation, slashOrigin, scale * 1.5f, slashEffects, 0f);

            // Tip sparkle with rotation offset
            float tipRotationOffset = MathHelper.ToRadians(30f);
            if (player.direction == -1) tipRotationOffset = -tipRotationOffset;
            Vector2 tipOffset = (Projectile.rotation + tipRotationOffset).ToRotationVector2() * 120f * scale;

            Main.EntitySpriteDraw(tipTexture, Projectile.Center + tipOffset - Main.screenPosition,
                null, Color.Gold * 0.7f * progress, 0f, tipOrigin, scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
