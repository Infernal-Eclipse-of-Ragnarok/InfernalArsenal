using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Healing;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Core.Sheaths;
using ThoriumMod.Items.MeleeItems;
using ThoriumMod.Items.Misc;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.Sheaths
{
    public class TerraSheath : SwordSheathBase
    {
        public override SheathData SheathData => SheathDataLoader.Get<TerraSheathData>();

        public override LocalizedText Tooltip
        {
            get
            {
                return base.Tooltip.WithFormatArgs(new object[2]
                {
                    (JetstreamSheathData.SlashCount + 1),
                    CooldownToString()
                });
            }
        }

        public override void SafeSetDefaults()
        {
            accDamage = $"{this.SheathData.DamageMultiplier * 100f}% basic damage";
            Item.width = 36;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<LeechingSheath>()
                .AddIngredient<BrokenHeroFragment>(3)
                .AddIngredient<LivingShard>(12)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    public class TerraSheathData : SheathData
    {
        public override SoundStyle? HitSound => SoundID.Item71;

        public override float DamageMultiplier => 2f;

        public override int HitDustType => 235;

        public override int DustType => 235;

        public override int DustAlpha => 0;

        public override float DustScale => 1.5f;

        protected override void OnHit(Player player, NPC target, NPC.HitInfo hit, int damageDone, int hitCount)
        {
            Vector2 center = player.Center + new Vector2(player.direction * 160f, 0f);

            int damage = hit.SourceDamage * 2;
            int projectileType = ModContent.ProjectileType<TerraOrb>();

            if (hitCount == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<TerraOrb>()] <= 4)
                        Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, player.DirectionTo(target.Center).RotatedByRandom(1.25f) * 1.75f, projectileType, damage, 0f, player.whoAmI);
                }
            }
        }
    }
}
