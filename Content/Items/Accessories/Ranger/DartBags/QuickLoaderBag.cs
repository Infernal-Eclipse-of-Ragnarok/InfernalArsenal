using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.RangedItems;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Ranger.DartBags
{
    [AutoloadEquip(
    [
        //EquipType.Waist for once I get a waist sprite
    ])]
    public class QuickLoaderBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            //ArmorIDs.Waist.Sets.IsABelt[Item.waistSlot] = true; for once I get a waist sprite
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ThoriumPlayer thoriumPlayer = player.GetThoriumPlayer();
            thoriumPlayer.accDartPouch = true;
            player.GetModPlayer<InfernalWeaponsPlayer>().quickLoader = true;
            if (Item.useAmmo == AmmoID.Dart)
            {
                player.GetDamage(DamageClass.Ranged) += 0.1f;
                player.GetAttackSpeed(DamageClass.Ranged) += 0.1f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<DartPouch>(1)
                .AddIngredient<InfectedArmorPlating>(15)
                .AddIngredient<PlagueCellCanister>(20)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    public class QuickLoaderBagProjectile : GlobalProjectile //PSST EY, I donno if it's WH or Akira reading this but add a global projotile file so we can put this here. Or if there's one tell me about it.
{
    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player owner = Main.player[projectile.owner];

        if (owner.HeldItem.useAmmo == AmmoID.Dart && owner.GetModPlayer<InfernalWeaponsPlayer>().quickLoader)
        {
            target.AddBuff(ModContent.BuffType<Plague>(), 180);
        }
    }
}
}