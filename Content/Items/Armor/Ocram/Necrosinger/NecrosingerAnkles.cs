using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
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
    [AutoloadEquip(EquipType.Legs)]
    public class NecrosingerAnkles : BardItem
    {
        public override void SetBardDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();

            if (ModLoader.HasMod("SOTS"))
            {
                Item.defense = 20;
            }
            else
                Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            player.GetDamage(DamageClass.Summon) += 0.2f;
            player.whipRangeMultiplier += 0.2f;
            player.moveSpeed += 0.2f;

            player.GetCritChance((DamageClass)(object)ThoriumDamageBase<BardDamage>.Instance) += 20f;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 3f, 100);

            player.maxTurrets += 3;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedGreaves);
            recipe.AddIngredient<Lumenyl>(12);
            recipe.AddIngredient<RuinousSoul>(3);

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
