using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items;
using CalamityMod.Sounds;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Audio;
using InfernalEclipseWeaponsDLC.Content.Projectiles.RangedPro.Void;
using CalamityMod.Items.Materials;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged.Void
{
    public class ThunderboltActionSniper : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("SOTS");
        public override void SetDefaults()
        {
            Item.damage = 1440;
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
            Item.width = 162;
            Item.height = 65;
            Item.crit = 26;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-29, -10f);
        public override void HoldItem(Player player) => player.scope = true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = Vector2.Normalize(velocity + new Vector2(0, -0.6f)) * 122f;
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
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<VoidBolt>(), damage, knockback, player.whoAmI);
            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SniperRifle)
                .AddIngredient<UelibloomBar>(5)
                .AddIngredient<ArmoredShell>(3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}