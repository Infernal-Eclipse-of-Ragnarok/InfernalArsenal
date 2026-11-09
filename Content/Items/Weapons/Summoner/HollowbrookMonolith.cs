using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.SentryPro;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Items.Placeable;
using ThoriumMod.Items.SummonItems;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class HollowbrookMonolith : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 36;
            Item.useTime = Item.useAnimation = 20;
            Item.mana = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(0, 60);

            Item.shoot = ModContent.ProjectileType<HollowbrookMonolithPro>();
            Item.autoReuse = true;

            Item.UseSound = SoundID.Item8;
        }

        public override bool CanUseItem(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.FindSentryRestingSpot(type, out int x, out int y, out int height);

            position = new Vector2(x, y);
            position.Y -= type >= 0 && type < ProjectileLoader.ProjectileCount
                ? (int)Math.Ceiling(ContentSamples.ProjectilesByType[type].height / 2.0)
                : height;

            InfernalWeaponsPlayer modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            player.UpdateMaxTurrets();

            if (modPlayer.hollowbrookTier <= 0 || player.ownedProjectileCounts[type] <= 0)
            {
                modPlayer.hollowbrookLocation = position;

                if (!TryFreeSentrySlot(player))
                    return false;

                Projectile sentry = Projectile.NewProjectileDirect(
                    source, position, Vector2.UnitY * -8f,
                    type, damage, knockback, player.whoAmI, type
                );

                if (sentry == null || !sentry.active)
                    return false;

                sentry.originalDamage = Item.damage;
                modPlayer.hollowbrookTier = 1;
            }
            else if (modPlayer.hollowbrookTier < 3)
            {
                modPlayer.hollowbrookLocation = position;

                if (!TryFreeSentrySlot(player))
                    return false;

                Projectile upgrade = Projectile.NewProjectileDirect(
                    source, position, Vector2.Zero,
                    ModContent.ProjectileType<HollowbrookMonolithUpgradePro>(),
                    damage, knockback, player.whoAmI, type
                );

                if (upgrade == null || !upgrade.active)
                    return false;

                upgrade.originalDamage = Item.damage;
                modPlayer.hollowbrookTier++;
            }
            else
            {
                modPlayer.hollowbrookLocation = position;
            }

            player.UpdateMaxTurrets();

            return false;
        }

        //To allow replacing existing sentrys with upgrades without spawning a new projectile
        private int GetOccupiedSentrySlots(Player player)
        {
            int occupied = 0;

            foreach (Projectile projectile in Main.projectile)
            {
                if (projectile.active &&
                    projectile.owner == player.whoAmI &&
                    projectile.sentry)
                {
                    occupied++;
                }
            }

            return occupied;
        }

        private bool TryFreeSentrySlot(Player player)
        {
            player.UpdateMaxTurrets();

            int occupied = GetOccupiedSentrySlots(player);

            if (occupied < player.maxTurrets)
                return true;

            if (!RemoveOldestSentry(player))
                return false;

            player.UpdateMaxTurrets();

            occupied = GetOccupiedSentrySlots(player);

            return occupied < player.maxTurrets;
        }

        private bool RemoveOldestSentry(Player player)
        {
            Projectile oldestSentry = null;
            int oldestTimeLeft = int.MaxValue;

            int hollowbrookType = ModContent.ProjectileType<HollowbrookMonolithPro>();
            int upgradeType = ModContent.ProjectileType<HollowbrookMonolithUpgradePro>();

            foreach (Projectile projectile in Main.projectile)
            {
                if (!projectile.active ||
                    projectile.owner != player.whoAmI ||
                    !projectile.sentry)
                    continue;

                if (projectile.type == hollowbrookType ||
                    projectile.type == upgradeType)
                    continue;

                if (projectile.timeLeft < oldestTimeLeft)
                {
                    oldestTimeLeft = projectile.timeLeft;
                    oldestSentry = projectile;
                }
            }

            if (oldestSentry == null)
                return false;

            oldestSentry.Kill();

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.RPG"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
                line5.OverrideColor = new Color(196, 35, 44);
                tooltips.Add(line5);
            }
            else
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor"));
                line5.OverrideColor = new Color(196, 35, 44);
                tooltips.Add(line5);
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient<PiousBanner>();
            recipe.AddIngredient<BountyBanner>();
            recipe.AddIngredient<BattleBanner>();
            recipe.AddIngredient<CombatBanner>();
            recipe.AddIngredient<MoraleBanner>();
            recipe.AddIngredient<PowerBanner>();
            recipe.AddIngredient<PrecisionBanner>();
            recipe.AddIngredient(ItemID.StoneBlock, 100);

            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                recipe.AddIngredient(cal.Find<ModItem>("ScoriaBar").Type, 5);
            }
            else
            {
                recipe.AddIngredient(ItemID.LihzahrdBrick, 25);
            }

            recipe.AddIngredient(ItemID.HeartStatue);
            recipe.AddIngredient(ItemID.StarStatue);
            recipe.AddIngredient<MusicNoteStatue>();
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
         }
    }
}
