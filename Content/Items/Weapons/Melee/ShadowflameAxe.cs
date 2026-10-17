using InfernalEclipseWeaponsDLC.Common;
using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.ShadowflameAxePro;
using InfernalEclipseWeaponsDLC.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee
{
    public class ShadowflameAxe : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => WeaponConfig.Instance.AIGenedWeapons;

        private static Mod Calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public override void SetDefaults()
        {
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;
            Item.DamageType = Calamity != null ? Calamity.Find<DamageClass>("TrueMeleeDamageClass") : DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.damage = 113;
            Item.width = 52;
            Item.height = 38;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.knockBack = 8f;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<ShadowflameAxeProjectile>();

            Item.GetGlobalItem<WeaponsGlobalItem>().verveineItem = true;
        }

        public override void HoldItem(Player player)
        {
            foreach (Projectile projectile in Main.projectile)
            {
                if (projectile.type == Item.shoot && projectile.owner == player.whoAmI && projectile.active && projectile.ai[1] < 5)
                { // flips the player near the end of the swing animation
                    player.direction = (int)projectile.localAI[1];
                    return;
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            foreach (Projectile projectile in Main.projectile)
            {
                if (projectile.type == Item.shoot && projectile.owner == player.whoAmI && projectile.active)
                { // Axe already exists, make its ai[0] progress for the swing combo
                    projectile.ai[0]++; // swing combo count
                    projectile.ai[1] = 80f; // swing timer

                    if (projectile.ai[0] > 2)
                    { // reset combo after 3rd swing
                        projectile.ai[0] = 0;
                    }
                    else if (projectile.ai[0] == 2)
                    { // longer swing timer for 3rd swing
                        projectile.localAI[0] = 1; // used to reset swing damage during the spin attack
                        projectile.ai[1] = 140f; // swing timer

                        int projectileType = ModContent.ProjectileType<ShadowflameAxeBolt>();
                        SoundEngine.PlaySound(SoundID.Item103, player.Center);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 boltVelocity = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i + MathHelper.PiOver4).RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(7f, 13f);
                            Projectile.NewProjectileDirect(source, player.Center, boltVelocity, projectileType, (int)(Item.damage * 0.5f), Item.knockBack * 0.5f, player.whoAmI, 90, 0f, Main.rand.Next(30) - 15);
                        }
                    }

                    projectile.ai[2] = (Main.MouseWorld - player.Center).ToRotation() - MathHelper.PiOver2; // target angle

                    projectile.ResetLocalNPCHitImmunity();
                    projectile.netUpdate = true;
                    return false;
                }
            }

            // Axe does not exist, spawn it at the beginning of the swing combo

            Projectile newProjectile = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            newProjectile.ai[1] = 80f; // swing timer
            newProjectile.ai[2] = (Main.MouseWorld - player.Center).ToRotation() - MathHelper.PiOver2; // target angle

            return false;
        }
    }
}
