using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Utilities;
using InfernalEclipseWeaponsDLC.Core;
using System.Collections.Generic;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse
{
    [AutoloadEquip(EquipType.Legs)]
    public class EclipseGreaves : ModItem
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

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.TryGetMod("SOTS", out Mod sots)) return;

            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();

            player.GetDamage((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 0.08f;
            player.GetDamage(DamageClass.Magic) += 0.08f;

            player.GetCritChance((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 4f;
            player.GetCritChance(DamageClass.Magic) += 4f;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 1f, 50);

            player.manaCost -= 0.15f;
            player.moveSpeed += 0.25f;
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
}
