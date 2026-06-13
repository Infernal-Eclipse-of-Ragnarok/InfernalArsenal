using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes;
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
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.BossThePrimordials.Dream;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Tiles;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes
{
    public class StormCarver : ScytheItem
    {
        public override void SetStaticDefaults()
        {
            SetStaticDefaultsToScythe();
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
        }

        public override void SetDefaults()
        {
            SetDefaultsToScythe();

            Item.damage = 16;
            scytheSoulCharge = 2;
            Item.width = 70;
            Item.height = 82;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<StormCarverPro>();
            Item.autoReuse = true;
            Item.holdStyle = 6;

            Item.noUseGraphic = false;
            Item.scale = 1.0f;

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = 100;
        }

        private int swingDirection;

        public override void HoldStyle(Player player, Rectangle itemFrame)
        {
            player.itemLocation += new Vector2(-10f, 12f) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle itemFrame)
        {
            player.itemLocation = Vector2.Zero;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (swingDirection != -1 && swingDirection != 1)
            {
                swingDirection = 1;
            }
            if (Main.myPlayer == player.whoAmI)
            {
                float attackTime = player.itemAnimationMax <= 0 ? Item.useAnimation : player.itemAnimationMax > player.itemTimeMax ? player.itemTimeMax : player.itemAnimationMax;
                int z = Projectile.NewProjectile((IEntitySource)(object)source, position, Vector2.Normalize(Main.MouseWorld - player.MountedCenter), type, damage, knockback, player.whoAmI, attackTime, attackTime, player.GetAdjustedItemScale(Item));
                Main.projectile[z].direction = swingDirection;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, z, 0f, 0f, 0f, 0, 0, 0);
            }
            swingDirection = -swingDirection;
            return false;
        }
        public override bool MeleePrefix()
        {
            return true;
        }
    }
}
