using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Utilities;
using CalamityMod.Items;
using CalamityMod.Items.Potions;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse;
using System.Collections.Generic;
using CalamityMod.Items.Materials;
using ThoriumMod.Items;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Necrosinger
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    [AutoloadEquip(EquipType.Head)]
    public class NecrosingerSkull : BardItem
    {
        public override void SetBardDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();

            if (ModLoader.HasMod("SOTS"))
                Item.defense = 20;
            else
                Item.vanity = true;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            if (!ModLoader.HasMod("SOTS")) return false;
            return body.type == ModContent.ItemType<NecrosingerRibs>() && legs.type == ModContent.ItemType<NecrosingerAnkles>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalization("SetBonus").Format();
            var modPlayer = player.GetModPlayer<NecrosingerPlayer>();
            modPlayer.NecrosingerSet = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();
            player.GetDamage((DamageClass)(object)ThoriumDamageBase<BardDamage>.Instance) += 0.2f;
            player.GetDamage(DamageClass.Summon) += 0.2f;
            SOTSBonuses.IncraseVoidGenericDamage(player, 0.2f);

            player.GetAttackSpeed((DamageClass)(object)ThoriumDamageBase<BardDamage>.Instance) += 0.1f;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.1f;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 4f, 125);

            thoriumPlayer.inspirationRegenBonus += 0.15f;

            player.maxMinions += 3;
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedBar, 12);
            recipe.AddIngredient<Lumenyl>(12);
            recipe.AddIngredient<RuinousSoul>(2);

            if (ModLoader.TryGetMod("Consolaria", out Mod consolariaMod))
            {
                recipe.AddIngredient(consolariaMod.Find<ModItem>("SoulofBlight").Type, 10);
            }
            else
            {
                recipe.AddIngredient<AureusCell>(10);
                recipe.AddIngredient(ItemID.SoulofSight, 5);
                recipe.AddIngredient(ItemID.SoulofMight, 5);
                recipe.AddIngredient(ItemID.SoulofFright, 5);
                recipe.AddIngredient(ItemID.CursedFlame, 8);
            }

            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
