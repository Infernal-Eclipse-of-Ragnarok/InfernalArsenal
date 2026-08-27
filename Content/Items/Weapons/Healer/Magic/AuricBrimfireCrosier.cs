using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Items.BossThePrimordials.Aqua;
using ThoriumMod.Items.BossThePrimordials.Omni;
using ThoriumMod.Items.BossThePrimordials.Slag;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Magic
{
    [ExtendsFromMod("ThoriumMod")]
    public class AuricBrimfireCrosier : ThoriumItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            if (ModLoader.TryGetMod("YharimEX", out Mod _))
            {
                return false;
            }
            return base.IsLoadingEnabled(mod);
        }

        public override string Texture => ModLoader.TryGetMod("FargowiltasSouls", out _) ? "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Magic/AuricBrimfireCrosierSouls" : base.Texture;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(44, 48);

            Item.useTime = 5;
            // Item.reuseDelay = 4;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = new Terraria.Audio.SoundStyle("CalamityMod/Sounds/Custom/SCalSounds/BrothersDeath1");
            Item.damage = 15 * (ModLoader.TryGetMod("FargowiltasSouls", out _) ? ModLoader.TryGetMod("ssm", out _) ? 450 : 300 : 235);
            Item.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.mana = 30;
            radiantLifeCost = 2;

            Item.rare = ModLoader.TryGetMod("NoxusBoss", out Mod noxus) ? noxus.Find<ModRarity>("LotusOfCreationRarity").Type : ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;

            Item.shoot = ModContent.ProjectileType<Staff_Projectile>();
            Item.shootSpeed = 20;

            isHealer = true;
        }

        public override bool CanUseItem(Player player)
        {
            int lifeAfterUsing = player.statLife - radiantLifeCost * 4;
            return lifeAfterUsing > 0;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity * 3;
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("FargowiltasSouls", out _)) return;

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<ShadowspecBar>(10);
            recipe.AddIngredient<AshesofCalamity>(8);
            recipe.AddIngredient<InfernoEssence>();
            recipe.AddIngredient<DeathEssence>();
            recipe.AddIngredient<OceanEssence>();
            //recipe.AddIngredient(ModLoader.TryGetMod("NoxusBoss", out Mod wotg) ? wotg.Find<ModItem>("MetallicChunk").Type : ModContent.ItemType<Rock>());
            recipe.AddTile<SCalAltar>();
            recipe.Register();
        }
    }

    public class Staff_Projectile : ModProjectile
    {
        public int TicksBeforeTurn;
        public static Asset<Texture2D> screamTex;
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty"; //"InfernalEclipseWeaponsDLC/Content/Projectiles/HealerPro/Staff_Projectile";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            screamTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/ScreamyFace", AssetRequestMode.AsyncLoad);
        }

        public override void SetDefaults()
        {
            TicksBeforeTurn = 25; // 3 seconds

            Projectile.Size = new Vector2(72, 72);
            Projectile.timeLeft = TicksBeforeTurn * 5 - 5; // Shortened a bit so that it doesn't hit the  player
            Projectile.penetrate = 5;

            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.friendly = true;

            Projectile.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= TicksBeforeTurn)
            {
                Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(144)); // Make star shape
                Projectile.ai[0] = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Global scale: render everything at 15% of original size
            const float GlobalScale = 0.15f;

            // Shader + scream texture pass
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            lightColor.R = (byte)(255 * Projectile.Opacity);

            Main.spriteBatch.End();
            Effect shieldEffect = Filters.Scene["CalamityMod:HellBall"].GetShader().Shader;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

            float noiseScale = 0.6f;

            shieldEffect.Parameters["time"].SetValue(Projectile.timeLeft / 60f * 0.24f);
            shieldEffect.Parameters["blowUpPower"].SetValue(3.2f);
            shieldEffect.Parameters["blowUpSize"].SetValue(0.4f);
            shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

            float opacity = Projectile.Opacity;
            shieldEffect.Parameters["shieldOpacity"].SetValue(opacity);
            shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

            Color edgeColor = Color.Black * opacity;
            Color shieldColor = Color.Lerp(Color.Red, Color.Magenta, 0.5f) * opacity;

            shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
            shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

            Vector2 pos = Projectile.Center - Main.screenPosition;
            float scale = 0.715f;
            Main.spriteBatch.Draw(
                screamTex.Value,
                pos,
                null,
                Color.White,
                0,
                screamTex.Size() * 0.5f,
                scale * GlobalScale * Projectile.scale * Projectile.Opacity, // 15% size
                0,
                0
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D vortexTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/SoulVortex").Value;
            Texture2D centerTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                Color outerColor = Color.Lerp(Color.Red, Color.Magenta, i * 0.15f);
                Color drawColor = Color.Lerp(outerColor, Color.Black, i * 0.2f) * 0.5f;
                drawColor.A = 0;
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                drawPosition += (angle + Main.GlobalTimeWrappedHourly * i / 16f).ToRotationVector2() * 6f;

                Main.EntitySpriteDraw(
                    vortexTexture,
                    drawPosition,
                    null,
                    drawColor * Projectile.Opacity,
                    -angle + MathHelper.PiOver2,
                    vortexTexture.Size() * 0.5f,
                    Projectile.scale * (1 - i * 0.05f) * GlobalScale * Projectile.Opacity, // 15% size
                    SpriteEffects.None,
                    0
                );
            }

            Main.EntitySpriteDraw(
                centerTexture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.Black * Projectile.Opacity,
                Projectile.rotation,
                centerTexture.Size() * 0.5f,
                Projectile.scale * 0.9f * GlobalScale * Projectile.Opacity, // 15% size
                SpriteEffects.None,
                0
            );

            // Trail rendering
            Texture2D texture2D13 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = texture2D13.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = Projectile.GetAlpha(lightColor);
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = color26 * ((ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type]);
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                Main.EntitySpriteDraw(
                    texture2D13,
                    value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY),
                    rectangle,
                    color27,
                    num165,
                    origin2,
                    Projectile.scale * GlobalScale, // 15% size
                    SpriteEffects.None,
                    0
                );
            }

            // Current frame draw
            Main.EntitySpriteDraw(
                texture2D13,
                Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
                rectangle,
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                origin2,
                Projectile.scale * GlobalScale, // 15% size
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
