using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Utilities;
using InfernalEclipseWeaponsDLC.Core;
using System.Collections.Generic;
using SOTS.Void;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse
{
    [AutoloadEquip(EquipType.Head)]
    public class EclipseHelm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.buyPrice(0, 45);
            Item.rare = ItemRarityID.Lime;

            if (!ModLoader.HasMod("SOTS"))
            {
                Item.vanity = true;
            }
            else
            {
                Item.defense = 19;
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<EclipseBreastplate>() && legs.type == ModContent.ItemType<EclipseGreaves>();
        }

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.GetModPlayer<EclipsePlayer>();
            modPlayer.EclipseSet = true;

            if (!ModLoader.HasMod("SOTS")) return;

            player.setBonus = this.GetLocalization("SetBonus").Format();

            bool hasExtraSetBonus = false;
            if (ModLoader.TryGetMod("InfernalEclipseAPI", out Mod ieor))
            {
                player.buffImmune[ieor.Find<ModBuff>("BrokenOath").Type] = true;

                if (!hasExtraSetBonus) 
                {
                    player.setBonus += "\n" + this.GetLocalization("SetBonusExtra").Format();
                    hasExtraSetBonus = true;
                }
            }
            if (ModLoader.TryGetMod("WHummusMultiModBalancing", out Mod hummus))
            {
                player.buffImmune[hummus.Find<ModBuff>("BrokenOath").Type] = true;

                if (!hasExtraSetBonus)
                {
                    player.setBonus += "\n" + this.GetLocalization("SetBonusExtra").Format();
                    hasExtraSetBonus = true;
                }
            }
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.TryGetMod("SOTS", out Mod sots)) return;

            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();

            player.GetDamage((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 0.08f;
            player.GetDamage(DamageClass.Magic) += 0.08f;
            player.GetDamage(sots.Find<DamageClass>("VoidGeneric")) += 0.08f;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 2f, 50);

            player.lifeRegenTime += 10f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Mod thorium = ModLoader.GetMod("ThoriumMod");

            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddRecipeGroup(RecipeGroups.Titanium, 12);
            recipe.AddIngredient(ItemID.SoulofLight, 10);

            if (ModLoader.TryGetMod("Consolaria", out Mod consolariaMod))
            {
                recipe.AddIngredient(consolariaMod.Find<ModItem>("SoulofBlight").Type, 10);
            }
            else
            {
                if (ModIntegrationsSystem.Calamity.Loaded)
                    recipe.AddIngredient(ModIntegrationsSystem.Calamity.Mod.Find<ModItem>("AureusCell").Type, 10);

                recipe.AddIngredient(ItemID.SoulofSight, 5);
                recipe.AddIngredient(ItemID.SoulofMight, 5);
                recipe.AddIngredient(ItemID.SoulofFright, 5);
                recipe.AddIngredient(ItemID.CursedFlame, 8);
            }

            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public static class SOTSBonuses
    {
        public static void IncraseVoidGenericDamage(Player player, float ammount)
        {
            player.GetDamage<VoidGeneric>() += ammount;
        }

        public static void IncreaseVoidGenericCrit(Player player, float ammount)
        {
            player.GetCritChance<VoidGeneric>() += ammount;
        }

        public static void IncreseVoidRegenAndMaxVoid(Player player, float voidRegen = 0, int voidMax = 0)
        {
            VoidPlayer vp = VoidPlayer.ModPlayer(player);

            vp.bonusVoidGain += voidRegen;
            vp.voidMeterMax2 += voidMax;
        }

        public static void RegainVoid(Player player, float regen)
        {
            VoidPlayer vp = VoidPlayer.ModPlayer(player);
            vp.voidMeter += regen;
            VoidPlayer.VoidEffect(player, (int)regen);
        }
    }
}
