using CalamityMod;
using CalamityMod.CalPlayer;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CatalystMod.Items;
using InfernalEclipseWeaponsDLC.Content.Items.Accessories.Donor;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.NPCItems;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    [AutoloadEquip(EquipType.Neck)]
    public class BlightedBadge : ModItem
    {
        private Mod catalyst = null;

        public static int ThornsDamage => CalamityUtils.ScaleWithDifficulty(225);

        public override void SetStaticDefaults()
        {
            ModLoader.TryGetMod("CatalystMod", out catalyst);
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.accessory = true;
            Item.expert = true;
        }

        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            return (equippedItem.type == ModContent.ItemType<HideofAstrumDeus>() || equippedItem.type == Item.type) != (equippedItem.type == ModContent.ItemType<HideofAstrumDeus>() || incomingItem.type == Item.type);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetThoriumPlayer().RapierBadge = true;

            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.hideOfDeus = true;
            if (modPlayer.hideOfDeusMeleeBoostTimer > 0)
                player.GetDamage<TrueMeleeDamageClass>() += 0.3f;
            if (!hideVisual)
                modPlayer.arcFlashRingVisual = true;

            player.GetModPlayer<InfernalWeaponsPlayer>().blightedBadge = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<RapierBadge>()
                .AddIngredient<HideofAstrumDeus>()
                .AddIngredient((catalyst != null ? catalyst.Find<ModItem>("MetanovaBar").Type : ModContent.ItemType<AstralBar>()), 5)
                .AddIngredient<ArmoredShell>(3)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    [JITWhenModsEnabled("CatalystMod")]
    [ExtendsFromMod("CatalystMod")]
    public class BlightBadgeNameDrawingHelper : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ModContent.ItemType<BlightedBadge>() || entity.type == ModContent.ItemType<ImagiknightHeraldry>();
        }

        public override void SetDefaults(Item entity)
        {
            entity.rare = CatalystItem.RarityExpertSuperboss;
        }
    }
 }
