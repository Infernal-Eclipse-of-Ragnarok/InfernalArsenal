using System;
using System.Collections.Generic;
using ThoriumMod.Items;
using ThoriumMod.Empowerments;
using ThoriumMod.Sounds;
using ThoriumMod;
using CalamityMod.Items;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.DeusFlute;
using Terraria.Localization;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class DeusFlute : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<ResourceConsumptionChance>(2);
            Empowerments.AddInfo<Defense>(3);
            Empowerments.AddInfo<ResourceRegen>(3);
        }

        public override void SetBardDefaults()
        {
            Item.width = 62;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<DeusFlutePro>();
            Item.UseSound = ThoriumSounds.Flute_Sound;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.knockBack = 1.5f;
            Item.damage = 50;
            Item.shootSpeed = 22f;

            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();

            InspirationCost = 2;

            if (!ModLoader.HasMod("Look"))
            {
                ((ModItem)this).Item.holdStyle = 3;
            }
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
            for (int i = 0; i < 4; i++)
            {
                Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(10));
                position += velocity * 1.25f;
                Projectile.NewProjectile(source, position.X, position.Y - 4, perturbedSpeed.X, perturbedSpeed.Y, type, damage, knockback, player.whoAmI, ai0: i % 2);
            }
            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DuesFluteLore", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DuesFluteLore")) { OverrideColor = Color.MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "DuesFluteLore2", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DuesFluteLore2")) { OverrideColor = Color.MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "DuesFluteLore3", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DuesFluteLore3")) { OverrideColor = Color.MediumPurple });
        }
    }
}
