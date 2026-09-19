using CalamityMod;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee
{
    public class Stick : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 600; //Sharpness V
            Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();

            Item.width = 32;
            Item.height = 32;

            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.knockBack = 15f; //Knockback IV
            Item.ArmorPenetration = 15; //Breach IV

            Item.value = Item.buyPrice(1, 20);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item1;

            Item.noMelee = false;
            Item.noUseGraphic = false;

            //Items are unbreakable in terraria so mending and unbreaking mean nothing other than flavor.
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 10); //Fire Aspect II
        }

        #region Drawing
        private static Asset<Texture2D> _glintTex;
        private static Effect _glintFx;

        private static void EnsureAssetsLoaded()
        {
            if (_glintTex == null || !_glintTex.IsLoaded)
                _glintTex = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/Enchanted", AssetRequestMode.ImmediateLoad);

            if (_glintFx == null)
                _glintFx = ModContent.Request<Effect>("InfernalEclipseWeaponsDLC/Assets/Effects/Transform", AssetRequestMode.ImmediateLoad).Value;
        }

        public override bool PreDrawInInventory(
            SpriteBatch sb,
            Vector2 position,
            Rectangle frame,
            Color drawColor,
            Color itemColor,
            Vector2 origin,
            float scale)
        {
            EnsureAssetsLoaded();

            _glintFx.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 0.2f);
            _glintFx.CurrentTechnique.Passes["EnchantedPass"].Apply();
            Main.instance.GraphicsDevice.Textures[1] = _glintTex.Value;

            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                sb.GraphicsDevice.BlendState,
                sb.GraphicsDevice.SamplerStates[0],
                sb.GraphicsDevice.DepthStencilState,
                sb.GraphicsDevice.RasterizerState,
                _glintFx,
                Main.UIScaleMatrix
            );

            return true; // let vanilla draw the item with our active effect
        }

        public override void PostDrawInInventory(
            SpriteBatch sb,
            Vector2 position,
            Rectangle frame,
            Color drawColor,
            Color itemColor,
            Vector2 origin,
            float scale)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                sb.GraphicsDevice.BlendState,
                sb.GraphicsDevice.SamplerStates[0],
                sb.GraphicsDevice.DepthStencilState,
                sb.GraphicsDevice.RasterizerState,
                null,
                Main.UIScaleMatrix
            );
        }

        public override bool PreDrawInWorld(
            SpriteBatch sb,
            Color lightColor,
            Color alphaColor,
            ref float rotation,
            ref float scale,
            int whoAmI)
        {
            EnsureAssetsLoaded();

            _glintFx.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 0.2f);
            _glintFx.CurrentTechnique.Passes["EnchantedPass"].Apply();
            Main.instance.GraphicsDevice.Textures[1] = _glintTex.Value;

            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                Main.spriteBatch.GraphicsDevice.BlendState,
                sb.GraphicsDevice.SamplerStates[0],
                Main.spriteBatch.GraphicsDevice.DepthStencilState,
                sb.GraphicsDevice.RasterizerState,
                _glintFx,
                Main.GameViewMatrix.TransformationMatrix
            );

            return true; // let vanilla draw the world item with our active effect
        }

        public override void PostDrawInWorld(
            SpriteBatch sb,
            Color lightColor,
            Color alphaColor,
            float rotation,
            float scale,
            int whoAmI)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                sb.GraphicsDevice.BlendState,
                sb.GraphicsDevice.SamplerStates[0],
                sb.GraphicsDevice.DepthStencilState,
                sb.GraphicsDevice.RasterizerState,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }
        #endregion
    }

    #region Shimmer Adjustments
    public class NewShimmerRulesSystem : ModSystem
    {
        public override void Load()
        {
            On_Item.CanShimmer += CanShimmerHook;
        }

        public override void Unload()
        {
            On_Item.CanShimmer -= CanShimmerHook;
        }

        private static bool CanShimmerHook(On_Item.orig_CanShimmer orig, Item self)
        {
            if (self.type == ItemID.WoodenSword && !NPC.downedMoonlord)
                return false;

            return orig(self);
        }
    }

    public class WoodenSwordShimmerToStick : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.WoodenSword;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[ItemID.WoodenSword] = ModContent.ItemType<Stick>();
        }
    }
    #endregion

    #region Player Drawing
    public sealed class HeldItemGlowLayer : PlayerDrawLayer //this doesn't work but someone can try to fix it. not too important though.
    {
        public override Position GetDefaultPosition() =>
            new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var p = drawInfo.drawPlayer;

            if (p.HeldItem?.type != ModContent.ItemType<Stick>()) return base.GetDefaultVisibility(drawInfo);

            // Only visible if actually holding an item and using it (swinging or channeling)
            return !p.dead && p.HeldItem?.type == ModContent.ItemType<Stick>() && (p.itemAnimation > 0 || p.channel);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player p = drawInfo.drawPlayer;
            Item item = p.HeldItem;
            if (item == null || !(item.type == ModContent.ItemType<Stick>()) || item.ModItem == null)
                return;

            // Try to get "<itemTexture>_Glow"
            if (!ModContent.RequestIfExists<Texture2D>(item.ModItem.Texture + "_Glow", out var glowAsset, AssetRequestMode.ImmediateLoad))
                return;

            Texture2D glow = glowAsset.Value;

            // Pulse brightness
            float pulse = 0.6f + 0.4f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Color glowColor = new Color(255, 255, 255, 0) * pulse;

            // Match vanilla held-item draw
            // ItemLocation is where vanilla draws from, origin is bottom-left of the texture by default.
            Vector2 pos = drawInfo.ItemLocation - Main.screenPosition;
            float rotation = p.itemRotation + (p.direction < 0 ? MathHelper.Pi : 0f);
            SpriteEffects fx = (p.direction < 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Origin matches vanilla: (0, texture.Height)
            Vector2 origin = new Vector2(0f, glow.Height);

            // Respect vanilla scaling for held items
            float scale = p.GetAdjustedItemScale(item);

            // Draw
            drawInfo.DrawDataCache.Add(new DrawData(
                glow,
                pos,
                null,
                glowColor,
                rotation,
                origin,
                scale,
                fx,
                0
            ));
        }
        #endregion
    }
}
