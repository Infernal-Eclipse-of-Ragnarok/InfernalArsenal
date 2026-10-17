using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.WhipPro.DNAWhip;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Whips
{
    public class Deoxyribonunoboko : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DeoxyribonunobokoWhipTagDefault.TagDamage, DeoxyribonunobokoWhipTagDefault.CritChance);

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.damage = 275;
            Item.knockBack = 4f;
            Item.shoot = ModContent.ProjectileType<DNAWhipPro>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shootSpeed = 10f;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item152;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(1, 20);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.ownedProjectileCounts[type] < 1;

        public override void AddRecipes()
        {
            base.AddRecipes();
        }

        public override bool MeleePrefix() => true;
    }
}
