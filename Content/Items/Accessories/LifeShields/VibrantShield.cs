using InfernalEclipseWeaponsDLC.Core;
using SOTS.Items.Earth;
using SOTS.Void;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Tiles;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.LifeShields
{
    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    [AutoloadEquip(EquipType.Shield)]
    public class VibrantShield : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.LifeShields;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 38;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
            Item.defense = 2;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            VoidPlayer voidPlayer = VoidPlayer.ModPlayer(player);
            ThoriumPlayer thoriumPlayer = player.GetModPlayer<ThoriumPlayer>();

            voidPlayer.voidMeterMax2 += 25;
            thoriumPlayer.MetalShieldMax += 15;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(ArsenalRecipeSystem.IronShield)
                .AddIngredient(ModContent.ItemType<VibrantBar>(), 10)
                .AddTile<ThoriumAnvil>()
                .Register();
        }
    }
}
