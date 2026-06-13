using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfernalEclipseWeaponsDLC.Content.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod.Empowerments;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Tiles;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.RestoredDeepSeaDrawl;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.ExecutionersSword;
using Terraria.DataStructures;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class RestoredDeepSeaDrawl : BardItem
    {
        public const int numTyphoons = 3;
        public const float typhoonSpread = MathHelper.PiOver4 / 2; // 22.5 degrees
        public const int altUseCdMax = 30;

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<Damage>(2);
            Empowerments.AddInfo<CriticalStrikeChance>(3);
            Empowerments.AddInfo<AttackSpeed>(2);
            // Right click support
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetBardDefaults()
        {
            Item.damage = 76;
            Item.DamageType = ModContent.GetInstance<BardDamage>();
            Item.width = 48;
            Item.height = 36;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.buyPrice(gold: 40);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item84;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<OurSharknado>();
            Item.shootSpeed = 0f;
            InspirationCost = 3;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
        }

        public override void UseItemFrame(Player player)
        {
            ((ModItem)this).HoldItemFrame(player);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanPlayInstrument(Player player)
        {
            // Prevent multiple nados unless using alt
            return player.altFunctionUse == 2 || player.ownedProjectileCounts[ModContent.ProjectileType<OurSharknado>()] == 0;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 0 * player.direction;
            player.itemLocation.Y += 0;
        }

        public override bool? BardUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                InspirationCost = 2;
            }
            else
            {
                InspirationCost = 3;
            }
            return base.BardUseItem(player);
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Alternate use: Spread of Typhoon projectiles
                float baseAngle = (Main.MouseWorld - player.Center).ToRotation();
                float startAngle = baseAngle - typhoonSpread * (numTyphoons - 1) / 2f;
                float shootSpeed = 5f;

                var thorPlayer = player.GetModPlayer<ThoriumPlayer>();
                for (int i = 0; i < numTyphoons; i++)
                {
                    float angle = startAngle + i * typhoonSpread;
                    Vector2 typhoonVelocity = Vector2.UnitX.RotatedBy(angle) * shootSpeed;
                    var proj = Projectile.NewProjectileDirect(
                        source,
                        player.Center,
                        typhoonVelocity,
                        ModContent.ProjectileType<SharknadoBolt>(),
                        damage / 2,
                        knockback,
                        player.whoAmI
                    );
                    proj.friendly = true;
                    proj.hostile = false;
                    proj.DamageType = ModContent.GetInstance<BardDamage>();
                }
                // Prevent base shoot
                return false;
            }
            else
            {
                // Main use: Summon Sharknado at cursor if one doesn't already exist
                var proj = Projectile.NewProjectileDirect(
                    source,
                    Main.MouseWorld,
                    Vector2.Zero,
                    ModContent.ProjectileType<OurSharknado>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
                proj.friendly = true;
                proj.hostile = false;
                // Only one
                return false;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DeepSeaDrawl>())
                .AddIngredient(ModContent.ItemType<DeepSeaDrawlShard1>())
                .AddIngredient(ModContent.ItemType<DeepSeaDrawlShard2>())
                .AddIngredient(ModContent.ItemType<DeepSeaDrawlShard3>())
                .AddTile(ModContent.TileType<SoulForgeNew>())
                .Register();
        }
    }
}
