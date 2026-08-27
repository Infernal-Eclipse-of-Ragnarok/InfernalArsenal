using System;
using System.Collections.Generic;
using System.Text;
using CalamityMod.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell;
using CalamityMod;
using CalamityMod.Items.Accessories.Wings;
using System.Linq;
using Terraria.Localization;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Vanity
{
    [AutoloadEquip(EquipType.Wings)]
    public class GarudaWings : ModItem
    {
        public const int supercellWingTime = 180;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SuperCellGuard>();
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 28;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (player.wings <= 0 || player.wingTimeMax < supercellWingTime)
            {
                player.wings = SuperCellCirclet.wingsSlot;
                player.wingsLogic = ArmorIDs.Wing.SolarWings;
                player.wingTimeMax = supercellWingTime;
                player.noFallDmg = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            void ApplyTooltipEdits(IList<TooltipLine> lines, Func<Item, TooltipLine, bool> predicate, Action<TooltipLine> action)
            {
                foreach (TooltipLine line in lines)
                    if (predicate.Invoke(Item, line))
                        action.Invoke(line);
            }
            Func<Item, TooltipLine, bool> LineNum(int n) => (Item i, TooltipLine l) => l.Mod == "Terraria" && l.Name == $"Tooltip{n}";
            void EditTooltipByNum(int lineNum, Action<TooltipLine> action) => ApplyTooltipEdits(tooltips, LineNum(lineNum), action);
            void AddWingStats(int slot, float fall, float rise, float rMax, float tMax, float asc, string extraKey = null) => EditTooltipByNum(0, (line) => line.Text += WingStatsTooltip(ArmorIDs.Wing.Sets.Stats[slot], fall, rise, rMax, tMax, asc, extraKey));

            AddWingStats(ArmorIDs.Wing.SolarWings, 00.85f, 0.15f, 1f, 3f, 0.135f);

            string WingStatsTooltip(WingStats stats, float fall, float rise, float rMax, float tMax, float asc, string extraKey = null)
            {
                int time = supercellWingTime;
                float run = stats.AccRunSpeedOverride;
                float rAcc = stats.AccRunAccelerationMult * 0.08f;
                bool hover = stats.HasDownHoverStats;
                float hSpeed = stats.DownHoverSpeedOverride;
                float hAcc = stats.DownHoverAccelerationMult * 0.08f;
                float baseJumpSpeed = (CalamityServerConfig.Instance.FasterJumpSpeed ? 5.71f : 5.01f) + 1f;
                StringBuilder sb = new StringBuilder(512);
                sb.Append('\n');
                if (Main.keyState.PressingShift())
                {
                    sb.Append(CalamityUtils.GetText($"Common.WingStatsFull").Format(time.FramesToSeconds(),
                    BaseWings.HorizontalSpeedText(run), run.ToMph(),
                    BaseWings.VerticalSpeedText(1.35f), (1.35f * baseJumpSpeed).ToMph(),
                    BaseWings.HorizontalAccelerationText(stats.AccRunAccelerationMult), rAcc.ToMphps(),
                    BaseWings.VerticalAccelerationText(0.195f), 0.195f.ToMphps(),
                    (0.1f + 0.15f).ToMphps(), (1 * baseJumpSpeed).ToMph(),
                    (0.195f + 0.85f).ToMphps()));
                    if (hover)
                    {
                        sb.Append('\n');
                        sb.Append(Language.GetText($"Common.WingStatsHover").Format(hSpeed.ToMph(), hAcc.ToMphps()));
                    }
                }
                else
                {
                    sb.Append(CalamityUtils.GetText($"Common.WingStats").Format(time.FramesToSeconds(), BaseWings.HorizontalSpeedText(run), BaseWings.VerticalSpeedText(1.35f),
                    BaseWings.HorizontalAccelerationText(stats.AccRunAccelerationMult), BaseWings.VerticalAccelerationText(0.195f)));
                    sb.Append('\n');
                    sb.Append($"[c/B8B8B8:{Language.GetTextValue("Mods.CalamityMod.UI.HoldShiftTooltipExtensionIndicator")}]");
                }

                return sb.ToString();
            }
        }
    }
}
