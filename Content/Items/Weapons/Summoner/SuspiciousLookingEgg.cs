using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.MinionPro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner
{
    public class SuspiciousLookingEgg : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;
            Item.width = 22;
            Item.height = 28;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.value = Item.buyPrice(0, 60);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item1;

            Item.shootSpeed = 9f;
            Item.knockBack = 6.5f;
            Item.shoot = ModContent.ProjectileType<SusLookingEggPro>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.Renavo"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
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
    }
}
