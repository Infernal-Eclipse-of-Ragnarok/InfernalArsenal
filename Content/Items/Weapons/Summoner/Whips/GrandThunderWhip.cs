using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.WhipPro.GrandThunderWhip;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Whips
{
    public class GrandThunderWhip : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.damage = 14;
            Item.knockBack = 2f;
            Item.shoot = ModContent.ProjectileType<GrandThunderWhipProjectile>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shootSpeed = 12f;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item152;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(0, 1);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.ownedProjectileCounts[type] < 1;
        public override bool MeleePrefix() => true;
    }
}
