using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro;
using InfernalEclipseWeaponsDLC.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Core.Sheaths;
using ThoriumMod.Items.MeleeItems;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.Sheaths
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    [AutoloadEquip(EquipType.Back, EquipType.Waist)]
    public class LightMatterSheath : SwordSheathBase
    {
        public static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public override SheathData SheathData => SheathDataLoader.Get<LightMatterSheathData>();

        public override LocalizedText Tooltip
        {
            get
            {
                return base.Tooltip.WithFormatArgs(new object[2]
                {
                    DamageMultiplierToString(),
                    CooldownToString()
                });
            }
        }

        public override void SafeSetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.value = Item.buyPrice(1, 50, 0, 0);
            Item.rare = Calamity != null ? Calamity.Find<ModRarity>("Turquoise").Type : ItemRarityID.Purple;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient<LeatherSheath>();

            if (Calamity != null)
            {
                recipe.AddIngredient(Calamity.Find<ModItem>("DivineGeode"), 5);
                recipe.AddIngredient(Calamity.Find<ModItem>("UnholyEssence"), 8);
            }
            else
            {
                recipe.AddIngredient(ItemID.LunarBar, 8);
                recipe.AddIngredient(ItemID.FragmentSolar, 5);
            }

            recipe.Register();
        }
    }

    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class LightMatterSheathData : SheathData
    {
        public override SoundStyle? HitSound => SoundID.Item71;

        public override float DamageMultiplier => 12f;

        public override float MultihitPenalty => 0.33f;

        public override int HitDustType => 235;

        public override int DustType => 235;

        public override int DustAlpha => 0;

        public override float DustScale => 1.5f;

        protected override void OnHit(Player player, NPC target, NPC.HitInfo hit, int damageDone, int hitCount)
        {
            int damage = hit.SourceDamage;
            int projType = ModContent.ProjectileType<HolyExplosion>();

            if (player.ownedProjectileCounts[projType] <= 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, target.Center);
                Projectile.NewProjectile(player.GetSource_OnHit(target), target.Center, Vector2.Zero, projType, damage, 0f, player.whoAmI);
            }
        }
    }
}
