using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.WhipPro.DNAWhip
{
    public class DNAWhipPro : ModProjectile
    {
        private int SegmentCount = 20;

        private const int DebuffDuration = 300;

        private const float TipDamageMultiplier = 1.5f;
        private const float MultihitModifier = 0.8f;

        private readonly List<Vector2> whipPoints = new();

        private int lastHitSegment = -1;

        private enum SegmentEffect
        {
            Normal,
            Tag,
            Armor,
            Poison,
            Tip
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            Projectile.width = 40;
            Projectile.height = 40;

            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            Projectile.WhipSettings.Segments = SegmentCount;
            Projectile.WhipSettings.RangeMultiplier = 16f / 32f;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }

        #region Whip Points
        private void UpdateWhipPoints()
        {
            whipPoints.Clear();

            Projectile.FillWhipControlPoints(Projectile, whipPoints);
        }
        private Vector2 GetTipPosition() =>whipPoints.Count > 0 ? whipPoints[^1] : Projectile.Center;

        private SegmentEffect GetSegmentEffect(int index)
        {
            // Always use the final segment as the tip.
            int lastSegment = whipPoints.Count - 2;

            if (index == lastSegment)
                return SegmentEffect.Tip;

            return index switch
            {
                3 => SegmentEffect.Tag,
                4 => SegmentEffect.Tag,
                5 => SegmentEffect.Tag,
                
                9 => SegmentEffect.Armor,
                10 => SegmentEffect.Armor,
                11 => SegmentEffect.Armor,

                14 => SegmentEffect.Poison,
                15 => SegmentEffect.Poison,
                16 => SegmentEffect.Poison,

                _ => SegmentEffect.Normal
            };
        }
        #endregion

        #region AI
        public override bool PreAI()
        {
            UpdateWhipPoints();
            return true;
        }

        public override void PostAI()
        {
            // Refresh the points after vanilla whip AI updates.
            UpdateWhipPoints();

            if (Main.dedServ || whipPoints.Count < 2)
                return;

            for (int i = 0; i < whipPoints.Count - 1; i++)
            {
                Vector3 lightColor = GetSegmentEffect(i) switch
                {
                    SegmentEffect.Tag => new Vector3(0.65f, 0.08f, 0.08f),
                    SegmentEffect.Armor => new Vector3(0.08f, 0.25f, 0.75f),
                    SegmentEffect.Poison => new Vector3(0.08f, 0.65f, 0.12f),
                    _ => Vector3.Zero
                };

                if (lightColor != Vector3.Zero)
                {
                    Lighting.AddLight(whipPoints[i], lightColor);
                }
            }

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out _, out _);

            // Play the whip crack near maximum extension.
            if (Projectile.ai[0] == (int)(timeToFlyOut * 0.5f))
            {
                SoundEngine.PlaySound(SoundID.Item153, GetTipPosition());
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 tip = GetTipPosition();

                Dust dust = Dust.NewDustDirect(tip, 2, 2, DustID.Smoke);

                dust.noGravity = true;
                dust.scale = 0.5f;
                dust.velocity *= 0.5f;
            }
        }
        #endregion

        #region Collision
        private int FindCollidingSegment(Rectangle targetHitbox)
        {
            UpdateWhipPoints();

            if (whipPoints.Count < 2)
                return -1;

            int selectedSegment = -1;
            float closestDistance = float.MaxValue;

            Vector2 targetCenter = targetHitbox.Center.ToVector2();

            for (int i = 0; i < whipPoints.Count - 1; i++)
            {
                Vector2 start = whipPoints[i];
                Vector2 end = whipPoints[i + 1];

                SegmentEffect effect = GetSegmentEffect(i);

                float collisionWidth = effect switch
                {
                    SegmentEffect.Tag => 16f,
                    SegmentEffect.Armor => 18f,
                    SegmentEffect.Poison => 16f,
                    SegmentEffect.Tip => 14f,
                    _ => 10f
                };

                float collisionPoint = 0f;

                if (!Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, collisionWidth, ref collisionPoint))
                    continue;

                Vector2 center = (start + end) * 0.5f;

                float distance = Vector2.DistanceSquared(targetCenter, center);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    selectedSegment = i;
                }
            }

            return selectedSegment;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => FindCollidingSegment(targetHitbox) >= 0;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            lastHitSegment = FindCollidingSegment(target.Hitbox);

            if (lastHitSegment < 0)
                return;

            if (GetSegmentEffect(lastHitSegment) == SegmentEffect.Tip)
            {
                modifiers.SourceDamage *= TipDamageMultiplier;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int segment = lastHitSegment;

            lastHitSegment = -1;

            if (segment < 0)
                segment = FindCollidingSegment(target.Hitbox);

            SegmentEffect effect = segment >= 0 ? GetSegmentEffect(segment) : SegmentEffect.Normal;

            switch (effect)
            {
                case SegmentEffect.Tag:
                    target.AddBuff(ModContent.BuffType<DeoxyribonunobokoWhipTagStrong>(), DebuffDuration);

                    Player player = Main.player[Projectile.owner];

                    float healAmount = damageDone / 20;
                    if (healAmount > player.lifeSteal)
                    {
                        healAmount = player.lifeSteal;
                    }
                    if (player.lifeSteal < 0)
                    {
                        healAmount += player.lifeSteal;

                        if (healAmount <= 0)
                        {
                            break;
                        }

                        if (healAmount > player.statLifeMax2 - player.statLife)
                        {
                            healAmount = player.statLifeMax2 - player.statLife;
                        }
                    }

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ProjectileID.VampireHeal, 0, 0f, Projectile.owner, player.whoAmI, healAmount, 0f);
                    player.lifeSteal -= healAmount;

                    break;

                case SegmentEffect.Armor:
                    if (ModLoader.TryGetMod("CalamityMod", out Mod calamity) && calamity.TryFind("ArmorCrunch", out ModBuff armorCrunch))
                    {
                        target.AddBuff(armorCrunch.Type, DebuffDuration);
                    }
                    else
                    {
                        target.AddBuff(BuffID.BrokenArmor, DebuffDuration);
                    }
                    target.AddBuff(ModContent.BuffType<DeoxyribonunobokoWhipTagDefault>(), DebuffDuration);
                    break;

                case SegmentEffect.Poison:
                    target.AddBuff(BuffID.Venom, DebuffDuration);
                    target.AddBuff(ModContent.BuffType<DeoxyribonunobokoWhipTagDefault>(), DebuffDuration);
                    break;

                default:
                    target.AddBuff(ModContent.BuffType<DeoxyribonunobokoWhipTagDefault>(), DebuffDuration);
                    break;
            }

            Projectile.damage = Math.Max(1, (int)(Projectile.damage * MultihitModifier));

            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

            if (!Main.dedServ)
                SpawnHitDust(target.Center, effect);
        }
        #endregion

        private static void SpawnHitDust(Vector2 position, SegmentEffect effect)
        {
            int dustType = effect switch
            {
                SegmentEffect.Tag => DustID.LifeDrain,
                SegmentEffect.Armor => DustID.Silver,
                SegmentEffect.Poison => DustID.Poisoned,
                _ => DustID.Smoke
            };

            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = new Vector2(2f, 0f).RotatedBy(MathHelper.TwoPi * i / 8f);

                Dust dust = Dust.NewDustDirect(position, 0, 0, dustType);

                dust.noGravity = true;
                dust.scale = 0.7f;
                dust.velocity = velocity * 1.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> points = new();
            Projectile.FillWhipControlPoints(Projectile, points);

            if (points.Count < 2)
                return false;

            const string assetPath = "InfernalEclipseWeaponsDLC/Content/Projectiles/SummonerPro/WhipPro/DNAWhip/";

            // Request textures directly, without caching them ourselves.
            Texture2D handle = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D normal = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegment", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D tag = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentTag", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D tagGlow = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentTag_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D armor = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentArmor", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D armorGlow = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentArmor_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D poison = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentPoison", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D poisonGlow = ModContent.Request<Texture2D>(assetPath + "DNAWhipSegmentPoison_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D tip = ModContent.Request<Texture2D>(assetPath + "DNAWhipTip", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int segmentCount = points.Count - 1;
            int lastSegment = segmentCount - 1;

            // Draw all whip sprites directly at their control points.
            Vector2 drawPosition = points[0];
            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 change = points[i + 1] - points[i];

                Texture2D segmentTexture;
                Texture2D glowTexture = null;

                // Tip always takes priority and is always the last segment.
                if (i == lastSegment)
                {
                    segmentTexture = tip;
                }
                else if (i == 0)
                {
                    segmentTexture = handle;
                }
                else if (i == 3 || i == 4 || 1 == 5)
                {
                    segmentTexture = tag;
                    glowTexture = tagGlow;
                }
                else if (i == 9 || i == 10 || 1 == 11)
                {
                    segmentTexture = armor;
                    glowTexture = armorGlow;
                }
                else if (i == 14 || i == 15 || 1 == 16)
                {
                    segmentTexture = poison;
                    glowTexture = poisonGlow;
                }
                else
                {
                    segmentTexture = normal;
                }

                // The supplied DNA sprites are oriented horizontally.
                float rotation = change.ToRotation();

                float scale = 1f;

                // Draw each segment at its control point
                Vector2 origin = segmentTexture.Size() * 0.5f;

                // Get Terraria's lighting at this segment's position.
                Color segmentLighting = Lighting.GetColor(
                    drawPosition.ToTileCoordinates()
                );

                // Draw the entire segment with normal world lighting.
                Main.EntitySpriteDraw(
                    segmentTexture,
                    drawPosition - Main.screenPosition,
                    null,
                    segmentLighting,
                    rotation,
                    origin,
                    scale,
                    spriteEffects,
                    0f
                );

                // Only the three special segments have glow textures.
                if (glowTexture != null)
                {
                    Main.EntitySpriteDraw(
                        glowTexture,
                        drawPosition - Main.screenPosition,
                        null,
                        Color.White,
                        rotation,
                        glowTexture.Size() * 0.5f,
                        scale,
                        spriteEffects,
                        0f
                    );
                }

                drawPosition += change;
            }

            return false;
        }
    }
}