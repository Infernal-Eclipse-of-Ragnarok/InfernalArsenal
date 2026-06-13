using System;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.DivineAxe
{
    public class DivineAxeSunAura : ModProjectile
    {
        public override string Texture => "CalamityMod/Particles/SmallBloomRing";

        public float DarkLifetimeCompletion => 1f - Projectile.timeLeft / 60f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 90;
            Projectile.timeLeft = 60;
            Projectile.scale = 0.001f;
        }

        public override void AI()
        {
            Projectile.Center = Main.player[Projectile.owner].Center + new Vector2(0f, -DivineAxeSpearHoldout.AxeSpikeHeight - DivineAxeSpearHoldout.SunRadius);

            Projectile.ai[0]++;

            // Initial random rotation setup
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.rotation = Utils.NextFloat(Main.rand, MathHelper.TwoPi);
                Projectile.localAI[0] = Utils.ToDirectionInt(Utils.NextBool(Main.rand));
                Projectile.netUpdate = true;
            }

            Projectile.Opacity = (1f - (float)Math.Pow(DarkLifetimeCompletion, 1.56)) * 0.4f;
            Projectile.scale = MathHelper.Lerp(0.1f, 15f, DarkLifetimeCompletion);
            Projectile.rotation += Projectile.localAI[0] * 0.012f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Effect sunShader = ModContent.Request<Effect>("InfernalEclipseWeaponsDLC/Assets/Effects/AphelionSun", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D noiseTex = TextureAssets.Extra[ExtrasID.FlameLashTrailShape].Value;

            Main.graphics.GraphicsDevice.Textures[1] = noiseTex;
            Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Main.graphics.GraphicsDevice.Textures[2] = TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion].Value;
            Main.graphics.GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;

            float pulse = 0.1f * MathF.Sin(Main.GameUpdateCount * 0.04f);
            float coronaIntensity = 0.5f + Math.Abs(pulse);

            sunShader.Parameters["globalTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            sunShader.Parameters["spinTime"]?.SetValue(Projectile.rotation % MathHelper.TwoPi);
            sunShader.Parameters["coronaIntensityFactor"]?.SetValue(coronaIntensity);
            sunShader.Parameters["mainColor"]?.SetValue(new Vector3(1f, 1f, 0.6f));
            sunShader.Parameters["darkerColor"]?.SetValue(new Vector3(0.8f, 0.7f, 0.2f));
            sunShader.Parameters["subtractiveAccentFactor"]?.SetValue(new Vector3(0.1f, 0.1f, 0.05f));

            Vector2 origin = texture.Size() * 0.5f;
            Color drawColor = new Color(255, 255, 180) * Projectile.Opacity;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearWrap,
                DepthStencilState.None,
                Main.Rasterizer,
                sunShader,
                Main.GameViewMatrix.ZoomMatrix
            );

            sunShader.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < 8; i++)
            {
                float rotation = Projectile.rotation;
                Vector2 drawOffset = MathHelper.TwoPi
                    .ToRotationVector2();

                drawOffset = ((MathHelper.TwoPi * i / 8f).ToRotationVector2()) * Projectile.scale;

                Vector2 drawPosition = Projectile.Center - Main.screenPosition + drawOffset;

                if (i % 2 == 1)
                    rotation *= -1f;

                Main.spriteBatch.Draw(
                    texture,
                    drawPosition,
                    null,
                    drawColor,
                    rotation,
                    origin,
                    Projectile.scale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            return false;
        }

        public override bool? CanHitNPC(NPC target) => !target.CountsAsACritter && !target.friendly && target.chaseable;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Daybreak, 300);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.scale * 48f, targetHitbox);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}
