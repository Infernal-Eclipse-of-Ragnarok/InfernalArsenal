using System.IO;
using InfernalEclipseWeaponsDLC.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using ThoriumMod.Projectiles.Scythe;
using CalamityMod.Buffs.StatDebuffs;
using System;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes.GammaKnife
{
    public class GammaKnifeProjectile : ScythePro
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Melee/Scythes/GammaKnife";

        /// <summary>
        ///     Gets the asset for the glowmask texture of the projectile.
        /// </summary>
        public static Asset<Texture2D> Glowmask { get; private set; }

        /// <summary>
        ///     Gets the <see cref="Player" /> instance that owns the projectile. Shorthand for
        ///     <c>Main.player[Projectile.owner]</c>.
        /// </summary>
        private Player Owner => Main.player[Projectile.owner];

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Glowmask = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Projectiles/HealerPro/Scythes/GammaKnife/GammaKnife_Glow");
            }
        }

        public override void Unload()
        {
            Glowmask = null;
        }

        public override void SafeSetDefaults()
        {
            Projectile.ownerHitCheck = true;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.width = 64;
            Projectile.height = 64;

            Projectile.penetrate = -1;

            Projectile.timeLeft = (int)Projectile.ai[1];

            Projectile.localNPCHitCooldown = 10;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesIDStaticNPCImmunity = false;

            Projectile.manualDirectionChange = true;

            Projectile.ArmorPenetration = Projectile.ArmorPenetration = 15;
        }

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            var direction = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;

            Projectile.direction = direction;
            Projectile.spriteDirection = direction;
            Projectile.scale *= Owner.GetAdjustedItemScale(Owner.HeldItem);
        }

        public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var position = target.Center;
            var velocity = Main.rand.NextVector2Circular(2f, 2f);

            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);

            SpawnDustEffects(in position, in velocity);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            var position = target.Center;
            var velocity = Main.rand.NextVector2Circular(2f, 2f);

            target.AddBuff(ModContent.BuffType<Irradiated>(), 180);

            SpawnDustEffects(in position, in velocity);
        }

        private static void SpawnDustEffects(in Vector2 position, in Vector2 velocity)
        {
            for (var i = 0; i < 10; i++)
            {
                var offset = Main.rand.NextVector2Circular(8f, 8f) * 2f;
                var dust = Dust.NewDustDirect(position + offset, 0, 0, ModContent.DustType<GammaDust>(), velocity.X, velocity.Y);

                dust.noGravity = true;

                dust.scale = Main.rand.NextFloat(1f, 3f);
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            Projectile.spriteDirection = reader.ReadSByte();
        }

        public float swingOffset;

        public override bool PreAI()
        {
            Player owner = Owner;

            if (!owner.active || owner.dead || owner.noItems || owner.CCed)
            {
                Projectile.Kill();
                return false;
            }

            // === Attack-speed-based lifespan ===
            if (Projectile.ai[1] <= 0)
            {
                Projectile.Kill();
                return false;
            }

            Projectile.timeLeft = (int)Projectile.ai[1];
            Projectile.ai[1]--;

            // === Swing progress (0 → 1) ===
            float t = 1f - Projectile.ai[1] / Projectile.ai[0];
            t = 1f - MathF.Pow(1f - t, 3); // your cubic ease

            // === Rotation code stays identical ===
            Vector2 toMouse = Main.MouseWorld - owner.Center;
            float aimAngle = toMouse.ToRotation();

            float startSwing = MathHelper.ToRadians(-60f);
            float endSwing = MathHelper.ToRadians(150f);

            swingOffset = MathHelper.Lerp(startSwing, endSwing, t);

            Projectile.rotation =
                Projectile.spriteDirection == 1
                ? aimAngle + swingOffset
                : MathHelper.Pi + aimAngle - swingOffset;

            Projectile.Center = owner.Center;

            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return false;
            }

            var direction = Owner.direction;

            Projectile.direction = direction;
            Projectile.spriteDirection = direction;

            UpdateEffects();
            UpdateSlash();
            //UpdateSwing();
            UpdateOwner();

            Projectile.velocity = Vector2.Zero;

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float itemScale = Owner.GetAdjustedItemScale(Owner.HeldItem);

            // Calculate the blade tip position
            Vector2 hitboxPos = Projectile.Center -
                new Vector2((-32f * Owner.GetAdjustedItemScale(Owner.HeldItem)) * Projectile.direction, 64f).RotatedBy(Projectile.rotation);

            // Base size of the hitbox
            int baseSize = 64;

            // Scale the hitbox
            float scaledSize = baseSize * itemScale;

            Rectangle bladeHitbox = new Rectangle(
                (int)(hitboxPos.X - scaledSize / 2f),
                (int)(hitboxPos.Y - scaledSize / 2f),
                (int)scaledSize,
                (int)scaledSize
            );

            return bladeHitbox.Intersects(targetHitbox);
        }

        private void UpdateEffects()
        {
            var rotation = Projectile.rotation - MathHelper.PiOver4;

            if (Projectile.spriteDirection == -1)
            {
                rotation -= MathHelper.PiOver2;
            }

            var position = Projectile.Center - new Vector2(-32f * Projectile.direction, 64f).RotatedBy(Projectile.rotation);
            var velocity = rotation.ToRotationVector2() * 4f;

            SpawnDustEffects(in position, in velocity);
        }

        private bool crescentFired = false;

        private void UpdateSlash()
        {
            // Spawn normal gamma explosion periodically
            if (Projectile.timeLeft % 20 == 0)
            {
                var position = Projectile.Center - new Vector2(-32f * Projectile.direction, 64f).RotatedBy(Projectile.rotation);

                Projectile.NewProjectile
                (
                    Projectile.GetSource_Death(),
                    position,
                    Vector2.Zero,
                    ModContent.ProjectileType<GammaExplosionProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            // Spawn crescent moon slash a third of the way into swing
            if (!crescentFired && Projectile.ai[1] <= Projectile.ai[0] * 0.75f)
            {
                Player owner = Main.player[Projectile.owner];

                // Get aim direction from player to mouse
                Vector2 aimDirection = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.Zero);

                // Get shoot speed from held item
                float shootSpeed = owner.HeldItem.shootSpeed;

                // Set velocity
                Vector2 velocity = aimDirection * (float)owner.HeldItem.shootSpeed * 1.75f;

                // Position of crescent
                var position = Projectile.Center;

                Projectile.NewProjectile
                (
                    Projectile.GetSource_Death(),
                    position,
                    velocity,
                    ModContent.ProjectileType<GammaSlashProjectile>(),
                    (Projectile.damage * 2),
                    Projectile.knockBack,
                    Projectile.owner
                );

                crescentFired = true;
            }
        }

        private void UpdateSwing()
        {
            var multiplier = 1f - Projectile.ai[1] / Projectile.ai[0];
            var progress = multiplier * multiplier * multiplier;

            /*
            var position = Owner.Center;

            var start = MathHelper.ToRadians(160f);
            var end = MathHelper.ToRadians(-110f);
            var rotation = MathHelper.SmoothStep(start, end, progress);

            if (Projectile.spriteDirection == -1)
            {
                rotation = MathHelper.Pi - rotation;
            }

            Projectile.Center = position;
            Projectile.rotation = Projectile.rotation.AngleLerp(rotation + (Projectile.spriteDirection == -1 ? MathHelper.Pi : 0f), 0.2f);
            */

            float itemScale = Owner.GetAdjustedItemScale(Owner.HeldItem);
            Projectile.scale = MathHelper.SmoothStep(1.2f, 0f, progress) * itemScale;
        }

        private void UpdateOwner()
        {
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(135f) * Projectile.spriteDirection);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.ToRadians(135f) * Projectile.spriteDirection);

            Owner.heldProj = Projectile.whoAmI;

            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawProjectile(in lightColor);

            return false;
        }

        private void DrawProjectile(in Color lightColor)
        {
            var texture = TextureAssets.Projectile[Type].Value;

            var color = Projectile.GetAlpha(lightColor);
            var origin = (Projectile.spriteDirection == -1 ? texture.Size() : new Vector2(0f, texture.Height)) + new Vector2(DrawOriginOffsetX, DrawOriginOffsetY);

            var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, Projectile.scale, effects);

            var glowmask = Glowmask.Value;

            color = Projectile.GetAlpha(Color.White);

            Main.EntitySpriteDraw(glowmask, position, null, color, Projectile.rotation, origin, Projectile.scale, effects);
        }
    }
}

