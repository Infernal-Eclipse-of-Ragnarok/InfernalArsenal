using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using ReLogic.Content;
using CalamityMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro.GrandAmplifier
{
    public class GrandAmplifierLightning : ModProjectile
    {
        public override string Texture =>
            "InfernalEclipseWeaponsDLC/Assets/Textures/GrandThunder";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1200;

            if (ModLoader.TryGetMod("Redemption", out Mod redemption))
            {
                redemption.Call(
                    "addElementProj",
                    7,
                    Projectile.type
                );
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.timeLeft = 180;
            Projectile.MaxUpdates = 180;
        }

        public NPC TargetNPC => Projectile.ai[0] >= 0 && Projectile.ai[0] < Main.maxNPCs
        ? Main.npc[(int)Projectile.ai[0]]
        : null;

        float LightningProgress
        {
            get => Projectile.localAI[2];
            set => Projectile.localAI[2] = value;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f && Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[0] = Projectile.Center.X;
                Projectile.localAI[1] = Projectile.Center.Y;

                if (Projectile.velocity == Vector2.Zero)
                    Projectile.velocity = Vector2.UnitY;

                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 8f;

                Main.instance.CameraModifiers.Add(
                    new PunchCameraModifier(
                        Projectile.Center,
                        Projectile.velocity.SafeNormalize(Vector2.UnitY),
                        6f,
                        8f,
                        20,
                        1200f
                    )
                );

                SoundStyle zap = new("ThoriumMod/Sounds/Custom/GrandZapNoise")
                {
                    Pitch = -0.3f,
                    MaxInstances = 5
                };

                SoundEngine.PlaySound(zap, Projectile.Center);
            }

            // ⭐ THIS RETURN IS CRITICAL ⭐
            if (LightningProgress <= 0f && Projectile.timeLeft > 2)
                return;

            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 2;
            Projectile.velocity = Vector2.Zero;

            if (LightningProgress < 20f * Projectile.MaxUpdates)
                LightningProgress++;
            else
                Projectile.Kill();
        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (LightningProgress <= 0f)
                return false;

            Vector2 startPos = new(
                Projectile.localAI[0],
                Projectile.localAI[1]
            );

            Vector2 endPos = Projectile.Center;

            Texture2D boltTex = ModContent.Request<Texture2D>(
                Texture,
                AssetRequestMode.ImmediateLoad
            ).Value;

            float progress = LightningProgress / (20f * Projectile.MaxUpdates);
            float scale = (float)Math.Sin(progress * Math.PI) * Projectile.scale * 0.365f;

            List<Vector2> arcPoints = new();
            int arcCount = (int)(Vector2.Distance(startPos, endPos) / 8f);

            for (int i = 1; i < arcCount; i++)
            {
                arcPoints.Add(
                    Vector2.SmoothStep(startPos, endPos, i / (float)arcCount) +
                    (i > 1 ? scale * 5f : 0f) *
                    Utils.NextVector2Circular(Main.rand, boltTex.Width, boltTex.Height) / 5f
                );
            }

            lightColor = Color.White;

            // === ARC NODES ===
            for (int i = 0; i < arcPoints.Count; i++)
            {
                Vector2 drawPos = arcPoints[i];

                Main.EntitySpriteDraw(
                    boltTex,
                    drawPos - Main.screenPosition,
                    null,
                    lightColor,
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    boltTex.Size() / 2f,
                    scale,
                    SpriteEffects.None
                );
            }

            // === BEAM BODY ===
            Vector2 prev = startPos;
            for (int i = 0; i < arcPoints.Count; i++)
            {
                Vector2 next = arcPoints[i];

                Main.EntitySpriteDraw(
                    boltTex,
                    prev - Main.screenPosition,
                    new Rectangle(0, boltTex.Height / 2 - 1, boltTex.Width, 1),
                    lightColor,
                    (prev - next).ToRotation() + MathHelper.PiOver2,
                    new Vector2(boltTex.Width / 2f, 0f),
                    new Vector2(scale, Vector2.Distance(prev, next)),
                    SpriteEffects.None
                );

                prev = next;
            }

            // === THUNDERBIRD CLOUD ===
            if (ModLoader.TryGetMod("ThoriumMod", out _))
            {
                Texture2D cloudTex = ModContent.Request<Texture2D>(
                    "ThoriumMod/Projectiles/Boss/GrandThunderBirdCloud",
                    AssetRequestMode.ImmediateLoad
                ).Value;

                Projectile.frameCounter++;
                if (Projectile.frameCounter > 15)
                    Projectile.frameCounter = 0;

                Projectile.frame = Projectile.frameCounter / 3;

                Main.EntitySpriteDraw(
                    cloudTex,
                    startPos - Main.screenPosition,
                    new Rectangle(0, cloudTex.Height / 6 * Projectile.frame, cloudTex.Width, cloudTex.Height / 6),
                    lightColor * MathHelper.Min(scale * 3f, 1f),
                    0f,
                    new Vector2(cloudTex.Width, cloudTex.Height / 6) / 2f,
                    Projectile.scale * 1.25f,
                    SpriteEffects.None
                );
            }

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;

            SpawnImpactDust();
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.tileCollide = false;
            Projectile.velocity = Vector2.Zero;

            SpawnImpactDust();

            // === SERVER-SIDE ONLY ===
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ProjectileID.Electrosphere,
                    Projectile.damage,
                    0f,
                    Projectile.owner
                );

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    Projectile p = Main.projectile[proj];
                    p.timeLeft = 30;
                    p.DamageType = DamageClass.Magic;

                    p.usesIDStaticNPCImmunity = false;
                    p.usesLocalNPCImmunity = true;
                    p.localNPCHitCooldown = 40;

                    p.GetGlobalProjectile<AmplifierElectrosphereGlobal>().spawnedByAmplifier = true;

                    p.netUpdate = true;
                }
            }

            /*
            // === REMOVE ELECTRIFIED ===
            if (target.HasBuff(BuffID.Electrified))
                target.DelBuff(BuffID.Electrified);

            var calNPC = target.Calamity();
            if (calNPC.electrified == true)
                calNPC.electrified = false;
            */
        }


        private void SpawnImpactDust()
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVelocity = Utils.NextVector2Circular(
                    Main.rand,
                    Projectile.width,
                    Projectile.height
                );

                dustVelocity.Y -= Projectile.height;
                dustVelocity.Y *= 0.5f;

                Dust.NewDustPerfect(
                    Projectile.Center + dustVelocity,
                    DustID.Electric, // Electric dust (same as Thorium)
                    dustVelocity,
                    128,
                    default,
                    Projectile.scale * 1.1f
                );
            }
        }


        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
            Projectile.localAI[1] = reader.ReadSingle();
        }
    }

    public class AmplifierElectrosphereGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool spawnedByAmplifier;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.type == ProjectileID.Electrosphere &&
                projectile.ai[2] == 1f)
            {
                spawnedByAmplifier = true;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!spawnedByAmplifier)
                return;

            var gNPC = target.GetGlobalNPC<AmplifierNoElectrifiedGlobalNPC>();
            gNPC.electrifiedBlockTimer = 6;

            target.netUpdate = true;
        }

        public class AmplifierNoElectrifiedGlobalNPC : GlobalNPC
        {
            public override bool InstancePerEntity => true;

            public int electrifiedBlockTimer;

            // === SYNC THE TIMER ===
            public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
            {
                binaryWriter.Write(electrifiedBlockTimer);
            }

            public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
            {
                electrifiedBlockTimer = binaryReader.ReadInt32();
            }

            public override void ResetEffects(NPC npc)
            {
                if (electrifiedBlockTimer > 0)
                    electrifiedBlockTimer--;
            }

            public override void UpdateLifeRegen(NPC npc, ref int damage)
            {
                if (electrifiedBlockTimer <= 0)
                    return;

                // ONLY RUN ON SERVER
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return;

                bool changed = false;

                // Remove vanilla Electrified
                int index = npc.FindBuffIndex(BuffID.Electrified);
                if (index != -1)
                {
                    npc.DelBuff(index);
                    changed = true;
                }

                // Remove Calamity Electrified
                if (ModLoader.TryGetMod("CalamityMod", out _))
                {
                    var calNPC = npc.Calamity();
                    if (calNPC.electrified)
                    {
                        calNPC.electrified = false;
                        changed = true;
                    }
                }

                // FORCE SYNC IF ANYTHING CHANGED
                if (changed)
                    npc.netUpdate = true;
            }
        }
    }
}
