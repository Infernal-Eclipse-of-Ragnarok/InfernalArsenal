using CalamityMod.Items;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Potions;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.Legacy;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod.Sounds;
using ThoriumMod.Tiles;
using ThoriumMod.Items.BardItems;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class Legacy : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<Damage>(3);
            Empowerments.AddInfo<CriticalStrikeChance>(3);
        }

        public override void SetBardDefaults()
        {
            Item.damage = 100;
            InspirationCost = 2;
            Item.width = 61;
            Item.height = 33;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.UseSound = ThoriumSounds.BugleHorn_Sound;
            Item.shoot = ModContent.ProjectileType<LegacyProBolt>();
            Item.shootSpeed = 15f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(6, -10);
        }

        public override void UseItemFrame(Player player)
        {
            ((ModItem)this).HoldItemFrame(player);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitX);

            for (int i = 0; i < 5; i++)
            {
                float angleOffset = MathHelper.Lerp(MathHelper.ToRadians(-15f), MathHelper.ToRadians(15f), i / 4f);

                Vector2 angledVelocity = direction.RotatedBy(angleOffset) * 15f;

                if ((i == 0) || (i == 4))
                {
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, angledVelocity, ModContent.ProjectileType<LegacyProBolt>(), damage, knockback, player.whoAmI);
                }
                else
                {
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, angledVelocity, ModContent.ProjectileType<LegacyProSickle>(), damage, knockback, player.whoAmI);
                }
                    
            }

            return false;
        }

        public override void AddRecipes()
        {
            if (!ModLoader.TryGetMod("Consolaria", out Mod _))
            {
                CreateRecipe()
                    .AddIngredient<AureusCell>(10)
                    .AddIngredient<AcidBelcher>(1)
                    .AddIngredient(ItemID.SoulofSight, 5)
                    .AddIngredient(ItemID.SoulofMight, 5)
                    .AddIngredient(ItemID.SoulofFright, 5)
                    .AddIngredient(ItemID.SoulofNight, 8)
                    .AddIngredient(ItemID.Bone, 12)
                    .AddIngredient(ItemID.CursedFlame, 8)
                    .AddTile(ModContent.TileType<SoulForgeNew>())
                    .Register();
            }
        }

    }
}
