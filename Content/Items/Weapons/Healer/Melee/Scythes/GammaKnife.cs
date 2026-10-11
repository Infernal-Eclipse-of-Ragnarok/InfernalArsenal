using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.CustomRecipes;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes.GammaKnife;
using System;
using Terraria.DataStructures;
using ThoriumMod;
using ThoriumMod.Items.HealerItems;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes
{
    public class GammaKnife : ScytheItem
    {
        // store projectile IDs ourselves
        private int swingProj;
        private int thrownProj;

        public override void SetStaticDefaults()
        {
            SetStaticDefaultsToScythe();
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public static readonly SoundStyle Sound = new($"{nameof(InfernalEclipseWeaponsDLC)}/Assets/Effects/Sounds/Swing", 2)
        {
            PitchVariance = 0.25f,
            Volume = 0.8f
        };

        public override void SetDefaults()
        {
            SetDefaultsToScythe();

            // assign our local fields (works regardless of ScytheItem internals)
            swingProj = ModContent.ProjectileType<GammaKnifeProjectile>();
            thrownProj = ModContent.ProjectileType<GammaKnifeThrownProjectile>();

            scytheSoulCharge = 3;

            Item.damage = 400;
            Item.knockBack = 2f;

            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.width = 86;
            Item.height = 74;

            Item.UseSound = Sound;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;

            // default to swing projectile
            Item.shoot = swingProj;
            Item.shootSpeed = 8f;

            Item.DamageType = ThoriumDamageBase<HealerDamage>.Instance;

            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override float UseTimeMultiplier(Player player)
        {
            // Stab is faster
            return player.altFunctionUse == 2 ? 1f : 1f;
        }

        public override float UseAnimationMultiplier(Player player)
        {
            return player.altFunctionUse == 2 ? 1f : 1f;
        }

        public override bool CanUseItem(Player player)
        {
            // Count active thrown projectiles
            int thrownCount = player.ownedProjectileCounts[thrownProj];

            // If a thrown projectile exists, disable using the item entirely
            if (thrownCount > 0)
                return false;

            bool alt = player.HasAltFunctionUse();

            Item.shoot = alt ? thrownProj : swingProj;

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float attackTime = player.itemAnimationMax;

            int p = Projectile.NewProjectile(
                source,
                position,
                velocity,     // You aren't using this for the swing itself but it's fine
                type,
                damage,
                knockback,
                player.whoAmI,
                attackTime,   // ai[0] = full duration
                attackTime    // ai[1] = current time
            );

            return false;
        }


        public override bool MeleePrefix()
        {
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MysteriousCircuitry>(18)
                .AddIngredient<DubiousPlating>(12)
                .AddIngredient<UelibloomBar>(8)
                .AddIngredient(ItemID.LunarBar, 4)
                .AddTile(TileID.LunarCraftingStation)
                .AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(4, out Func<bool> condition), condition)
                .Register();

            if (ModLoader.TryGetMod("CatalystMod", out Mod catalyst))
            {
                if (catalyst.TryFind("MetanovaBar", out ModItem metaNovaBar))
                {
                    // Catalyst-specific recipe using MetanovaBar instead of Uelibloom
                    CreateRecipe()
                        .AddIngredient<MysteriousCircuitry>(18)
                        .AddIngredient<DubiousPlating>(12)
                        .AddIngredient(metaNovaBar.Type, 4)
                        .AddIngredient(ItemID.LunarBar, 4)
                        .AddTile(TileID.LunarCraftingStation)
                        .AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(4, out Func<bool> condition2), condition2)
                        .Register();
                }
            }
        }
    }
}