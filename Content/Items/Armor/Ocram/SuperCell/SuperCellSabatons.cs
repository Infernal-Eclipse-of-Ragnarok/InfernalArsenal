using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items;
using CalamityMod.Items.Potions;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse;
using CalamityMod.Items.Materials;
using System.Collections.Generic;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    [AutoloadEquip(EquipType.Legs)]
    public class SuperCellSabatons : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;

            if (ModLoader.HasMod("SOTS"))
                Item.defense = 12;
            else
                Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            player.GetCritChance(DamageClass.Throwing) += 25f;
            player.GetCritChance(DamageClass.Ranged) += 25f;

            SOTSBonuses.IncreaseVoidGenericCrit(player, 15f);
            player.moveSpeed += 0.3f;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 3f, 75);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedGreaves);
            if (ModLoader.TryGetMod("SOTS", out Mod sots))
                recipe.AddIngredient(sots.Find<ModItem>("SanguiteBar").Type, 10);
            else
                recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddIngredient<EffulgentFeather>(7);

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
