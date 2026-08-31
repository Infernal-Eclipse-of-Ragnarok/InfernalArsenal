using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Utilities;
using CalamityMod.Items;
using CalamityMod.Items.Potions;
using CalamityMod.Rarities;
using System.Collections.Generic;
using CalamityMod.Items.Materials;
using ThoriumMod.Items;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Necrosinger
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    [AutoloadEquip(EquipType.Body)]
    public class NecrosingerRibs : BardItem
    {
        public override void SetBardDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();

            if (ModLoader.HasMod("SOTS"))
                Item.defense = 22;
            else
                Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();

            player.GetDamage((DamageClass)(object)ThoriumDamageBase<BardDamage>.Instance) += 0.2f;
            player.GetDamage(DamageClass.Summon) += 0.2f;

            player.lifeRegen += 5;

            thoriumPlayer.bardBuffDuration += 180;
            thoriumPlayer.bardResourceDropBoost += 0.1f;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedPlateMail);
            recipe.AddIngredient<Lumenyl>(12);
            recipe.AddIngredient<RuinousSoul>(4);

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
