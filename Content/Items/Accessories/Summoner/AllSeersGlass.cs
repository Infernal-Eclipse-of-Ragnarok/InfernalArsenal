using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Core.Players;
using SOTS;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Summoner
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class AllSeersGlass : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            //return true;
            return WeaponConfig.Instance.UnfinishedContent;
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 12));
        }

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 42;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.buyPrice(0, 45);
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.maxTurrets += 2;
            player.GetCritChance(DamageClass.Generic) += 10f;

            if (ModLoader.HasMod("SOTS"))
                SetSOTSCritBonusDamage.SetUp(player);
            else
                player.GetThoriumPlayer().bonusCritDamage += 0.15f;

            player.GetModPlayer<InfernalWeaponsPlayer>().minionCrits = true;
            player.GetModPlayer<ThoriumAccessoryKeyEffects>().canFreezeCamera = true;

            if (!hideVisual)
            {
                player.GetThoriumPlayer().accScryingGlass = true;
                player.GetThoriumPlayer().accScryingGlassActive = true;
            }
        }
    }

    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public static class SetSOTSCritBonusDamage
    {
        public static void SetUp(Player player)
        {
            SOTSPlayer sotsPlayer = SOTSPlayer.ModPlayer(player);

            sotsPlayer.CritBonusDamage += 15;
        }
    }
}
