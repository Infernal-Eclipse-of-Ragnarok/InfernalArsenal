using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.SentryPro.Void;
using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using SOTS.Void;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Void
{
    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public class VeryVeryStrangeSkullVoid : VoidItem
    {
        public static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/Void/VeryVeryStrangeSkull";

        public override void SafeSetDefaults()
        {
            Item.damage = 1100;
            Item.DamageType = DamageClass.Summon;
            Item.sentry = true;
            Item.mana = 10;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.knockBack = 8f;
            Item.rare = (Calamity != null) ? Calamity.Find<ModRarity>("PureGreen").Type : ItemRarityID.Purple;
            Item.value = Item.buyPrice(1, 75);
            Item.UseSound = new("InfernalEclipseWeaponsDLC/Assets/Sounds/GasterCharge") { PitchVariance = 0.5f };
            Item.shoot = ModContent.ProjectileType<VeryVeryStrangeSkullPro>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI).originalDamage = Item.damage;
            player.UpdateMaxTurrets();
            return false;
        }

        public override int GetVoid(Player player)
        {
            return 4;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            bool strangeSkullInRecipe = false;
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
            {
                strangeSkullInRecipe = ThoriumHelper.IsStrangeSkullToggled();
            }

            if (thorium != null && strangeSkullInRecipe)
            {
                recipe.AddIngredient(thorium.Find<ModItem>("StrangeSkull"));
            }
            else
            {
                recipe.AddIngredient(ItemID.Ectoplasm, 5);
                recipe.AddIngredient(ItemID.Bone, 5);
            }

            if (Calamity != null)
            {
                recipe.AddIngredient(Calamity.Find<ModItem>("RuinousSoul").Type, 5);
                recipe.AddIngredient(Calamity.Find<ModItem>("DarkPlasma").Type, 3);
            }
            else
            {
                recipe.AddIngredient(ItemID.LunarBar, 8);
            }

            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
