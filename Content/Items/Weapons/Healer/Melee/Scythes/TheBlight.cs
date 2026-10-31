using CalamityMod.Items;
using CalamityMod.Items.Potions;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes;
using InfernalEclipseWeaponsDLC.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Tiles;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes
{
    public class TheBlight : ScytheItem
    {
        public override void SetStaticDefaults()
        {
            SetStaticDefaultsToScythe();
        }

        public override void SetDefaults()
        {
            SetDefaultsToScythe();

            Item.damage = 140;
            scytheSoulCharge = 2;
            Item.width = 70;
            Item.height = 82;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<TheBlightProScythe>();
            Item.autoReuse = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<TheBlightProServant>()] < 1)
                {
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<TheBlightProServant>(), Item.damage, Item.knockBack, Main.myPlayer, ai0: 0);
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<TheBlightProServant>(), Item.damage, Item.knockBack, Main.myPlayer, ai0: 1);
                }
            }

            base.HoldItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 sickleVelocity = (Main.MouseWorld - position).SafeNormalize(default) * 20f;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                position,
                sickleVelocity,
                ModContent.ProjectileType<TheBlightProSickle>(),
                damage,
                knockback,
                player.whoAmI,
                ai0: 200
            );

            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Main.myPlayer)
                {
                    if (proj.ModProjectile is TheBlightProServant servant)
                    {
                        servant.Shoot(Item.damage, Item.knockBack);
                    }
                }
            }

            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void AddRecipes()
        {
            if (!ModLoader.TryGetMod("Consolaria", out Mod consolaria))
            {
                var recipe = CreateRecipe()
                    .AddIngredient<AureusCell>(10)
                    .AddIngredient<HallowedScythe>()
                    .AddIngredient(ItemID.SoulofSight, 5)
                    .AddIngredient(ItemID.SoulofMight, 5)
                    .AddIngredient(ItemID.SoulofFright, 5)
                    .AddIngredient(ItemID.SoulofNight, 8)
                    .AddIngredient(ItemID.Bone, 12)
                    .AddIngredient(ItemID.CursedFlame, 8);

                recipe.AddTile(ModContent.TileType<SoulForgeNew>())
                      .Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient<HallowedScythe>()
                    .AddRecipeGroup(ArsenalRecipeSystem.Titanium, 10)
                    .AddIngredient(consolaria.Find<ModItem>("SoulofBlight").Type, 15)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
            }
        }

    }
}
