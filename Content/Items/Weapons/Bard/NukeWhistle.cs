using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
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

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class NukeWhistle : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<Damage>(200);
            Empowerments.AddInfo<CriticalStrikeChance>(25);
        }

        public override void SetBardDefaults()
        {
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.width = 20;
            Item.height = 38;
            Item.shootSpeed = 15;
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
                if (calamity.TryFind("ShadowspecBar", out ModItem bar))
                {
                    recipe.AddIngredient(bar, 5);
                }
            }
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            base.AddRecipes();
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            InfernalWeaponsPlayer modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();
            modPlayer.missileIndex--;
            if (modPlayer.missileIndex == 0)
            {
                modPlayer.missileIndex = 10;
                int sign = Main.rand.Next(0, 2) * 2 - 1;
                int xOffset = (int)((Main.MouseWorld.X - player.Center.X) / 3) +
                              (sign == 0 ? Main.rand.Next(-8, 4) * 65 : Main.rand.Next(-4, 8) * 65);
                Projectile.NewProjectile(source, player.Center + new Vector2(xOffset, -Main.screenHeight / 2),
                    new Vector2((Main.MouseWorld.X -
                                 (player.Center.X + xOffset)) / 37, 7), ModContent.ProjectileType<NukePro>(),
                    (int)((double)damage * 2), knockback, 0, Item.useTime);
            }
            else
            {
                int sign = Main.rand.Next(0, 2) * 2 - 1;
                int xOffset = (int)((Main.MouseWorld.X - player.Center.X) / 3) +
                              (sign == 0 ? Main.rand.Next(-8, 4) * 65 : Main.rand.Next(-4, 8) * 65);
                Projectile.NewProjectile(source, player.Center + new Vector2(xOffset, -Main.screenHeight / 2),
                    new Vector2((Main.MouseWorld.X -
                                 (player.Center.X + xOffset)) / 37, 7), type, damage, knockback, 0, Item.useTime);
            }

            return false;
        }
    }
}
