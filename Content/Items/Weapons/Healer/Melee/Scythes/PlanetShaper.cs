using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Items.Titan;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes
{
    public class PlanetShaper : ScytheItem
    {
        private int swingDirection;
        internal int spin;

        private Mod catalyst = null;
        private Mod calBardHealer = null;

        public override void SetStaticDefaults()
        {
            SetDefaultsToScythe();
            ItemID.Sets.SkipsInitialUseSound[Type] = true;

            ModLoader.TryGetMod("CatalystMod", out catalyst);
            ModLoader.TryGetMod("CalamityBardHealer", out calBardHealer);
        }

        public override void SetDefaults()
        {
            SetDefaultsToScythe();
            Item.damage = 500;
            scytheSoulCharge = 5;
            Item.width = 75;
            Item.height = 110;
            Item.rare = catalyst != null ? catalyst.Find<ModRarity>("SuperbossMasterRarity").Type : ModContent.RarityType<BurnishedAuric>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.shoot = ModContent.ProjectileType<PlanetShaperPro>();
            Item.holdStyle = 6;
            Item.useStyle = 100;
            Item.noUseGraphic = false;
            Item.useTime = Item.useAnimation = 36;
        }

        //Credit for this logic goes to Unowen
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (swingDirection != -1 && swingDirection != 1)
                swingDirection = 1;
            if (Main.myPlayer == player.whoAmI)
            {
                float num = player.itemAnimationMax > 0 ? (player.itemAnimationMax > player.itemTimeMax ? player.itemTimeMax : player.itemAnimationMax) : Item.useAnimation;
                int index = Projectile.NewProjectile(source, position, Vector2.Normalize(Main.MouseWorld - player.MountedCenter), type, damage, knockback, player.whoAmI, num, num, player.GetAdjustedItemScale(Item));
                Main.projectile[index].direction = swingDirection;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
            swingDirection = -swingDirection;
            if (++spin > 3)
                spin = 1;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(catalyst != null ? calBardHealer != null ? calBardHealer.Find<ModItem>("Singularity").Type : ModContent.ItemType<TitanScythe>() : ModContent.ItemType<TitanScythe>())
                .AddIngredient<YharonSoulFragment>(4)
                .AddIngredient<ExodiumCluster>(15)
                .AddTile<CosmicAnvil>()
                .Register();
        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation += new Vector2(-15f, 12f) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle itemFrame)
        {
            player.itemLocation = Vector2.Zero;
        }

        public override bool MeleePrefix() => true;
    }
}
