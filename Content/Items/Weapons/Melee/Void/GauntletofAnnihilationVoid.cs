using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.Void;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using SOTS;
using SOTS.Items.Chaos;
using SOTS.Void;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void
{
    [JITWhenModsEnabled("SOTS", "CalamityMod")]
    [ExtendsFromMod("SOTS", "CalamityMod")]
    public class GauntletofAnnihilationVoid : VoidItem
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Melee/Void/GauntletofAnnihilation";

        public override void SetStaticDefaults() => this.SetResearchCost(1);

        public override void SafeSetDefaults()
        {
            Item.damage = 225;
            Item.DamageType = DamageClass.Melee;
            Item.width = 28;
            Item.height = 26;
            Item.useTime = 7;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.UseSound = SoundID.Item19;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmicPunch>();
            Item.shootSpeed = 11f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.crit = 16;

            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool BeforeUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useAnimation = Item.useTime = 40;
                Item.shootSpeed = 1f;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.crit = 20;
                return player.ownedProjectileCounts[0] <= 0;
            }
            else
            {
                Item.useTime = player.GetModPlayer<InfernalWeaponsPlayer>().annihilationBonusShotTimeLeft > 0 ? 28 : 7;
                Item.useAnimation = 28;
                Item.shootSpeed = 11f;
                Item.useStyle = ItemUseStyleID.Shoot;
            }

            return base.BeforeUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GauntletofAnnihilationPunches>(), damage * 37, knockback, player.whoAmI);
            }
            else
            {
                Vector2 direction = Utils.RotatedBy(velocity, MathHelper.ToRadians(Utils.NextFloat(Main.rand, -15, 15f)), new Vector2());
                Projectile.NewProjectile(source, position.X, position.Y, direction.X, direction.Y, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void UpdateInventory(Player player)
        {
            if (!Item.favorited) return;
            Lighting.AddLight(player.Center, 1.1f, 0.9f, 1f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<Armaggedon>()
                .AddIngredient<PhosphorescentGauntlet>()
                .AddIngredient<CosmiliteBar>(10)
                .AddTile<CosmicAnvil>()
                .Register();
        }

        public override int GetVoid(Player player) => player.altFunctionUse == 2 ? 8 : 16;
    }
}
