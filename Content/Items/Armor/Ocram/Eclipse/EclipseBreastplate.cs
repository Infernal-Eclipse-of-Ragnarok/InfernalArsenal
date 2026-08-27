using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Utilities;
using InfernalEclipseWeaponsDLC.Core;
using System.Collections.Generic;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse
{
    [AutoloadEquip(EquipType.Body)]
    public class EclipseBreastplate : ModItem
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
                Item.defense = 25;
            }
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.TryGetMod("SOTS", out Mod sots)) return;

            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();

            player.GetDamage((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 0.12f;
            player.GetDamage(DamageClass.Magic) += 0.12f;

            player.GetCritChance((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 8f;
            player.GetCritChance(DamageClass.Magic) += 8f;

            player.GetAttackSpeed((DamageClass)(object)ThoriumDamageBase<HealerDamage>.Instance) += 0.05f;
            player.GetAttackSpeed((DamageClass)(object)ThoriumDamageBase<HealerTool>.Instance) += 0.05f;
            player.GetAttackSpeed(DamageClass.Magic) += 0.05f;

            thoriumPlayer.thoriumEndurance += 0.15f;
            thoriumPlayer.healBonus += 3;
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
            recipe.AddRecipeGroup(RecipeGroups.Titanium, 12);
            recipe.AddIngredient(ItemID.SoulofLight, 15);

            if (ModLoader.TryGetMod("Consolaria", out Mod consolariaMod))
            {
                recipe.AddIngredient(consolariaMod.Find<ModItem>("SoulofBlight").Type, 15);
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
