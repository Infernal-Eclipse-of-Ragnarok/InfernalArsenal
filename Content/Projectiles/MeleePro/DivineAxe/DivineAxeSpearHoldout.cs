using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.DivineAxe
{
    public class DivineAxeSpearHoldout : BaseCustomUseStyleProjectile
    {
        public override int AssignedItemID => ModContent.ItemType<Items.Weapons.Melee.DivineAxe>();
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Items.Weapons.Melee.DivineAxe>();
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/DivineAxe";

        public override float HitboxOutset => 245f;
        public override Vector2 HitboxSize => new(40f, 40f);
        public override float HitboxRotationOffset => MathHelper.ToRadians(-45f);
        public override Vector2 SpriteOrigin => new(-5f, 125f);

        private Vector2 mousePos;
        private Vector2 aimVel;
        private Vector2 tipOutset;

        private bool doSwing = true;
        private bool postSwing;
        private bool fireProj = true;
        private bool sunActive;

        private float fadeIn;
        private float colorFadeIn;
        private float spearOutset;

        private int useAnim;
        private int swingCount;

        public bool InitSwing => swingCount % 7 == 0;

        public const float SunRadius = 100f;
        public const float AxeSpikeHeight = 150f;
        private float sunSpinTime;

        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void WhenSpawned()
        {
            Projectile.knockBack = 0f;
            Projectile.scale = 1f;
            Projectile.ai[1] = 1f;

            Projectile.rotation = Vector2.UnitY.ToRotation() + MathHelper.ToRadians(-135f);

            useAnim = Owner.itemAnimationMax;
            swingCount = 0;

            Owner.direction = mousePos.X < Owner.Center.X ? -1 : 1;
            FlipAsSword = Owner.direction == -1;

            CanHit = false;
            fireProj = true;
            doSwing = true;
        }

        public override void UseStyle()
        {
            if (Owner.HeldItem.type != AssignedItemID || !Main.mouseRight || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            Owner.Calamity().mouseWorldListener = true;
            Owner.Calamity().rightClickListener = true;

            tipOutset = Vector2.One.RotatedBy(Projectile.rotation + MathHelper.ToRadians(90f)) * spearOutset * 12f;
            AbsolutePosition = Owner.MountedCenter + tipOutset;
            AnimationProgress = Animation % useAnim;
            DrawUnconditionally = false;

            if (postSwing)
                mousePos = Owner.Center - aimVel;
            else
                mousePos = Owner.Calamity().mouseWorld;

            fadeIn = MathHelper.Lerp(fadeIn, 0f, 0.12f);
            colorFadeIn = MathHelper.Lerp(colorFadeIn, 0f, 0.07f);

            if (!doSwing)
                StartNextSwing();
            else
                UpdateCurrentSwing();

            ArmRotationOffset = MathHelper.ToRadians(-140f);
            ArmRotationOffsetBack = MathHelper.ToRadians(-140f);
        }

        private void StartNextSwing()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
                Projectile.localNPCImmunity[i] = 0;

            Projectile.numHits = 0;

            mousePos = Owner.Calamity().mouseWorld;
            aimVel = Owner.DirectionTo(mousePos) * -65f;

            postSwing = false;
            fireProj = true;
            doSwing = true;

            Owner.direction = mousePos.X < Owner.Center.X ? -1 : 1;
            FlipAsSword = Owner.direction == -1;

            swingCount++;

            useAnim = InitSwing ? (int)(Owner.itemAnimationMax * 2f) : Owner.itemAnimationMax;

            AnimationProgress = Animation % useAnim;
        }

        private void UpdateCurrentSwing()
        {
            if (!CanHit && !postSwing)
                Owner.direction = mousePos.X < Owner.Center.X ? -1 : 1;
            else
                Owner.direction = (Owner.Center - aimVel).X < Owner.Center.X ? -1 : 1;

            if (InitSwing)
                Projectile.rotation = Projectile.rotation.AngleLerp(Vector2.UnitY.ToRotation() + MathHelper.ToRadians(-135f), 0.22f);

            if (AnimationProgress < useAnim / 2f)
                UpdateWindup();
            else
                UpdateRelease();
        }

        private void UpdateWindup()
        {
            aimVel = Owner.DirectionTo(Owner.Calamity().mouseWorld) * -65f;

            postSwing = false;

            if (AnimationProgress == 0f)
                doSwing = false;

            spearOutset = MathHelper.Lerp(spearOutset, MathHelper.ToRadians(120f), 0.2f);
            FlipAsSword = Owner.DirectionTo(Owner.Calamity().mouseWorld).X < 0f;
        }

        private void UpdateRelease()
        {
            float time = AnimationProgress - useAnim / 3f;
            float timeMax = useAnim - useAnim / 3f;

            spearOutset = MathHelper.Lerp(spearOutset, MathHelper.ToRadians(120f), 0.2f);

            if (time == (int)(timeMax * 0.4f))
            {
                SoundEngine.PlaySound(new("InfernalEclipseWeaponsDLC/Assets/Sounds/DemonSwordSwing1"), Projectile.Center);
            }


            if (fireProj && time >= (int)(timeMax * 0.7f))
                FireSpecialProjectile();


            float easedOutset = MathHelper.Lerp(450f, 0f, CalamityUtils.ExpInOutEasing(time / timeMax, 1));
            spearOutset = MathHelper.Lerp(spearOutset, MathHelper.ToRadians(easedOutset), 0.2f);

            if (time >= timeMax)
            {
                doSwing = false;
            }

            postSwing = time < (int)(timeMax * 0.7f);
        }

        private void FireSpecialProjectile()
        {
            if (InitSwing)
            {
                Owner.SetScreenshake(3f);

                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/SkytideBolt") with
                {
                    Volume = 0.8f
                }, Projectile.Center);
                sunActive = true;

                swingCount = 0;
            }
            else
            {
                if (swingCount % 6 == 0)
                    sunActive = false;

                Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    Owner.Center + new Vector2(0f, -AxeSpikeHeight - SunRadius),
                    Vector2.Zero,
                    ModContent.ProjectileType<DivineAxeSunAura>(),
                    (int)(Projectile.damage * 0.33f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            colorFadeIn = 1f;
            fireProj = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if ((useAnim <= 0 && !DrawUnconditionally) || !Owner.ItemAnimationActive)
                return false;

            Asset<Texture2D> texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/DivineAxe");

            float drawRotationOffset = FlipAsSword ? MathHelper.ToRadians(90f) : 0f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Owner.gfxOffY);

            Main.EntitySpriteDraw(
                texture.Value,
                drawPosition,
                texture.Value.Frame(1, FrameCount, 0, Frame),
                lightColor,
                Projectile.rotation + RotationOffset + drawRotationOffset,
                FlipAsSword ? new Vector2(texture.Width() - SpriteOrigin.X, SpriteOrigin.Y) : SpriteOrigin,
                Projectile.scale,
                FlipAsSword ? SpriteEffects.FlipHorizontally : SpriteEffects.None
            );

            if (sunActive)
            {
                Effect sunShader = ModContent.Request<Effect>("InfernalEclipseWeaponsDLC/Assets/Effects/AphelionSun", AssetRequestMode.ImmediateLoad).Value;
                Texture2D noiseTex = TextureAssets.Extra[ExtrasID.FlameLashTrailShape].Value;

                Main.graphics.GraphicsDevice.Textures[1] = noiseTex;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                Main.graphics.GraphicsDevice.Textures[2] = TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion].Value;
                Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

                sunSpinTime += 0.04f;

                float pulse = 0.1f * MathF.Sin(Main.GameUpdateCount * 0.04f);
                float coronaIntensity = 0.5f + Math.Abs(pulse);

                sunShader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                sunShader.Parameters["spinTime"]?.SetValue(sunSpinTime % MathHelper.TwoPi);
                sunShader.Parameters["coronaIntensityFactor"]?.SetValue(coronaIntensity);
                sunShader.Parameters["mainColor"]?.SetValue(new Vector3(1f, 0.6f, 0f));
                sunShader.Parameters["darkerColor"]?.SetValue(new Vector3(0.5f, 0.1f, 0f));
                sunShader.Parameters["subtractiveAccentFactor"]?.SetValue(new Vector3(0.2f, 0.2f, 0.2f));

                float targetDiameter = SunRadius * 2f;
                Vector2 sunScale = new Vector2(targetDiameter / noiseTex.Width, targetDiameter / noiseTex.Height) * Projectile.scale;
                Vector2 sunDrawPos = Owner.MountedCenter + new Vector2(0f, -AxeSpikeHeight - SunRadius) - Main.screenPosition;
                Vector2 texOrigin = noiseTex.Size() * 0.5f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, sunShader, Main.GameViewMatrix.ZoomMatrix);
                sunShader.CurrentTechnique.Passes[0].Apply();
                Main.spriteBatch.Draw(noiseTex, sunDrawPos, null, Color.White, 0f, texOrigin, sunScale, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, sunShader, Main.GameViewMatrix.ZoomMatrix);
                sunShader.Parameters["coronaIntensityFactor"]?.SetValue(coronaIntensity * 1.4f);
                sunShader.CurrentTechnique.Passes[0].Apply();
                Main.spriteBatch.Draw(noiseTex, sunDrawPos, null, Color.White * 0.5f, 0f, texOrigin, sunScale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
