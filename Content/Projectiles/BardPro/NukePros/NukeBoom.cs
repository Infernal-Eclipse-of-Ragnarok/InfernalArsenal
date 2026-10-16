using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;
using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NukePros
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class NukeBoom : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override string Texture =>
            "CalamityMod/Projectiles/InvisibleProj";

        private const int DefaultLifetime = 60;
        private bool initialized;

        private int Lifetime =>
            Projectile.ai[2] > 0f
                ? (int)Projectile.ai[2]
                : DefaultLifetime;

        // ai[0] = current radius
        // ai[1] = maximum radius

        private float CurrentRadius
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float MaxRadius => Projectile.ai[1];

        public override void SetStaticDefaults()
        {
        }

        public sealed override void SetBardDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = Lifetime;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;

                // Apply the lifetime passed through ai[2].
                Projectile.timeLeft = Lifetime;
            }

            float progress = 1f - Projectile.timeLeft / (float)Lifetime;

            float expansion = MathHelper.Clamp(progress * 2f, 0f, 1f);
            CurrentRadius = MaxRadius * expansion;

            int diameter = Math.Max(2, (int)(CurrentRadius * 2f));

            Vector2 center = Projectile.Center;
            Projectile.width = diameter;
            Projectile.height = diameter;
            Projectile.Center = center;

            Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }

        private static float Fadeout(float completion)
        {
            float fadeProgress = MathHelper.Clamp(completion * 2f, 0f, 1f);

            const float fadeExponent = 1f;

            return (float)Math.Pow(1f - fadeProgress, fadeExponent);
        }

        private static Color GetExplosionColor(float completion)
        {
            return Color.Lerp(
                Color.Yellow * 1.6f,
                Color.White,
                MathHelper.Clamp(completion * 2.2f, 0f, 1f)
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float completion = 1f - Projectile.timeLeft / (float)Lifetime;

            // Match the ForceField shader rendering used by Calamity.
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            Vector2 scale = new Vector2(1.5f, 1f);

            Vector2 drawPosition =
                Projectile.Center - Main.screenPosition
                + Projectile.Size * scale * 0.5f;

            Rectangle drawArea = new Rectangle(
                0,
                0,
                Projectile.width,
                Projectile.height
            );

            Color fadeColor = new Color(
                new Vector4(Fadeout(completion))
                * Projectile.Opacity
            );

            DrawData drawData = new DrawData(
                ModContent.Request<Texture2D>(
                    "Terraria/Images/Misc/Perlin",
                    AssetRequestMode.ImmediateLoad
                ).Value,
                drawPosition,
                drawArea,
                fadeColor,
                Projectile.rotation,
                Projectile.Size,
                scale,
                SpriteEffects.None,
                0
            );

            GameShaders.Misc["ForceField"]
                .UseColor(GetExplosionColor(completion));

            GameShaders.Misc["ForceField"].Apply(drawData);
            drawData.Draw(Main.spriteBatch);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            return false;
        }
    }
}
