using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoriumMod.Items;
using ThoriumMod.Empowerments;
using ThoriumMod.Sounds;
using ThoriumMod;
using CalamityMod.Items;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ID;
using ThoriumMod.Items.BardItems;
using CalamityMod.Items.Placeables;
using static Terraria.Player;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class BrimstoneHarp : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<DamageReduction>(3);
            Empowerments.AddInfo<LifeRegeneration>(3);
            Empowerments.AddInfo<Defense>(3);
        }

        public override void SetBardDefaults()
        {
            Item.width = 38;
            Item.height = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<BrimstoneHarpPro>();
            Item.UseSound = SoundID.Item26;

            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.knockBack = 1.5f;
            Item.damage = 55;
            Item.shootSpeed = 14f;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            InspirationCost = 2;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(6, -7.5f);
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
            for (int i = 0; i < 3; i++)
            {
                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(-10 + i * 10) + MathHelper.ToRadians(Main.rand.NextFloat(-3f, 3f)));
                Projectile.NewProjectile(source, position.X, position.Y, perturbedSpeed.X, perturbedSpeed.Y, type, damage, knockback, player.whoAmI);
            }
            return false;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void AddRecipes()
        {
        }
    }
}
