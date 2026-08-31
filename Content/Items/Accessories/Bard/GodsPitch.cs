using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Bard
{
    [AutoloadEquip(EquipType.Face)]
    public class GodsPitch : BardItem
    {
        public Mod calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetBardDefaults()
        {
            accessoryType = AccessoryType.SoundDevice;

            Item.width = 36;
            Item.height = 38;
            Item.accessory = true;
            Item.rare = (calamity != null) ? calamity.Find<ModRarity>("BurnishedAuric").Type : ItemRarityID.Purple;
            Item.value = Item.buyPrice(2, 40);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();

            player.GetDamage(ThoriumDamageBase<BardDamage>.Instance) += 0.15f;
            //player.GetAttackSpeed(ThoriumDamageBase<BardDamage>.Instance) += 0.1f;

            thoriumPlayer.bardResourceMax2 += 5;
            thoriumPlayer.bardBuffDuration += 60 * 3;
            thoriumPlayer.bardRangeBoost += 250;

            if (!hideVisual)
                SpawnNoteEffect(player);

            player.GetThoriumPlayer().musicPlayerShared[BardInstrumentType.Brass] = true;
            player.GetThoriumPlayer().musicPlayerShared[BardInstrumentType.String] = true;
            player.GetThoriumPlayer().musicPlayerShared[BardInstrumentType.Wind] = true;
            player.GetThoriumPlayer().musicPlayerShared[BardInstrumentType.Percussion] = true;

            player.GetThoriumPlayer().musicPlayerLevels[BardInstrumentType.Brass] = 2;
            player.GetThoriumPlayer().musicPlayerLevels[BardInstrumentType.String] = 2;
            player.GetThoriumPlayer().musicPlayerLevels[BardInstrumentType.Wind] = 2;
            player.GetThoriumPlayer().musicPlayerLevels[BardInstrumentType.Percussion] = 2;

            player.GetModPlayer<InfernalWeaponsPlayer>().godsPitch = true;
        }
        protected void SpawnNoteEffect(Player player)
        {
            if (Main.netMode == NetmodeID.Server || !Utils.NextBool(Main.rand, 50))
                return;
            Gore.NewGoreDirect(player.GetSource_Accessory(Item), player.Center, Vector2.Zero, ModLoader.GetMod("ThoriumMod").Find<ModGore>("NoteEffect").Type, 0.5f).velocity = new Vector2(Utils.NextFloat(Main.rand, -1.5f, 1.5f), -2f);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe()
                .AddIngredient<Headset>()
                .AddIngredient<TunePlayerDamage>()
                .AddIngredient<TunePlayerDamageReduction>()
                .AddIngredient<TunePlayerLifeRegen>()
                .AddIngredient<TunePlayerMovementSpeed>();
                

            if (ModLoader.TryGetMod("ThoriumRework", out Mod helheim))
            {
                if (helheim.TryFind("ConcussiveInstrument", out ModItem cInstrument))
                    recipe.AddIngredient(cInstrument.Type);
            }

            if (calamity != null)
            {
                recipe.AddIngredient(calamity.Find<ModItem>("AuricBar").Type, 5);
                recipe.AddTile(calamity.Find<ModTile>("CosmicAnvil").Type);
            }
            else
            {
                recipe.AddIngredient(ItemID.LunarBar, 5);
                recipe.AddTile(TileID.LunarCraftingStation);
            }

            recipe.Register();
        }
    }
}
