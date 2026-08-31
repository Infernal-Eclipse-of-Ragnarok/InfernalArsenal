using InfernalEclipseWeaponsDLC.Core.NewFolder;
using InfernalEclipseWeaponsDLC.Core.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.BossMini;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Summoner
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class AllSeersGlass : ModItem
    {
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
            player.GetModPlayer<InfernalWeaponsPlayer>().minionCrits = true;
            player.GetModPlayer<ThoriumAccessoryKeyEffects>().canFreezeCamera = true;

            if (!hideVisual)
            {
                player.GetThoriumPlayer().accScryingGlass = true;
            }
        }
        public override void AddRecipes()
        {
        CreateRecipe()
            .AddIngredient<ScryingGlass>(1)
            .AddIngredient(ItemID.RifleScope, 1)
            .AddIngredient(ItemID.FragmentSolar, 8)
            .AddIngredient(ItemID.FragmentStardust, 4)
            .AddTile(TileID.LunarCraftingStation)
               .Register();
        }
    }
}