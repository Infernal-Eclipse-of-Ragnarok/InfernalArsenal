using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.AbyssalAncor
{
    public class AbyssalAimPro : ModProjectile
    {
        private const float ThrowSpeed = 20f;
        private const float ReturnSpeed = 24f;
        private const float ReturnAcceleration = 2.5f;
        private const float MaxThrowDistance = 500f;
        private const int MaxThrowUpdates = 240;

        private const int MaxChargeTime = 120;
        private const float JavelinSpeed = 22f;
        private const float JavelinSpawnRadius = 90f;

        private const int Aiming = 0;
        private const int Throwing = 1;
        private const int Returning = 2;

        public bool firedRight = true;

        public ref float State => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        private float returnRotation;
        private Vector2 throwDirection;
        private int throwTimer;


        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/AbyssalAnchor";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.noItems || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            Projectile.scale = player.GetAdjustedItemScale(player.HeldItem);

            Projectile.tileCollide = false;
            Projectile.timeLeft = 2;

            switch ((int)State)
            {
                case Aiming:
                    {
                        // Input is handled by the owning player.
                        if (Projectile.owner == Main.myPlayer)
                        {
                            if (!player.controlUseTile)
                            {
                                if (Timer > 0f)
                                    Fire(player);
                                else
                                    Projectile.Kill();

                                return;
                            }

                            HandleAim(player);
                        }

                        HandleCharge();
                        break;
                    }

                case Throwing:
                    {
                        DoThrowBehavior(player);
                        break;
                    }

                case Returning:
                    {
                        DoReturnBehavior(player);
                        break;
                    }
            }

            if (State != Aiming)
                UpdatePlayerAnimation(player);
        }

        private void HandleAim(Player player)
        {
            Vector2 dir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0f));

            player.ChangeDir(dir.X >= 0f ? 1 : -1);

            float animationCompletion = MathHelper.Clamp(Timer / 80f, 0f, 1f);
            float frontArmRotation = dir.ToRotation() - animationCompletion * player.direction * 0.74f;

            if (player.direction == 1)
                frontArmRotation += MathHelper.Pi;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);

            Vector2 handPosition = player.MountedCenter + (frontArmRotation + MathHelper.PiOver2).ToRotationVector2() * 16f * Projectile.scale;
            float pullbackDistance = MathHelper.Lerp(20f, 10f, animationCompletion);

            Projectile.Center = handPosition + dir * pullbackDistance * Projectile.scale;

            // Keep the anchor pointed at the cursor throughout charging.
            Projectile.rotation = dir.ToRotation();
            Projectile.spriteDirection = 1;

            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
        }

        private void HandleCharge()
        {
            // extraUpdates = 1, so increment once per game tick.
            if (Projectile.numUpdates != 0)
                return;

            Timer++;

            if (Timer == 20f || Timer == 40f || Timer == 60f || Timer == 80f)
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = SoundID.DD2_WitherBeastDeath.Volume * 0.5f }, Projectile.Center);

                int dustCount = Timer >= 60f ? 30 : Timer >= 40f ? 20 : 10;

                for (int i = 0; i < dustCount; i++)
                {
                    Vector2 velocity =  Main.rand.NextVector2CircularEdge(6f, 6f);

                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, velocity, 150, Color.White, 1.5f);

                    dust.noGravity = true;
                }
            }
        }

        private void Fire(Player player)
        {
            Vector2 mousePosition = Main.MouseWorld;

            throwDirection = (mousePosition - player.MountedCenter).SafeNormalize(new Vector2(player.direction, 0f));

            // Save the charge before changing states.
            float charge = Timer;

            // Scale damage from 1x to 1.5x
            float chargeProgress = MathHelper.Clamp(charge / 80f, 0f, 1f);
            float damageMultiplier = MathHelper.Lerp(1f, 1.5f, chargeProgress);

            Projectile.damage = (int)(Projectile.damage * damageMultiplier);

            Projectile.velocity = throwDirection * ThrowSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = 1;

            Projectile.friendly = true;
            Projectile.tileCollide = false;

            State = Throwing;
            throwTimer = 0;

            // Use the outward orientation during retraction.
            returnRotation = Projectile.rotation;

            Projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = SoundID.DD2_WitherBeastDeath.Volume * 0.5f }, Projectile.Center);

            SpawnHarpoons(player, mousePosition, charge);
        }

        private void DoThrowBehavior(Player player)
        {
            throwTimer++;

            float distance = Vector2.Distance(Projectile.Center, player.MountedCenter);

            if (throwTimer >= MaxThrowUpdates || distance >= MaxThrowDistance)
            {
                BeginReturn();
                return;
            }

            if (Projectile.velocity.LengthSquared() > 0.01f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        private void BeginReturn()
        {
            if (State == Returning)
                return;

            returnRotation = Projectile.rotation;

            State = Returning;
            throwTimer = 0;

            Projectile.velocity *= 0.2f;
            Projectile.tileCollide = false;
            Projectile.netUpdate = true;
        }

        private void DoReturnBehavior(Player player)
        {
            Vector2 toPlayer = player.MountedCenter - Projectile.Center;

            if (toPlayer.Length() <= 24f)
            {
                Projectile.Kill();
                return;
            }

            Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * ReturnSpeed;

            Projectile.velocity = Projectile.velocity.MoveTowards(desiredVelocity, ReturnAcceleration);

            // Keep the same orientation during retraction.
            Projectile.rotation = returnRotation;
        }

        private void UpdatePlayerAnimation(Player player)
        {
            Vector2 playerCenter = player.MountedCenter;

            player.ChangeDir(Projectile.Center.X >= playerCenter.X ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);

            player.itemRotation = (Projectile.Center - playerCenter).ToRotation();

            if (player.direction == -1)
                player.itemRotation -= MathHelper.Pi;

            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
        }

        private void SpawnHarpoons(Player player, Vector2 mousePosition, float charge)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            int count = Math.Clamp((int)(charge / 10f), 0, 8);

            if (count <= 0)
                return;

            const float spawnRadius = 100f;
            const float harpoonSpeed = 22f;

            int harpoonType = ModContent.ProjectileType<AbyssalHarpoon>();

            for (int i = 0; i < count; i++)
            {
                // Evenly distribute harpoons around the player.
                float angle = MathHelper.TwoPi * i / count;

                Vector2 spawnOffset = angle.ToRotationVector2() * spawnRadius;
                Vector2 spawnPosition = player.MountedCenter + spawnOffset;

                // Every harpoon aims at the cursor independently.
                Vector2 direction = (mousePosition - spawnPosition).SafeNormalize(new Vector2(player.direction, 0f));
                Vector2 velocity = direction * harpoonSpeed;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition, velocity, harpoonType, Projectile.damage / 8, Projectile.knockBack, player.whoAmI);

                // Blue dust burst at each harpoon's spawn position.
                for (int j = 0; j < 12; j++)
                {
                    Vector2 dustVelocity = Main.rand.NextVector2Circular(5f, 5f);

                    Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.BlueTorch, dustVelocity, 100, Color.White, 1.4f);

                    dust.noGravity = true;
                }
            }
        }

        public override bool? CanDamage()
        {
            return State == Aiming ? false : null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (State != Aiming)
                DrawAnchorChain();

            DrawAnchor(lightColor);

            return false;
        }

        private void DrawAnchor(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 origin = texture.Size() * 0.5f;

            // The texture points downward by default.
            // Convert the projectile's directional rotation
            // into the texture's actual drawing rotation.
            float drawRotation = Projectile.rotation - MathHelper.PiOver2;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, drawRotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        private void DrawAnchorChain()
        {
            Player player = Main.player[Projectile.owner];

            Texture2D chainTexture = TextureAssets.Chain.Value;

            Vector2 chainStart = player.MountedCenter;

            // Attach the chain near the anchor's upper end,
            // matching AbyssalAnchorSwungAnchor.
            float anchorRotation = Projectile.rotation - MathHelper.PiOver2;

            Vector2 chainEnd = Projectile.Center - Vector2.UnitY.RotatedBy(anchorRotation) * (Projectile.scale * 12f);

            Vector2 chainDirection = chainEnd - chainStart;
            float chainLength = chainDirection.Length();

            if (chainLength <= 0f)
                return;

            chainDirection.Normalize();

            float segmentLength = chainTexture.Height;
            float chainRotation = chainDirection.ToRotation() - MathHelper.PiOver2;

            Vector2 origin = chainTexture.Size() * 0.5f;

            for (float distance = 0f; distance < chainLength; distance += segmentLength)
            {
                Vector2 drawPosition = chainStart + chainDirection * distance;
                float remainingLength = chainLength - distance;
                int sourceHeight =  (int)Math.Min(segmentLength, remainingLength);

                if (sourceHeight <= 0)
                    break;

                Rectangle sourceRect = new Rectangle(0, 0, chainTexture.Width, sourceHeight);
                Vector2 segmentOrigin = new Vector2(origin.X, 0f);
                Color chainColor = Lighting.GetColor((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));

                Main.EntitySpriteDraw(chainTexture, drawPosition - Main.screenPosition, sourceRect, chainColor, chainRotation, segmentOrigin, 1f, SpriteEffects.None, 0);
            }
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(returnRotation);
            writer.Write(throwTimer);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            returnRotation = reader.ReadSingle();
            throwTimer = reader.ReadInt32();
        }
    }

    public class AbyssalHarpoon : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Harpoon}";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Vanilla Harpoon texture points upward.
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Blue particles trailing behind the harpoon.
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, -Projectile.velocity * 0.15f, 100, Color.White, 1.1f);
                dust.noGravity = true;
            }

            // Fade near the end of its lifetime.
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f,Projectile.timeLeft / 30f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch, Main.rand.NextVector2Circular(4f, 4f), 100, Color.White, 1.2f);
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        }
    }

}
