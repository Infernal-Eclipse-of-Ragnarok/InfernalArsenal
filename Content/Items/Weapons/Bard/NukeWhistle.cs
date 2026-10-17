using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod.Empowerments;
using ThoriumMod;
using ThoriumMod.Items;
using Microsoft.Xna.Framework;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NukePros;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using ThoriumMod.Sounds;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Terraria.Audio;
using CalamityMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class NukeWhistle : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<ResourceMaximum>(7);
            Empowerments.AddInfo<ResourceRegen>(7);
            Empowerments.AddInfo<ResourceGrabRange>(7);
            Empowerments.AddInfo<Damage>(7);
            Empowerments.AddInfo<CriticalStrikeChance>(7);
        }

        public override void SetBardDefaults()
        {
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.width = 20;
            Item.height = 38;
            Item.shootSpeed = 5;
            Item.shoot = ModContent.ProjectileType<NukeMissile>();
            // Item.Size = new Vector2(38, 31);
            Item.autoReuse = true;
            Item.damage = 800;
            Item.knockBack = 4f;
            Item.noMelee = true;
            Item.UseSound = ThoriumSounds.Whistle_Sound;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            InspirationCost = 10;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(8, -10);
        }

        public override void UseItemFrame(Player player)
        {
            ((ModItem)this).HoldItemFrame(player);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
            {
                if (thoriumMod.TryFind("WoodenWhistle", out ModItem whistle))
                {
                    recipe.AddIngredient(whistle);
                }
            }
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
            {
                if (calamity.TryFind("ExoPrism", out ModItem prism))
                {
                    recipe.AddIngredient(prism, 8);
                }
                recipe.AddIngredient(ItemID.FireworksBox, 1);
                recipe.AddIngredient(ItemID.MiniNukeI, 999);
                if (calamity.TryFind("ShadowspecBar", out ModItem bar))
                {
                    recipe.AddIngredient(bar, 5);
                }
            }
            else
            {
                recipe.AddIngredient(ItemID.FireworksBox, 1);
                recipe.AddIngredient(ItemID.MiniNukeI, 1000);
            }

            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            base.AddRecipes();
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            InfernalWeaponsPlayer modPlayer =
                player.GetModPlayer<InfernalWeaponsPlayer>();

            modPlayer.missileIndex--;

            bool fireBigNuke = modPlayer.missileIndex <= 0;

            if (fireBigNuke)
                modPlayer.missileIndex = 4;

            int projectileType = fireBigNuke
                ? ModContent.ProjectileType<NukePro>()
                : ModContent.ProjectileType<NukeMissile>();

            int projectileDamage = fireBigNuke
                ? (int)(damage * 5f)
                : damage;

            int sign = Main.rand.NextBool() ? 1 : -1;

            int xOffset = (int)((Main.MouseWorld.X - player.Center.X) / 3f)
                + Main.rand.Next(-4, 8) * 65 * sign;

            if (fireBigNuke)
            {
                Projectile.NewProjectile(
                    source,
                    player.Center + new Vector2(xOffset, (-Main.screenHeight / 2f) * 1.5f),
                    (new Vector2((Main.MouseWorld.X - (player.Center.X + xOffset)) / 37f, 7f) * (Item.shootSpeed / 4)),
                    projectileType,
                    projectileDamage,
                    knockback,
                    player.whoAmI,
                    Item.useTime
                );

                PlayTargetSound(player);
                return false;
            }

            // Fire the first missile immediately.
            Projectile.NewProjectile(
                source,
                player.Center + new Vector2(xOffset, (-Main.screenHeight / 2f) * 1.5f),
                (new Vector2((Main.MouseWorld.X - (player.Center.X + xOffset)) / 37f, 7f) * (Item.shootSpeed / 3)),
                projectileType,
                projectileDamage,
                knockback,
                player.whoAmI
            );

            // Schedule two more missiles at 10-tick intervals.
            modPlayer.nukeWhistleMissilesRemaining = 2;
            modPlayer.nukeWhistleMissileTimer = Item.useAnimation / 4;
            modPlayer.nukeWhistleMissileSetTimer = Item.useAnimation / 4;
            modPlayer.nukeWhistleVelocity = (new Vector2((Main.MouseWorld.X - (player.Center.X + xOffset)) / 37f, 7f) * (Item.shootSpeed / 3));
            modPlayer.nukeWhistleProjectileType = projectileType;
            modPlayer.nukeWhistleDamage = projectileDamage;
            modPlayer.nukeWhistleKnockback = knockback;

            return false;
        }

        public static void PlayTargetSound(Player player)
        {
            SoundEngine.PlaySound(
                new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/ApolloArtemisTargetSelection")
                with
                { Volume = 1f },
                player.position
            );
        }
    }
}
