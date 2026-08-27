using System;
using System.Collections.Generic;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell;
using InfernalEclipseWeaponsDLC.Content.Projectiles.SpearTipPro;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Common
{
    public class WeaponsGlobalItem : GlobalItem
    {
        public bool verveineItem = false;

        public override bool InstancePerEntity => true;

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                if (item.type == calamityMod.Find<ModItem>("WarbanneroftheRighteous").Type)
                {
                    player.GetModPlayer<InfernalWeaponsPlayer>().hasWarbanner = true;
                }
            }    
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity;
            InfernalWeaponsPlayer weaponPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            if (ItemID.Sets.Spears[item.type] && (weaponPlayer.spearSearing || weaponPlayer.spearArctic))
            {
                if (weaponPlayer.spearSearing)
                {
                    Projectile.NewProjectile(source, position, direction * 2f, ModContent.ProjectileType<HydrogenSulfideProj>(), (int)(damage * 1.5), knockback, player.whoAmI, 0.0f, 0.0f, 0.0f);
                }

                if (weaponPlayer.spearArctic)
                {
                    Projectile.NewProjectile(source, position, direction, ModContent.ProjectileType<CryonicSpearTip>(), (int)(damage * 1.15), knockback, player.whoAmI, 0.0f, 0.0f, 0.0f);
                }
            }
            return true;
        }

        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.GetModPlayer<SuperCellPlayer>().hasSuperCellGuardEquipped && item.CountsAsClass<RangedDamageClass>())
                velocity *= 1.3f;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (verveineItem)
            {
                int maxTooltipIndex = -1;
                int maxNumber = -1;

                for (int i = 0; i < tooltips.Count; i++)
                {
                    if (tooltips[i].Mod == "Terraria" && tooltips[i].Name.StartsWith("Tooltip"))
                    {
                        if (int.TryParse(tooltips[i].Name.Substring(7), out int num) && num > maxNumber)
                        {
                            maxNumber = num;
                            maxTooltipIndex = i;
                        }
                    }
                }

                if (maxTooltipIndex != -1)
                {
                    int insertIndex = maxTooltipIndex + 1;
                    TooltipLine verveineLine = new(Mod, "VerveineItem", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.VerveineItem"));
                    verveineLine.OverrideColor = Color.Lerp(Color.MediumPurple, Color.MediumOrchid, (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 2.0) * 0.5 + 0.5));

                    tooltips.Insert(insertIndex, verveineLine);
                }
            }
        }
    }
}
