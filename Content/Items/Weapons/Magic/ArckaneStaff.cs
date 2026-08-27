using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Magic
{
    public class ArckaneStaff : ModItem
    {
        public const int HoldoutDistance = 55;

        private static Asset<Texture2D> glowTexture;
        public override void Load()
        {
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 6));
            Item.staff[Item.type] = true;
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 110;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.width = 70;
            Item.height = 62;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 7f;
            Item.rare = ItemRarityID.Yellow;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item43;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<ArckaneStaffHoldout>();
            Item.shootSpeed = 12f;
            Item.channel = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModLoader.TryGetMod("ThoriumMod", out Mod thor) ? thor.Find<ModItem>("MagickStaff").Type : ItemID.DiamondStaff);
            recipe.AddIngredient<AstralBar>(8);
            recipe.AddIngredient<AshesofCalamity>(5);
            recipe.AddIngredient<DepthCells>(5);
            if (thor != null) recipe.AddIngredient(thor.Find<ModItem>("AbyssalChitin"), 3);
            recipe.AddIngredient<InfectedArmorPlating>(3);
            recipe.AddIngredient<Voidstone>(3);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = glowTexture.Value;
            Rectangle frame = Main.itemAnimations[Type].FrameCount > 1 ? Main.itemAnimations[Item.type].GetFrame(texture, Main.itemFrameCounter[whoAmI]) : texture.Frame();
            Vector2 origin = frame.Size() / 2f;
            Vector2 DrawCenter = Item.Bottom - Main.screenPosition - new Vector2(0, origin.Y);
            spriteBatch.Draw
            (
                texture,
                DrawCenter,
                frame,
                Color.White,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
