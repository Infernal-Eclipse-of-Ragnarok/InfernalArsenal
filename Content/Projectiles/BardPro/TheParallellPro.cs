using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;
using CalamityMod.Buffs.StatDebuffs;
using Terraria.GameContent;
using System.Reflection;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class TheParallellPro : BardProjectile
    {
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/CosmicKunai";
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 13;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        private bool tripled = false;

        public override void SetBardDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = 2;
            Projectile.DamageType = ThoriumDamageBase<BardDamage>.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }

        public override void AI()
        {
            // Fade-in on spawn
            int fadeInDuration = 180;
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 255 / fadeInDuration;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            // Normal State
            if (Projectile.ai[0] == 0f)
            {
                // Face movement direction
                if (Projectile.velocity.Length() > 0.1f)
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Trap State
            else if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[1]++;

                float progress = Projectile.ai[1] / 300f; // 5 sec charge

                // Aggressively slow down
                Projectile.velocity *= 0.95f;

                // Spin faster over time, but cap at full charge
                float spinSpeed = MathHelper.Lerp(0.01f, 0.5f, Math.Min(progress, 1f));
                Projectile.rotation += spinSpeed;

                // Fade out (or keep visible at full charge)
                if (progress < 1f)
                {
                    Projectile.alpha = (int)MathHelper.Lerp(0, 200, progress);
                }
                else
                {
                    Projectile.alpha = 200; // lock visibility
                }

                // Fully charged
                if (progress >= 1f)
                {
                    Projectile.velocity *= 0.925f; // basically zero
                    Projectile.alpha = 255;

                    NPC target = FindNearestTarget(600f);

                    if (target != null)
                    {
                        Projectile.ai[0] = 2f;

                        // Start slow → will accelerate in homing state
                        Vector2 dir = Projectile.DirectionTo(target.Center);
                        Projectile.velocity = dir * 4f;

                        if (!tripled)
                        {
                            Projectile.damage *= 3;
                            tripled = true;
                        }

                        Projectile.netUpdate = true;
                    }
                }
            }

            // Homing State
            else if (Projectile.ai[0] == 2f)
            {
                NPC target = FindNearestTarget(600f);

                float maxSpeed = 40f;
                float accel = 50f;     // acceleration per tick
                float inertia = 12f;    // turning smoothness

                if (target != null)
                {
                    Vector2 desiredDir = Projectile.DirectionTo(target.Center);

                    // Gradually increase speed
                    float currentSpeed = Projectile.velocity.Length();
                    float newSpeed = MathHelper.Clamp(currentSpeed + accel, 0f, maxSpeed);

                    Vector2 desiredVelocity = desiredDir * newSpeed;

                    Projectile.velocity =
                        (Projectile.velocity * (inertia - 1) + desiredVelocity) / inertia;
                }

                // Face movement direction
                if (Projectile.velocity.Length() > 0.1f)
                    Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            if (Projectile.ai[0] == 2f || Projectile.ai[0] == 1f && Projectile.ai[1] >= 300f) // homing or fully charged trap
            {
                Projectile.tileCollide = false; // ignore walls
            }
            else
            {
                Projectile.tileCollide = true; // normal movement respects walls
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Set lifetime to 15 seconds after bouncing
            this.Projectile.timeLeft = 3600;

            // Try entering trap state
            if (CountActiveTraps() < 27)
            {
                this.Projectile.ai[0] = 1f;
                this.Projectile.ai[1] = 0f;
            }
            else
            {
                this.Projectile.ai[0] = 2f;
            }

            Projectile.netUpdate = true;

            // Bounce (preserve full velocity)
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            return false;
        }

        private int CountActiveTraps()
        {
            int count = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (!p.active)
                    continue;

                if (p.type != Projectile.type)
                    continue;

                if (p.ai[0] == 1f) // trap state
                    count++;
            }

            return count;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float trapProgress = 0f;
            if (Projectile.ai[0] == 1f || (Projectile.ai[0] == 2f && Projectile.ai[1] >= 300f))
                trapProgress = MathHelper.Clamp(Projectile.ai[1] / 300f, 0f, 1f);

            //Trap Aura
            if (Projectile.ai[0] == 1f || (Projectile.ai[0] == 2f && Projectile.ai[1] >= 300f))
            {
                float intensity = (trapProgress * 1);

                int layers = (int)MathHelper.Lerp(4, 16, intensity);
                float radius = MathHelper.Lerp(4f, 14f, intensity);

                for (int i = 0; i < layers; i++)
                {
                    float angle = MathHelper.TwoPi * i / layers;

                    // Slight pulsing distortion
                    float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + i) * 0.01f;

                    Vector2 offset = angle.ToRotationVector2() * radius * pulse;

                    // VERY low alpha per layer → builds softness
                    Color glowColor = new Color(180, 120, 255, 0)
                        * intensity
                        * 0.25f; // key: keep this LOW

                    Main.EntitySpriteDraw(
                        texture,
                        drawPos + offset,
                        null,
                        glowColor,
                        Projectile.rotation,
                        origin,
                        Projectile.scale * (1f + intensity * 0.1f),
                        SpriteEffects.None,
                        0
                    );
                }

                for (int i = 0; i < 6; i++)
                {
                    Vector2 smallOffset = Main.rand.NextVector2Circular(2f, 2f);

                    Color innerGlow = new Color(220, 160, 255, 0)
                        * intensity
                        * 0.35f;

                    Main.EntitySpriteDraw(
                        texture,
                        drawPos + smallOffset,
                        null,
                        Projectile.GetAlpha(innerGlow),
                        Projectile.rotation,
                        origin,
                        Projectile.scale * 0.9f,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            // Homing Afterimage
            if (Projectile.ai[0] == 2f && Projectile.velocity.Length() > 2f)
            {
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);

                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = dir * -i * 12f;

                    Color trailColor = new Color(198, 118, 204, 0)
                        * (1f - i / 6f)
                        * 0.8f;

                    Main.EntitySpriteDraw(
                        texture,
                        drawPos + offset,
                        null,
                        Projectile.GetAlpha(trailColor),
                        Projectile.rotation,
                        origin,
                        Projectile.scale * 0.9f,
                        SpriteEffects.None,
                        0
                    );
                }
            }

            //Base Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = i / (float)Projectile.oldPos.Length;

                Color color = Color.Lerp(
                    new Color(84, 39, 89, 255),
                    new Color(36, 16, 56, 100),
                    progress
                );

                color *= 0.6f;

                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                Main.EntitySpriteDraw(
                    texture,
                    pos,
                    null,
                    color,
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale * (1f - progress),
                    SpriteEffects.None,
                    0
                );
            }

            //Main Proj
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            Vector2 toPlayer = owner.Center - Projectile.Center;
            Projectile.rotation = toPlayer.ToRotation() + MathHelper.PiOver2;

            target.AddBuff(ModContent.BuffType<WhisperingDeath>(), 120);

            bool pickPebble = false;
            if (ModLoader.TryGetMod("ThoriumRework", out Mod thoriumRework))
            {
                Type thoriumPlayerType = thoriumRework.Code?.GetType("ThoriumRework.ThoriumPlayer");
                if (thoriumPlayerType != null)
                {
                    var modPlayersField = typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic);
                    var modPlayersList = modPlayersField?.GetValue(owner) as IList<ModPlayer>;
                    if (modPlayersList != null)
                    {
                        foreach (var modPlayer in modPlayersList)
                        {
                            if (modPlayer.GetType().FullName == "ThoriumRework.ThoriumPlayer")
                            {
                                var pickPebbleField = thoriumPlayerType.GetField("pebblePick", BindingFlags.Instance | BindingFlags.Public);
                                if (pickPebbleField != null)
                                    pickPebble = (bool)pickPebbleField.GetValue(modPlayer);

                                break;
                            }
                        }
                    }
                }
            }

            // If true, do OnTileCollide behavior manually
            if (pickPebble)
            {
                // Set lifetime to 15 seconds after bouncing
                this.Projectile.timeLeft = 3600;

                // Try entering trap state
                if (CountActiveTraps() < 27)
                {
                    this.Projectile.ai[0] = 1f;
                    this.Projectile.ai[1] = 0f;
                }
                else
                {
                    this.Projectile.ai[0] = 2f;
                }

                Projectile.netUpdate = true;
            }
        }

        private NPC FindNearestTarget(float range)
        {
            NPC closest = null;
            float minDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
