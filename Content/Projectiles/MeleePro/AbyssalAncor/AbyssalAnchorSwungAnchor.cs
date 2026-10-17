using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static InfernalEclipseWeaponsDLC.Utilities.MathematicalUtils;
using static Terraria.ModLoader.ModContent;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.AbyssalAncor
{
    public class AbyssalAnchorSwungAnchor : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/AbyssalAnchor";

        private bool initialized = false;
        Vector2 direction = Vector2.Zero;
        public ref float Combo => ref Projectile.ai[0];
        public Player Owner => Main.player[Projectile.owner];

        #region Regular swing variables
        const float MaxSwingTime = 35;
        public int SwingDirection
        {
            get
            {
                switch (Combo)
                {
                    case 0:
                        return 1 * Math.Sign(direction.X);
                    case 1:
                        return -1 * Math.Sign(direction.X);
                    default:
                        return 0;

                }
            }
        }

        private float SwingWidth = MathHelper.PiOver2 * 1.5f;
        public Vector2 DistanceFromPlayer => direction * 30;
        public float SwingTimer => MaxSwingTime - Projectile.timeLeft;
        public float SwingCompletion => SwingTimer / MaxSwingTime;
        public ref float HasFired => ref Projectile.localAI[0];
        #endregion

        #region Throw variables
        //Only used for the long range throw
        private bool OwnerCanShoot => !CantUseHoldout(Owner) && Owner.HeldItem.type == ModContent.ItemType<AbyssalAnchor>();
        public static bool CantUseHoldout(Player player)
        {
            bool isChannel = player.channel;

            if (player != null && player.active && !player.dead && !(!isChannel) && !player.CCed)
            {
                return player.noItems;
            }

            return true;
        }

        public bool Thrown => Combo == 2 || Combo == 3;

        private const float ThrowSpeed = 20f;
        private const float ReturnSpeed = 24f;
        private const float ReturnAcceleration = 2.5f;

        private const float MaxThrowDistance = 500f;
        private const int MaxThrowUpdates = 240;

        public const float ThrowSpawnOffset = 55f;
        private const int TileCollisionGraceUpdates = 8;

        private ref float ThrowState => ref Projectile.ai[1];
        private ref float ThrowTimer => ref Projectile.localAI[1];

        private const int Throwing = 0;
        private const int Returning = 1;

        private float returnRotation;
        #endregion

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.width = 40;
            Projectile.width = 56;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (int)MaxSwingTime;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float bladeLength = 142f * Projectile.scale;

            Texture2D texture = Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/AbyssalAnchor").Value;

            if (Thrown)
            {
                float rotation = Projectile.rotation - MathHelper.PiOver2;
                Vector2 shaftDirection = Vector2.UnitY.RotatedBy(rotation);
                float halfLength = texture.Height * Projectile.scale * 0.5f;
                Vector2 collisionStartThrown = Projectile.Center - shaftDirection * halfLength;
                Vector2 collisionEndThrown = Projectile.Center + shaftDirection * halfLength;

                float collisionPointThrown = 0f;

                return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), collisionStartThrown, collisionEndThrown, texture.Width * Projectile.scale * 0.65f, ref collisionPointThrown);
            }

            bool flipped = Owner.direction < 0;

            float extraAngle = flipped ? MathHelper.PiOver2 : 0f;
            float angleShift = Combo == 0 ? MathHelper.PiOver4 : MathHelper.PiOver2;
            float drawRotation = Projectile.rotation + angleShift + extraAngle + MathHelper.Pi;
            Vector2 drawOrigin = new Vector2(Combo == 1 ? texture.Width / 2f : flipped ? texture.Width : 0f, 0f);
            Vector2 drawPosition = Owner.Center + Projectile.rotation.ToRotationVector2() * 10f;

            // Match the sprite's actual horizontal flip.
            float originX = flipped ? texture.Width - drawOrigin.X : drawOrigin.X;
            Vector2 collisionStart = drawPosition + new Vector2(texture.Width * 0.5f - originX, 0f).RotatedBy(drawRotation) * Projectile.scale;
            Vector2 collisionEnd = collisionStart + Vector2.UnitY.RotatedBy(drawRotation) * texture.Height * Projectile.scale;

            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), collisionStart, collisionEnd, 24f * Projectile.scale, ref collisionPoint);
        }

        //Swing animation keys
        public CurveSegment anticipation = new CurveSegment(EasingType.ExpOut, 0f, 0f, 0.15f);
        public CurveSegment thrust = new CurveSegment(EasingType.PolyInOut, 0.1f, 0.15f, 0.85f, 3);
        public CurveSegment hold = new CurveSegment(EasingType.Linear, 0.5f, 1f, 0.2f);
        internal float SwingRatio() => PiecewiseAnimation(SwingCompletion, new CurveSegment[] { anticipation, thrust, hold });

        public override void AI()
        {
            if (!initialized) //Initialization
            {
                Projectile.timeLeft = Thrown ? 2 : (int)MaxSwingTime;
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = SoundID.DD2_WitherBeastDeath.Volume * 0.5f }, Projectile.Center);
                direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.rotation = direction.ToRotation();

                if (Thrown)
                {
                    // Preserve the spawn position and velocity supplied by Shoot().
                    Projectile.velocity = direction * ThrowSpeed;
                    Projectile.rotation = direction.ToRotation();

                    ThrowState = Throwing;
                    ThrowTimer = 0f;

                    Projectile.scale = 1.5f;
                    Projectile.localNPCHitCooldown = 10;
                }

                initialized = true;
                ForceNetUpdate(Projectile);
            }

            if (Thrown)
            {
                DoAnchorThrowAI();
                return;
            }

            //Manage position and rotation
            Projectile.Center = Owner.Center + DistanceFromPlayer;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Lerp(SwingWidth / 2 * SwingDirection, -SwingWidth / 2 * SwingDirection, SwingRatio()) - (Combo == 1 ? MathHelper.PiOver4 : 0f);
            Projectile.scale = 1.2f + ((float)Math.Sin(SwingRatio() * MathHelper.Pi) * 0.6f);

            //Make the owner look like theyre holding the sword bla bla
            Owner.heldProj = Projectile.whoAmI;
            Owner.ChangeDir(Math.Sign(Projectile.velocity.X));
            Owner.itemRotation = Projectile.rotation;
            if (Owner.direction != 1)
            {
                Owner.itemRotation -= MathHelper.Pi;
            }
            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
        }

        private void DoAnchorThrowAI()
        {
            Vector2 playerCenter = Owner.MountedCenter;
            float distance = Vector2.Distance(Projectile.Center, playerCenter);

            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed || distance > 900f)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            switch ((int)ThrowState)
            {
                case Throwing:
                    {
                        ThrowTimer++;

                        if (ThrowTimer >= MaxThrowUpdates || distance >= MaxThrowDistance)
                        {
                            BeginAnchorReturn();
                        }

                        break;
                    }

                case Returning:
                    {
                        Vector2 toPlayer = playerCenter - Projectile.Center;

                        if (toPlayer.Length() <= 24f)
                        {
                            Projectile.Kill();
                            return;
                        }

                        Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * ReturnSpeed;

                        Projectile.velocity = Projectile.velocity.MoveTowards(desiredVelocity, ReturnAcceleration);

                        break;
                    }
            }

            // Orient the anchor based on movement.
            if (ThrowState == Returning)
            {
                Projectile.rotation = returnRotation;
            }
            else if (Projectile.velocity.LengthSquared() > 0.01f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Owner.ChangeDir(Projectile.Center.X >= playerCenter.X ? 1 : -1);

            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);

            Owner.itemRotation = (Projectile.Center - playerCenter).ToRotation();

            if (Owner.direction == -1)
                Owner.itemRotation -= MathHelper.Pi;

            Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation);
        }

        private void BeginAnchorReturn()
        {
            if (!Thrown || ThrowState == Returning)
                return;

            // Preserve the anchor's orientation when retraction begins.
            returnRotation = Projectile.rotation;

            ThrowState = Returning;
            ThrowTimer = 0f;

            // Stop outward movement before accelerating back.
            Projectile.velocity *= 0.2f;

            Projectile.netUpdate = true;
        }

        /*
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Thrown || ThrowState != Throwing)
                return false;

            // Stop movement immediately on collision.
            Projectile.velocity = Vector2.Zero;

            BeginAnchorReturn();

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Thrown && ThrowState == Throwing && !target.friendly && !target.dontTakeDamage)
            {
                //BeginAnchorReturn();
            }
        }
        */

        public override bool PreDraw(ref Color lightColor)
        {
            if (!Thrown)
            {
                DrawSingleSwungAnchor(lightColor);
            }
            else
            {
                DrawAnchorChain(lightColor);
                DrawThrownAnchor(lightColor);
            }

            return false;
        }

        public void DrawSingleSwungAnchor(Color lightColor)
        {
            Texture2D sword = Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/AbyssalAnchor").Value;

            bool flipped = Owner.direction < 0;
            SpriteEffects flip = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraAngle = flipped ? MathHelper.PiOver2 : 0f;

            float drawAngle = Projectile.rotation;
            float angleShift = Combo == 0 ? MathHelper.PiOver4 : MathHelper.PiOver2;
            float drawRotation = Projectile.rotation + angleShift + extraAngle + MathHelper.Pi;

            Vector2 drawOrigin = new Vector2(Combo == 1 ? sword.Width / 2f : flipped ? sword.Width : 0f, 0f);
            Vector2 drawOffset = Owner.Center + drawAngle.ToRotationVector2() * 10f - Main.screenPosition;

            Main.EntitySpriteDraw(sword, drawOffset, null, lightColor, drawRotation, drawOrigin, Projectile.scale, flip, 0);

            /*
            if (SwingCompletion > 0.5f)
            {
                Texture2D smear = Request<Texture2D>("CalamityMod/Particles/TrientCircularSmear").Value;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                float opacity = (float)Math.Sin(SwingCompletion * MathHelper.Pi);
                float rotation = (-MathHelper.PiOver4 * 0.5f + MathHelper.PiOver4 * 0.5f * SwingCompletion + (Combo == 1f ? MathHelper.PiOver4 : 0)) * SwingDirection;
                Color smearColor = Main.hslToRgb(((SwingTimer - MaxSwingTime * 0.5f) / (MaxSwingTime * 0.5f)) * 0.15f + ((Combo == 1f) ? 0.85f : 0f), 1, 0.6f);

                Main.EntitySpriteDraw(smear, Owner.Center - Main.screenPosition, null, smearColor * 0.5f * opacity, Projectile.velocity.ToRotation() + MathHelper.Pi + rotation, smear.Size() / 2f, Projectile.scale * 2.3f, SpriteEffects.None, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            */
        }

        private void DrawThrownAnchor(Color lightColor)
        {
            Texture2D texture = Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/AbyssalAnchor").Value;

            Vector2 origin = texture.Size() * 0.5f;

            // The anchor sprite points downward in its texture.
            float rotation = Projectile.rotation - MathHelper.PiOver2;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }

        private void DrawAnchorChain(Color lightColor)
        {
            Texture2D chainTexture = TextureAssets.Chain.Value;

            // Start at the player's hand.
            Vector2 chainStart = Owner.MountedCenter;

            // End at the top of the anchor.
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

                // Clip the last segment so it doesn't extend past the anchor.
                float remainingLength = chainLength - distance;
                int sourceHeight = (int)Math.Min(segmentLength, remainingLength);

                if (sourceHeight <= 0)
                    break;

                Rectangle sourceRect = new Rectangle(0, 0, chainTexture.Width, sourceHeight);
                Vector2 segmentOrigin = new Vector2(origin.X, 0f);
                Color chainColor = Lighting.GetColor((int)(drawPosition.X / 16f), (int)(drawPosition.Y / 16f));

                Main.EntitySpriteDraw(chainTexture, drawPosition - Main.screenPosition, sourceRect, chainColor, chainRotation, segmentOrigin, 1f, SpriteEffects.None, 0);
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
            writer.WriteVector2(direction);
            writer.Write(returnRotation);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
            direction = reader.ReadVector2();
            returnRotation = reader.ReadSingle();
        }

        private static void ForceNetUpdate(Projectile proj, bool ignoreCurrentNetSpam = true)
        {
            proj.netUpdate = true;
            if (proj.netSpam >= 10 || ignoreCurrentNetSpam)
            {
                proj.netSpam = 0;
            }
        }
    }
}
