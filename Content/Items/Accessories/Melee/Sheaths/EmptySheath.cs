using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro;
using InfernalEclipseWeaponsDLC.Core;
using Microsoft.Xna.Framework;
using SOTS;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Core.Sheaths;
using ThoriumMod.Items.MeleeItems;

namespace InfernalEclipseWeaponsDLC.Content.Items.Accessories.Melee.Sheaths
{
    [JITWhenModsEnabled("SOTS", "ThoriumMod")]
    [ExtendsFromMod("SOTS", "ThoriumMod")]
    [AutoloadEquip(EquipType.Back, EquipType.Waist)]
    public class EmptySheath : SwordSheathBase
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }

        public static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public override SheathData SheathData => SheathDataLoader.Get<EmptySheathData>();

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
            Item.rare = Calamity != null ? Calamity.Find<ModRarity>("PureGreen").Type : ItemRarityID.Purple;
        }
    }

    [JITWhenModsEnabled("SOTS", "ThoriumMod")]
    [ExtendsFromMod("SOTS", "ThoriumMod")]
    public class EmptySheathData : SheathData
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
        public override SoundStyle? HitSound => SoundID.Item71;

        public override float DamageMultiplier => GetDamageMult(Main.LocalPlayer);

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

        public static float GetDamageMult(Player player)
        {
            if (player == null || !player.active)
                return 0f;

            return player.VoidPlayer().voidMeter * 15f;
        }
    }
}
