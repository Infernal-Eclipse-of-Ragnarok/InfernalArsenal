using InfernalEclipseWeaponsDLC.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Dusts
{
    public class GammaDust : ModDust
    {
        /// <summary>
        ///     The color of the dust.
        /// </summary>
        public static readonly Color Color = new(49, 137, 96);

        public static Asset<Texture2D> Asset { get; private set; }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            if (Main.dedServ)
            {
                return;
            }

            Asset = ModContent.Request<Texture2D>(Texture);
        }

        public override void OnSpawn(Dust dust)
        {
            base.OnSpawn(dust);

            dust.frame = new Rectangle(0, 10 * Main.rand.Next(3), 10, 10);

            dust.color = Color;
        }

        public override bool Update(Dust dust)
        {
            if (dust.scale <= 0f)
            {
                dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;

                dust.velocity *= 0.8f;

                dust.scale -= 0.1f;
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            GammaRenderingSystem.Queue(() => DrawDust(dust));

            return false;
        }

        private void DrawDust(Dust dust)
        {
            var texture = Asset.Value;

            var color = dust.GetAlpha(dust.color);
            var origin = dust.frame.Size() / 2f;

            var position = dust.position - Main.screenPosition;

            Main.EntitySpriteDraw(texture, position, dust.frame, color, dust.rotation, origin, dust.scale, SpriteEffects.None);
        }
    }
}