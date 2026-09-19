using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using InfernalEclipseWeaponsDLC.Content.Projectiles.RangedPro.Void;
using SOTS.Void;
using CalamityMod.Items.Materials;
using SOTS.Items.Celestial;
using CalamityMod.Items;
using CalamityMod.Rarities;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged.Void
{
    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public class ThunderboltActionSniperVoid : VoidItem
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Ranged/Void/ThunderboltActionSniper";
        public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("SOTS");
        public override void SafeSetDefaults()
        {
            Item.damage = 1660;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<VoidBolt>();
            Item.shootSpeed = 6f;
            Item.useAmmo = AmmoID.Bullet;
            Item.width = 136;
            Item.height = 40;
            Item.crit = 26;
        }
        public override int GetVoid(Player player) => 20;
        public override Vector2? HoldoutOffset() => new Vector2(-25, -4f);
        public override void HoldItem(Player player) => player.scope = true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity + new Vector2(0, -0.6f)) * 112;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            if (type == 14)
            {
                type = ProjectileID.BulletHighVelocity;
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(CommonCalamitySounds.LargeWeaponFireSound with { Volume = 10f }, player.position);
            SoundEngine.PlaySound(SoundID.Thunder with { Volume = 10f }, player.position);
            Projectile.NewProjectile(source, position, velocity / 2, ModContent.ProjectileType<VoidBolt>(), damage, knockback, player.whoAmI);
            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SniperRifle)
                .AddIngredient<SanguiteBar>(15)
                .AddIngredient<ArmoredShell>(3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}