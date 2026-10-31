using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Other
{
    [AutoloadEquip(EquipType.Shoes)]
    public class MarniteHoverboots : ModItem
    {
        public static readonly int MovementSpeed = 5;

        public override LocalizedText Tooltip
        {
            get
            {
                return base.Tooltip.WithFormatArgs(MovementSpeed);
            }
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 40;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 5);
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.moveSpeed += MovementSpeed / 100f;
            player.accRunSpeed = 6f;
            player.GetModPlayer<InfernalWeaponsPlayer>().marniteHoverboots = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ItemID.HermesBoots);
            
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
            {
                recipe.AddIngredient(thorium.Find<ModItem>("AirWalkers"));
            }

            recipe.AddIngredient(ItemID.Granite, 25);
            recipe.AddIngredient(ItemID.Marble, 25);

            if (thorium != null)
            {
                recipe.AddIngredient(thorium.Find<ModItem>("GraniteEnergyCore"), 10);
                recipe.AddIngredient(thorium.Find<ModItem>("BronzeAlloyFragments"), 10);

                if (ModLoader.TryGetMod("RagnarokMod", out Mod ragnarok))
                {
                    recipe.AddIngredient(ragnarok.Find<ModItem>("EmpoweredGranite"), 3);
                    recipe.AddIngredient(ragnarok.Find<ModItem>("EnchantedMarble"), 3);
                }
            }
            else
            {
                recipe.AddIngredient(ItemID.Wire, 25);
            }

            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
