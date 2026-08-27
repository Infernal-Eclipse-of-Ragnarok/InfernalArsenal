using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod.Utilities;
using CalamityMod.Items;
using CalamityMod.Items.Potions;
using CalamityMod.CalPlayer;
using CalamityMod;
using System;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.Vanity;
using System.Collections.Generic;
using System.Text;
using Terraria.DataStructures;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Accessories.Wings;
using Terraria.Localization;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell
{
    [AutoloadEquip(EquipType.Head)]
    public class SuperCellCirclet : ModItem
    {
        private bool displayWingsTooltips;

        public static int wingsSlot = -1;
        public static Lazy<Asset<Texture2D>> wingAsset;

        public override void Load()
        {
            wingsSlot = EquipLoader.AddEquipTexture(Mod, Texture + "_WingsFake", EquipType.Wings, null, Name + "_Wings", new SuperCellWings());
            if (Main.dedServ)
                return;
            wingAsset = new Lazy<Asset<Texture2D>>(() => ModContent.Request<Texture2D>(Texture + "_Wings", AssetRequestMode.AsyncLoad));
        }

        public override void SetStaticDefaults()
        {
            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawFullHair[equipSlot] = true;   // Draw all hair
            ArmorIDs.Head.Sets.DrawHatHair[equipSlot] = false;   // Don’t limit hair shape
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;

            if (ModLoader.HasMod("SOTS"))
                Item.defense = 10;
            else
                Item.vanity = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            if (body.type == ModContent.ItemType<SuperCellGuard>() && legs.type == ModContent.ItemType<SuperCellSabatons>() && ModLoader.HasMod("SOTS"))
            {
                displayWingsTooltips = true;
                return true;
            }
            displayWingsTooltips = false;
            return false;
        }

        public override void UpdateArmorSet(Player player)
        {
            CalamityPlayer calamityPlayer = player.Calamity();

            player.setBonus = this.GetLocalizedValue("SetBonus");

            player.shroomiteStealth = true;
            player.Calamity().wearingRogueArmor = true;
            calamityPlayer.rogueStealthMax += 1.15f;

            if (player.wings <= 0 || player.wingTimeMax < GarudaWings.supercellWingTime)
            {
                player.wings = wingsSlot;
                player.wingsLogic = ArmorIDs.Wing.SolarWings;
                player.wingTimeMax = GarudaWings.supercellWingTime;
                player.noFallDmg = true;
            }

            if (displayWingsTooltips)
            {
                player.setBonus += WingStatsTooltip(ArmorIDs.Wing.Sets.Stats[ArmorIDs.Wing.SolarWings], 0.85f, 0.15f, 1f, 3f, 0.135f);
            }
        }

        private static string WingStatsTooltip(WingStats stats, float fall, float rise, float rMax, float tMax, float asc, string extraKey = null)
        {
            int time = GarudaWings.supercellWingTime;
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

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadow = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (!ModLoader.HasMod("SOTS")) return;

            player.GetDamage(DamageClass.Throwing) += 0.25f;
            player.GetDamage(DamageClass.Ranged) += 0.25f;

            SOTSBonuses.IncraseVoidGenericDamage(player, 0.15f);
            player.GetThoriumPlayer().techPointsMax += 2;

            SOTSBonuses.IncreseVoidRegenAndMaxVoid(player, 4f, 75);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ModLoader.HasMod("SOTS")) return;

            tooltips.RemoveAll(t => t.Name.Contains("Tooltip"));
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HallowedBar, 12);

            if (ModLoader.TryGetMod("SOTS", out Mod sots))
                recipe.AddIngredient(sots.Find<ModItem>("SanguiteBar").Type, 15);
            else
                recipe.AddIngredient(ItemID.LunarBar, 15);
            recipe.AddIngredient<EffulgentFeather>(5);

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

    public class SuperCellWings : EquipTexture
    {
        public override bool WingUpdate(Player player, bool inUse)
        {
            const int frames = 4;

            // GLIDE STATE: hold last frame (frame 3)
            if (player.wingsLogic > 0 && player.velocity.Y > 0f && !player.controlJump)
            {
                player.wingFrame = 2;           // hold frame 3
                player.wingFrameCounter = 0;    // stop animation
                return true;
            }

            // Normal animation logic
            int frameTime;

            if (player.velocity.Y < 0f || player.jump > 0) // flapping upwards
                frameTime = 4;
            else if (player.velocity.Y != 0f) // falling normally
                frameTime = 6;
            else // standing
            {
                frameTime = 0;
                player.wingFrame = 0;
                player.wingFrameCounter = 0;
                return true;
            }

            player.wingFrameCounter++;
            if (player.wingFrameCounter >= frameTime)
            {
                player.wingFrameCounter = 0;
                player.wingFrame++;

                if (player.wingFrame >= frames)
                    player.wingFrame = 0;
            }

            return true;
        }
    }
}
