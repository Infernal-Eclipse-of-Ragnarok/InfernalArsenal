using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using SOTS;
using SOTS.Items.Earth;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Other
{
    [JITWhenModsEnabled("CalamityMod", "SOTS")]
    [ExtendsFromMod("CalamityMod", "SOTS")]
    [AutoloadEquip(EquipType.Back, EquipType.Shield)]
    public class MarniteRepulsionShieldMK2 : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.width = 34;
            Item.height = 40;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(0, 5);
            Item.accessory = true;
            Item.defense = 4;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            MarniteRepulsionShieldPlayer modPlayer = player.GetModPlayer<MarniteRepulsionShieldPlayer>();
            modPlayer.shieldEquipped = true;

            if (player.whoAmI == Main.myPlayer)
            {
                var source = player.GetSource_Accessory(Item);
                if (player.ownedProjectileCounts[ModContent.ProjectileType<MarniteRepulsionHitbox>()] < 1)
                {
                    var hitbox = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<MarniteRepulsionHitbox>(), 5, 12f, Main.myPlayer);
                    hitbox.originalDamage = 5;
                }
            }

            SOTSPlayer.ModPlayer(player).PushBack = true;

            player.endurance += 0.08f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient<MarniteRepulsionShield>()
                .AddIngredient<MarbleDefender>()
                .AddIngredient<GraniteProtector>()
                .AddIngredient<AerialiteBar>(5);

            if (ModLoader.TryGetMod("RagnarokMod", out Mod ragnarok))
            {
                recipe.AddIngredient(ragnarok.Find<ModItem>("EmpoweredGranite"), 3);
                recipe.AddIngredient(ragnarok.Find<ModItem>("EnchantedMarble"), 3);
            }

            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
