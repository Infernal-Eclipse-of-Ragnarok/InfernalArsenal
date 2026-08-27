using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using CalamityMod.CalPlayer;
using CalamityMod;
using CalamityMod.Buffs.StatBuffs;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Donor
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    [AutoloadEquip(EquipType.Balloon)]
    public class ImagiknightHeraldry : ModItem
    {
        public const float MaxBonus = 0.2f;
        public const float MaxDistance = 700f;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(Affliction.RegenBoost.ToRegenPerSecond(), Affliction.MaxLifeBoostPercent, Affliction.DamageReductionBoost.ToPercent(), Affliction.DefenseBoost, Affliction.DamageBoost.ToPercent());

        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 5));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 78;
            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModLoader.TryGetMod("CatalystMod", out Mod catalyst) ? catalyst.Find<ModRarity>("SuperbossExpertRarity").Type : ModContent.RarityType<HotPink>();
            Item.accessory = true;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            InfernalWeaponsPlayer weaponsPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            modPlayer.affliction = true;
            if (player.whoAmI != Main.myPlayer && player.miscCounter % 10 == 0)
            {
                if (Main.LocalPlayer.team == player.team && player.team != 0)
                {
                    Main.LocalPlayer.AddBuff(ModContent.BuffType<Afflicted>(), 20, true);
                }
            }

            weaponsPlayer.imagiknightHeraldry = true;
            weaponsPlayer.hideHeraldryVisual = hideVisual;
            modPlayer.warbannerGlow = !hideVisual;

            if (player.ownedProjectileCounts[ModContent.ProjectileType<WarbannerLight>()] < 1 && !hideVisual && !player.dead)
            {
                Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<WarbannerLight>(), 0, 0f, player.whoAmI);
            }
        }

        public static float CalculateBonus(Player player)
        {
            float bonus;

            Player closestPlayer = ClosestPlayerAt(player.Center, MaxDistance, player); // extra range is to account for bonus range from massive targets
            if (closestPlayer != null)
            {
                float generousHitboxWidth = Math.Max(closestPlayer.Hitbox.Width / 2f, closestPlayer.Hitbox.Height / 2f) + 100; // Adds some room so max bonus isnt when you're ON the hitbox
                bonus = Utils.Remap(Utils.Distance(player.Center, closestPlayer.Center), MaxDistance + generousHitboxWidth, generousHitboxWidth, 0, MaxBonus, true);
            }
            else
                bonus = 0;

            return bonus;
        }

        public static Player ClosestPlayerAt(Vector2 origin, float maxDistanceToCheck, Player owner)
        {
            Player result = null;
            float num = maxDistanceToCheck;
            foreach (var player in Main.ActivePlayers)
            {
                if (player == owner || player.dead) // don't count the player wearing the accessory or any dead players
                    continue;

                if (owner.team == 0 || player.team != owner.team) // only count players on their team
                    continue;

                float num3 = player.width / 2 + player.height / 2;
                bool flag3 = true;
                if (num3 < num)
                {
                    flag3 = Collision.CanHit(origin, 1, 1, player.Center, 1, 1);
                }

                if (Vector2.Distance(origin, player.Center) < num && flag3)
                {
                    num = Vector2.Distance(origin, player.Center);
                    result = player;
                }
            }

            return result;
        }

        public static float CalculateBonusFromDistance(Player source, Player target)
        {
            float generousHitboxWidth = Math.Max(target.Hitbox.Width / 2f, target.Hitbox.Height / 2f) + 100f;

            return Utils.Remap(Vector2.Distance(source.Center, target.Center), MaxDistance + generousHitboxWidth, generousHitboxWidth, 0f, MaxBonus, true);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            CalamityUtils.DrawInventoryCustomScale(
                spriteBatch,
                texture: TextureAssets.Item[Type].Value,
                position,
                frame,
                drawColor,
                itemColor,
                origin,
                scale,
                wantedScale: 0.6f,
                drawOffset: new(0f, -2f)
            );
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.paradox"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
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
            CreateRecipe()
                .AddIngredient<WarbanneroftheRighteous>()
                .AddIngredient<Affliction>()
                .AddIngredient<ShadowspecBar>(5)
                .AddTile<DraedonsForge>()
                .Register();
        }
    }
}
