using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.Items;
using CalamityMod.Items.Potions;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.Vanity;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse;
using CalamityMod.Items.Materials;
using System.Collections.Generic;
using CalamityMod.CalPlayer;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    [AutoloadEquip(EquipType.Body)]
    public class SuperCellGuard : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<GarudaWings>();
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;

            if (ModLoader.HasMod("SOTS"))
                Item.defense = 14;
            else
                Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            player.GetModPlayer<CalamityPlayer>().rogueVelocity += 0.3f;

            player.GetModPlayer<SuperCellPlayer>().hasSuperCellGuardEquipped = true;

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

            recipe.AddIngredient(ItemID.HallowedPlateMail);
            if (ModLoader.TryGetMod("SOTS", out Mod sots))
                recipe.AddIngredient(sots.Find<ModItem>("SanguiteBar").Type, 20);
            else
                recipe.AddIngredient(ItemID.LunarBar, 20);
            recipe.AddIngredient<EffulgentFeather>(10);

            if (ModLoader.TryGetMod("Consolaria", out Mod consolariaMod))
            {
                recipe.AddIngredient(consolariaMod.Find<ModItem>("SoulofBlight").Type, 15);
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
