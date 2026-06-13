using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;
using CalamityMod.Graphics.Primitives;
using Terraria.Graphics.Shaders;
using CalamityMod.Enums;
using CalamityMod;
using CalamityMod.Particles;
using ReLogic.Content;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    public class DeathsWhisperPro : BardProjectile, IPixelatedPrimitiveRenderer
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.None}";

        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public bool IsAHomingFireball
        {
            get
            {
                return Projectile.ai[0] == 2f;
            }
            set
            {
                Projectile.ai[0] = Utils.ToInt(value);
            }
        }

        public override void SetBardDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.localAI[0]++;

            // Flame dust (tight, energetic)
            if (Main.rand.NextBool(12))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                    Vector2 vel = Projectile.velocity * -1.2f;

                    float scale = Main.rand.NextFloat(0.6f, 0.8f) * Projectile.scale;

                    Color color = Color.Lerp(Color.White, Color.Fuchsia, Main.rand.NextFloat(0.5f, 1f));

                    Dust dust = Dust.NewDustDirect(pos, 1, 1, DustID.TintableDustLighted, vel.X, vel.Y, 0, color, scale);
                    dust.noGravity = true;
                    dust.noLight = false;
                    dust.noLightEmittence = false;
                }
            }

            // Heavy smoke particles (this is what makes it look like DoGFire)
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 smokeVelocity =
                        -Projectile.velocity * 0.7f +
                        Main.rand.NextVector2Circular(1f, 1f) * 0.65f;

                    int lifetime = Main.rand.Next(10, 15);

                    float scale = Main.rand.NextFloat(0.25f, 0.45f) * Projectile.scale;
                    float opacity = Main.rand.NextFloat(0.7f, 0.9f);

                    Color color = Color.Lerp(Color.White, Color.Fuchsia, Main.rand.NextFloat(0.5f, 1f));

                    GeneralParticleHandler.SpawnParticle(
                        new HeavySmokeParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            smokeVelocity,
                            color,
                            lifetime,
                            scale,
                            opacity,
                            0.02f,
                            glowing: true
                        )
                    );
                }
            }

            if (Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming && !IsAHomingFireball)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy(Projectile.owner) && Vector2.DistanceSquared(npc.Center, Projectile.Center) < 200 * 200)
                    {
                        Vector2 vector = npc.Center - Projectile.Center;
                        float num4 = Projectile.velocity.Length();
                        vector.Normalize();
                        vector *= num4;
                        Projectile.velocity = (Projectile.velocity * 19f + vector) / 20f;
                        Projectile.velocity.Normalize();
                        Projectile.velocity *= num4;
                        break;
                    }
                }
            }

            if (IsAHomingFireball && Projectile.localAI[0] > 30f)
            {
                NPC target = null;
                float maxDist = 1200f;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() &&
                        Vector2.DistanceSquared(npc.Center, Projectile.Center) < maxDist * maxDist)
                    {
                        target = npc;
                        break;
                    }
                }

                if (target != null)
                {
                    float speed = Projectile.velocity.Length();

                    Vector2 toTarget = target.Center - Projectile.Center;

                    float currentAngle = Projectile.velocity.ToRotation();
                    float targetAngle = toTarget.ToRotation();

                    float maxTurn = MathHelper.ToRadians(5f);

                    float newAngle = Utils.AngleLerp(currentAngle, targetAngle, maxTurn);

                    Projectile.velocity = newAngle.ToRotationVector2() * speed;
                }
            }
        }

        public float TrailWidth(float completion, Vector2 _)
        {
            float maxWidth = 60f * Projectile.scale;
            float curveStart = 0.2f;

            float width =
                completion < curveStart
                ? (float)Math.Sqrt(completion / curveStart) * maxWidth
                : Utils.Remap(completion, curveStart, 1f, maxWidth, 0f, true);

            float pulse = (float)Math.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            width += MathHelper.Lerp(0f, 10f, pulse);

            return width;
        }

        public Color TrailColor(float completion, Vector2 _)
        {
            Color baseColor = Color.Fuchsia;

            Color endColor = Color.Lerp(baseColor, Color.Transparent,
                Utils.GetLerpValue(0.8f, 1f, completion, true));

            return Color.Lerp(baseColor, endColor, completion);
        }

        public float CoreWidth(float completion, Vector2 _)
        {
            return Utils.Remap(completion, 0f, 1f, 20f * Projectile.scale, 0f);
        }

        public Color CoreColor(float completion, Vector2 _)
        {
            return Color.Lerp(Color.White, Color.Transparent, completion);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, GeneralDrawLayer layer)
        {
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak", (AssetRequestMode)2));
            PrimitiveRenderer.RenderTrail(((ModProjectile)this).Projectile.oldPos, new PrimitiveSettings(FireWidthFunction, FireColorFunction, (float _, Vector2 _) => ((Entity)((ModProjectile)this).Projectile).Size * 0.5f + Utils.SafeNormalize(((Entity)((ModProjectile)this).Projectile).velocity, Vector2.Zero) * 24f, smoothen: true, pixelate: true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), ((ModProjectile)this).Projectile.oldPos.Length + 12);
            int coreLength = (IsAHomingFireball ? 6 : 7);
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak", (AssetRequestMode)2));
            PrimitiveRenderer.RenderTrail(((ModProjectile)this).Projectile.oldPos[..coreLength], new PrimitiveSettings(FireCoreWidthFunction, FireCoreColorFunction, (float _, Vector2 _) => ((Entity)((ModProjectile)this).Projectile).Size * 0.5f + Utils.SafeNormalize(((Entity)((ModProjectile)this).Projectile).velocity, Vector2.Zero) * 24f, smoothen: true, pixelate: true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), coreLength + 8);
        }

        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            float maxBodyWidth = 72f * ((ModProjectile)this).Projectile.scale;
            float curveRatio = 0.2f;
            float width = ((!(completion < curveRatio)) ? Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f, true) : (MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth));
            float pulseInterpolant = MathF.Cos((float)Math.PI * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return width + additionalPulseWidth;
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color val = (IsAHomingFireball ? (Color.Purple * 1.3f) : Color.Cyan);
            Color endColor = Color.Lerp(val, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(val, endColor, completion);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            float maxBodyWidth = ((ModProjectile)this).Projectile.scale * (IsAHomingFireball ? 24f : 64f);
            float curveRatio = 0.25f;
            if (completion < curveRatio)
            {
                return MathF.Sin(completion / curveRatio * ((float)Math.PI / 2f)) * maxBodyWidth + curveRatio;
            }
            return Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f, true);
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            Color val = (IsAHomingFireball ? Color.Fuchsia : Color.SkyBlue);
            Color tipColor = Color.Lerp(val, Color.Transparent, Utils.GetLerpValue(0.8f, 1f, completion, true));
            return Color.Lerp(Color.Lerp(val, tipColor, completion), Color.White, 0.175f);
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * 6f;

                float scale = Main.rand.NextFloat(3f, 5f);
                Color color = Color.Lerp(Color.White, Color.Fuchsia, Main.rand.NextFloat(0.5f, 1f));

                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, vel.X, vel.Y, 0, color, scale);
                dust.noGravity = true;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Damage > 0)
            {
                target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 120);
                target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 120);

                var player = Main.player[Projectile.owner].GetModPlayer<DeathsWhisperPlayer>();

                if (!IsAHomingFireball)
                {
                    player.IncrementHitCounter();
                }
            }
        }
    }
}