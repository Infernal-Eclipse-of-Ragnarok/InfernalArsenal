using CalamityMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Utilities;
using CalamityMod.Items.Materials;
using InfernalEclipseWeaponsDLC.Core.NewFolder;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.LifeShields
{
    [AutoloadEquip(EquipType.Shield)]
    public class PerennialShield : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.LifeShields;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 58;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
            Item.defense = 8;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();
            thoriumPlayer.MetalShieldMax += 50;
            player.GetModPlayer<InfernalWeaponsPlayer>().perennialShield = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PerennialBar>(10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
