using CalamityMod.Items;
using CalamityMod;
using CalamityMod.Projectiles.Magic;
using InfernalEclipseWeaponsDLC.Utilities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.BarrenGarden;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Utilities;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.CustomRecipes;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Hybrid
{
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class BarrenGarden : ThoriumItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void SetDefaults()
        {
            Item.damage = 300;
            Item.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
            Item.noMelee = true;
            Item.mana = 15;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;

            Item.rare = ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.UseSound = SoundID.Item32;

            Item.shoot = ModContent.ProjectileType<BarrenGardenPro>();
            Item.shootSpeed = 22f;

            isHealer = true;
            healType = HealType.Ally;
            healAmount = 1;
            healDisplay = true;

            Item.noUseGraphic = true;
            Item.knockBack = 4f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int owner = player.whoAmI;

            // Right-click - only one lotus active
            if (player.altFunctionUse == 2)
            {
                Vector2 mouseWorld = Main.MouseWorld;

                // Kill old Lotus if one already exists
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<BarrenGardenLotus>())
                    {
                        proj.Kill();
                    }
                }

                // Find the nearest walkable tile (solid OR platform) below cursor
                int tileX = (int)(mouseWorld.X / 16f);
                int tileY = (int)(mouseWorld.Y / 16f);

                for (int y = tileY; y < Main.maxTilesY - 10; y++)
                {
                    Tile tile = Main.tile[tileX, y];
                    if (tile == null)
                        continue;

                    bool walkable =
                        tile.HasTile &&
                        (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]);

                    if (walkable)
                    {
                        // Snap lotus bottom to top of that tile
                        mouseWorld.Y = y * 16f - 8f;

                        // Center horizontally on tile (adds 8px)
                        mouseWorld.X = tileX * 16f + 8f;
                        break;
                    }
                }

                Projectile.NewProjectile(
                    source,
                    mouseWorld,
                    Vector2.Zero,
                    ModContent.ProjectileType<BarrenGardenLotus>(),
                    damage,
                    knockback,
                    player.whoAmI
                );

                return false;
            }

            // --- Left click (original firing pattern) ---
            float[] healingAngles = { -21f, 21f };
            foreach (float angle in healingAngles)
            {
                Vector2 vel = velocity.RotatedBy(MathHelper.ToRadians(angle));
                Projectile.NewProjectile(source, position, vel * 0.975f, ModContent.ProjectileType<BarrenGardenHealingPro>(), 0, knockback, owner);
            }

            float[] homingAngles = { -17f, -13f, 13f, 17f };
            foreach (float angle in homingAngles)
            {
                Vector2 vel = velocity.RotatedBy(MathHelper.ToRadians(angle));
                Projectile.NewProjectile(source, position, vel, ModContent.ProjectileType<BarrenGardenProHoming>(), damage, knockback, owner);
            }

            float[] normalAngles = { -9f, -3f, 3f, 9f };
            foreach (float angle in normalAngles)
            {
                Vector2 vel = velocity.RotatedBy(MathHelper.ToRadians(angle));
                Projectile.NewProjectile(source, position, vel * 1.05f, ModContent.ProjectileType<BarrenGardenPro>(), damage, knockback, owner);
            }

            return false;
        }

        public override void AddRecipes()
        {
            // Always get Thorium (we depend on it anyway)
            Mod thorium = ModLoader.GetMod("ThoriumMod");
            Mod calamity = null;
            Mod clamity = null;
            Mod ragnarok = null;

            // Try to safely get Calamity and Ragnarok
            ModLoader.TryGetMod("CalamityMod", out calamity);
            ModLoader.TryGetMod("Clamity", out clamity);
            ModLoader.TryGetMod("RagnarokMod", out ragnarok);

            // Start the recipe builder
            Recipe recipe = CreateRecipe();

            // --- Ingredient Logic ---
            if (ragnarok != null)
            {
                recipe.AddIngredient(ragnarok.Find<ModItem>("VerdurantBloom").Type, 1);
            }
            else
            {
                recipe.AddIngredient(thorium.Find<ModItem>("BalanceBloom").Type, 1);
            }

            recipe.AddIngredient(thorium.Find<ModItem>("SamsaraLotus").Type, 1);
            recipe.AddIngredient(thorium.Find<ModItem>("DivineLotus").Type, 1);

            if (clamity != null)
            {
                recipe.AddIngredient(clamity.Find<ModItem>("EnchantedMetal").Type, 8);
            }
            else
            {
                recipe.AddIngredient(calamity.Find<ModItem>("AuricBar").Type, 8);
                recipe.AddIngredient(calamity.Find<ModItem>("EndothermicEnergy").Type, 20);
            }

            // --- Tile and output setup ---
            recipe.AddTile(ModContent.TileType<CosmicAnvil>());

            recipe.Register();
        }

    }
}