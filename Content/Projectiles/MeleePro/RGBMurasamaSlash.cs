using CalamityMod.Particles;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee;
using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro
{
    public class RGBMurasamaSlash : ModProjectile
    {
        private static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;
        public override LocalizedText DisplayName => ItemLoader.GetItem(ModContent.ItemType<RGBMurasama>()).GetLocalization("DisplayName");
        private Player Owner => Main.player[Projectile.owner];
        public ref int hitCooldown => ref Main.player[Projectile.owner].GetModPlayer<InfernalWeaponsPlayer>().murasamaHitCooldown;
        public int time = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 14;
        }

        public bool Slashing = false;
        public bool Slash1 => Projectile.frame == 10;
        public bool Slash2 => Projectile.frame == 0;
        public bool Slash3 => Projectile.frame == 6;

        public override void SetDefaults()
        {
            Projectile.width = 216;
            Projectile.height = 216;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = Calamity != null ? Calamity.Find<DamageClass>("TrueMeleeNoSpeedDamageClass") : DamageClass.Melee;
            Projectile.ContinuouslyUpdateDamageStats = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 6;
            Projectile.frameCounter = 0;
            Projectile.alpha = 255;
            Projectile.noEnchantmentVisuals = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.frameCounter <= 1)
                return false;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            SpriteEffects spriteEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + (Projectile.velocity * 0.3f) + new Vector2(0, -32).RotatedBy(Projectile.rotation), frame, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Projectiles/MeleePro/RGBMurasamaSlash_Glow").Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + (Projectile.velocity * 0.3f) + new Vector2(0, -32).RotatedBy(Projectile.rotation), frame, Color.Lerp(Main.DiscoColor, Color.White, 0.25f), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            return false;
        }

        public override void AI()
        {
            if (time == 0)
            {
                float baseScale = 1.25f;
                Owner.ApplyMeleeScale(ref baseScale);
                baseScale += Owner.HeldItem.scale - 1f;
                if (Owner.HasBuff(BuffID.Tipsy))
                    baseScale += 0.25f;
                if (Calamity != null)
                {
                    if (CalamityHelper.HasMeleeScaleIncreaes(Owner))
                        baseScale += 0.25f;
                }
                Projectile.scale = baseScale;

                if (Main.zenithWorld)
                {
                    Projectile.scale *= 2;
                    Projectile.damage = (int)(Projectile.damage * 2);
                }
                Projectile.frame = Main.zenithWorld ? 6 : 10;
                Projectile.alpha = 0;
                time++;
            }
            Player player = Main.player[Projectile.owner];
            if (Slash2)
            {
                SoundEngine.PlaySound(RGBMurasama.Swing with { Pitch = -0.1f }, Projectile.Center);
                if (hitCooldown == 0)
                    Slashing = true;
                Projectile.numHits = 0;
            }
            else if (Slash3)
            {
                SoundEngine.PlaySound(RGBMurasama.BigSwing with { Pitch = 0f }, Projectile.Center);
                if (hitCooldown == 0)
                    Slashing = true;
                Projectile.numHits = 0;
            }
            else if (Slash1)
            {
                SoundEngine.PlaySound(RGBMurasama.Swing with { Pitch = -0.05f }, Projectile.Center);
                if (hitCooldown == 0)
                    Slashing = true;
                Projectile.numHits = 0;
            }
            else
                Slashing = false;

            //Frames and crap
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 3 == 0)
            {
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
            }

            Vector2 origin = Projectile.Center + Projectile.velocity * 3f;
            Lighting.AddLight(origin, Color.Red.ToVector3() * (Slashing == true ? 3.5f : 2f));

            Vector2 playerRotatedPoint = player.RotatedRelativePoint(player.MountedCenter, true);
            if (Main.myPlayer == Projectile.owner)
            {
                if (!(player == null || !player.active || player.dead || !player.channel || player.CCed || player.noItems))
                    HandleChannelMovement(player, playerRotatedPoint);
                else
                {
                    hitCooldown = Main.zenithWorld ? 0 : 8;
                    Projectile.Kill();
                }
            }

            // Rotation and directioning.
            if (Slashing || Slash1)
            {
                float velocityAngle = Projectile.velocity.ToRotation();
                Projectile.rotation = velocityAngle + (Projectile.direction == -1).ToInt() * MathHelper.Pi;
            }
            float velocityAngle2 = Projectile.velocity.ToRotation();
            Projectile.direction = (Math.Cos(velocityAngle2) > 0).ToDirectionInt();

            // Positioning close to the end of the player's arm.
            float offset = 80f * Projectile.scale;
            Projectile.Center = playerRotatedPoint + velocityAngle2.ToRotationVector2() * offset;

            // Sprite and player directioning.
            player.ChangeDir(Projectile.direction);

            // Prevents the projectile from dying
            Projectile.timeLeft = 2;

            // Player item-based field manipulation.
            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
        }

        public void HandleChannelMovement(Player player, Vector2 playerRotatedPoint)
        {
            float speed = 1f;
            if (player.HeldItem.shoot == Projectile.type)
            {
                speed = player.HeldItem.shootSpeed * Projectile.scale;
            }
            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Vector2 newVelocity = (Main.MouseWorld - playerRotatedPoint).SafeNormalize(Vector2.UnitX * player.direction) * speed;

            // Sync if a velocity component changes.
            if (Slashing)
            {
                if (Projectile.velocity.X != newVelocity.X || Projectile.velocity.Y != newVelocity.Y)
                {
                    Projectile.netUpdate = true;
                }
                Projectile.velocity = newVelocity;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 lineEnd = Main.player[Projectile.owner].Center + Projectile.velocity * (Slash3 ? 11.35f : 8.6f);
            // The slash sprite is top-heavy, so make the width lean in a direction
            float lineWidth = (Projectile.direction == 1 && projHitbox.Center.X > targetHitbox.Center.X) || (Projectile.direction == -1 && projHitbox.Center.X < targetHitbox.Center.X) ? 320f : 200f;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Main.player[Projectile.owner].Center, lineEnd, lineWidth, ref _);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Slash3)
                modifiers.SourceDamage *= 2f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.HitSound != SoundID.NPCHit4 && target.HitSound != SoundID.NPCHit41 && target.HitSound != SoundID.NPCHit2 &&
                target.HitSound != SoundID.NPCHit5 && target.HitSound != SoundID.NPCHit11 && target.HitSound != SoundID.NPCHit30 &&
                target.HitSound != SoundID.NPCHit34 && target.HitSound != SoundID.NPCHit36 && target.HitSound != SoundID.NPCHit42 &&
                target.HitSound != SoundID.NPCHit49 && target.HitSound != SoundID.NPCHit52 && target.HitSound != SoundID.NPCHit53 &&
                target.HitSound != SoundID.NPCHit54 && target.HitSound != null)
                SoundEngine.PlaySound(RGBMurasama.OrganicHit with { Pitch = (Slash2 ? -0.1f : Slash3 ? 0.1f : Slash1 ? -0.15f : 0) }, Projectile.Center);
            else
                SoundEngine.PlaySound(RGBMurasama.InorganicHit with { Pitch = (Slash2 ? -0.1f : Slash3 ? 0.1f : Slash1 ? -0.15f : 0) }, Projectile.Center);

            if (Calamity != null)
            {
                HandleMurasamaWeaponSparkles.Spark(target, Owner, Projectile, Slash1, Slash2, Slash3, true);
            }
           
        }
        public override Color? GetAlpha(Color lightColor) => new Color(100, 0, 0, 0);

        public override bool? CanDamage() => Slashing ? null : false;
    }

    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public static class HandleMurasamaWeaponSparkles
    {
        public static void Spark(NPC target, Player Owner, Projectile Projectile, bool Slash1, bool Slash2, bool Slash3, bool isRGB = false)
        {
            for (int i = 0; i < 3; i++)
            {
                Color impactColor = isRGB ? Main.DiscoColor : Slash3 ? Main.rand.NextBool(3) ? Color.LightCoral : Color.White : Main.rand.NextBool(4) ? Color.LightCoral : Color.Crimson;
                float impactParticleScale = Main.rand.NextFloat(1f, 1.75f);

                if (Slash3)
                {
                    SparkleParticle impactParticle2 = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(target.width * 0.75f, target.height * 0.75f), Vector2.Zero, Color.White, isRGB ? Main.DiscoColor : Color.Red, impactParticleScale * 1.2f, 8, 0, 4.5f);
                    GeneralParticleHandler.SpawnParticle(impactParticle2);
                }
                SparkleParticle impactParticle = new SparkleParticle(target.Center + Main.rand.NextVector2Circular(target.width * 0.75f, target.height * 0.75f), Vector2.Zero, impactColor, isRGB ? Main.DiscoColor : Color.Red, impactParticleScale, 8, 0, 2.5f);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

            float sparkCount = MathHelper.Clamp(Slash3 ? 18 - Projectile.numHits * 3 : 5 - Projectile.numHits * 2, 0, 18);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVelocity2 = Projectile.velocity.RotatedBy(Slash2 ? -0.45f * Owner.direction : Slash3 ? 0 : Slash1 ? 0.45f * Owner.direction : 0).RotatedByRandom(0.35f) * Main.rand.NextFloat(0.5f, 1.8f);
                int sparkLifetime2 = Main.rand.Next(23, 35);
                float sparkScale2 = Main.rand.NextFloat(0.95f, 1.8f);
                Color sparkColor2 = isRGB ? Main.DiscoColor : Slash3 ? Main.rand.NextBool(3) ? Color.Red : Color.IndianRed : Main.rand.NextBool() ? Color.Red : Color.Firebrick;
                if (Main.rand.NextBool())
                {
                    AltSparkParticle spark = new AltSparkParticle(target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f) + Projectile.velocity * 1.2f, sparkVelocity2 * (Slash3 ? 1f : 0.65f), false, (int)(sparkLifetime2 * (Slash3 ? 1.2f : 1f)), sparkScale2 * (Slash3 ? 1.4f : 1f), sparkColor2);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
                else
                {
                    LineParticle spark = new LineParticle(target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f) + Projectile.velocity * 1.2f, sparkVelocity2 * (Projectile.frame == 7 ? 1f : 0.65f), false, (int)(sparkLifetime2 * (Projectile.frame == 7 ? 1.2f : 1f)), sparkScale2 * (Projectile.frame == 7 ? 1.4f : 1f), isRGB ? Main.DiscoColor : Main.rand.NextBool() ? Color.Red : Color.Firebrick);
                    GeneralParticleHandler.SpawnParticle(spark);
                }
            }
            float dustCount = MathHelper.Clamp(Slash3 ? 25 - Projectile.numHits * 3 : 12 - Projectile.numHits * 2, 0, 25);
            for (int i = 0; i <= dustCount; i++)
            {
                int dustID = Main.rand.NextBool(3) ? 182 : Main.rand.NextBool() ? Slash3 ? 309 : 296 : 90;
                Dust dust2 = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width * 0.5f, target.height * 0.5f), dustID, Projectile.velocity.RotatedBy(Slash2 ? -0.45f * Owner.direction : Slash3 ? 0 : Slash1 ? 0.45f * Owner.direction : 0).RotatedByRandom(0.55f) * Main.rand.NextFloat(0.3f, 1.1f), newColor: isRGB ? Main.DiscoColor : default);
                dust2.scale = Main.rand.NextFloat(0.9f, 2.4f);
                dust2.noGravity = true;
            }
        }
    }
}
