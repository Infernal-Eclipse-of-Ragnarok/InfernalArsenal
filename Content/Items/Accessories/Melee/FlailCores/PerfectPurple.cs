using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using CalamityMod.Items;
using CalamityMod.Rarities;
using System.Collections.Generic;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.FlailCores
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class PerfectPurple : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.FlailCores;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<InfernalWeaponsPlayer>().perfectPurple = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Lore lines
            tooltips.Add(new TooltipLine(Mod, "Lore", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.PerfectPurpleLore")) { OverrideColor = Color.MediumPurple });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<DoubleFlail>())
                .AddIngredient(ModContent.ItemType<CosmiliteBar>(), 10)
                .AddTile(ModContent.TileType<CosmicAnvil>())
                .Register();
        }
    }
}