using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.RoguePro;
using InfernalEclipseWeaponsDLC.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Items.BossThePrimordials.Dream;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Items.ThrownItems;
using ThoriumMod.Tiles;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Rogue
{
    public class OcramKnife : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 36;
            Item.damage = 90;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<OcramKnifePro>();
            Item.shootSpeed = 20f;
            Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Item.rare = ItemRarityID.Lime;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        }

        public override float StealthDamageMultiplier => 1.2f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealth.WithinBounds(Main.maxProjectiles))
                    Main.projectile[stealth].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            if (!ModLoader.TryGetMod("Consolaria", out Mod consolaria))
            {
                CreateRecipe()
                    .AddIngredient<Carnwennan>()
                    .AddIngredient<AureusCell>(10)
                    .AddIngredient(ItemID.SoulofSight, 5)
                    .AddIngredient(ItemID.SoulofMight, 5)
                    .AddIngredient(ItemID.SoulofFright, 5)
                    .AddIngredient(ItemID.SoulofNight, 8)
                    .AddIngredient(ItemID.Bone, 12)
                    .AddIngredient(ItemID.CursedFlame, 8)
                    .AddTile(ModContent.TileType<SoulForgeNew>())
                    .Register();
            }
            else
            {
                CreateRecipe()
                    .AddIngredient<Carnwennan>()
                    .AddRecipeGroup(ArsenalRecipeSystem.Titanium, 10)
                    .AddIngredient(consolaria.Find<ModItem>("SoulofBlight").Type, 15)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
            }
        }
    }
}
