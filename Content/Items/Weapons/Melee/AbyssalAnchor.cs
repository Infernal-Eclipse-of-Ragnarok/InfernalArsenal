using InfernalEclipseWeaponsDLC.Content.Projectiles.MeleePro.AbyssalAncor;
using InfernalEclipseWeaponsDLC.Core;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee
{
    public class AbyssalAnchor : ModItem
    {
        public Mod calamity => ModIntegrationsSystem.Calamity.Loaded ? ModIntegrationsSystem.Calamity.Mod : null;

        public float Combo = 0f;
        public float Charge = 0f;

        public const float ComboLength = 4f; //How many regular swings before the long throw happens

        public static float harpoonDamageMultiplier = 0.8f; //Damage multiplier for non-homing harpoon projectile
        public static float abyssCreatureDamageMultiplier = 0.2f; //Damage multiplier for the abyssal creature (1 glass stars per shot)

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 56;
            Item.damage = 5750;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.crit = 15;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 20;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.knockBack = 9.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(2, 40);
            Item.rare = (calamity != null) ? calamity.Find<ModRarity>("BurnishedAuric").Type : ItemRarityID.Purple;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 20f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            if (player.altFunctionUse != 2)
            {
                player.GetModPlayer<InfernalWeaponsPlayer>().mouseWorldListener = true;

                if (CanUseItem(player) && Combo != 4)
                    Item.channel = false;

                if (Combo == 4)
                    Item.channel = true;
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.ownedProjectileCounts[ProjectileType<AbyssalAimPro>()] > 0)
                    return false;
            }

            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<AbyssalAnchorSwungAnchor>());
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, player.Center, Vector2.Zero, ProjectileType<AbyssalAimPro>(), damage, knockback, player.whoAmI);
                return false;
            }

            float swingState = Combo == ComboLength ? 2 : Combo % 2;

            Vector2 throwDirection = velocity.SafeNormalize(new Vector2(player.direction, 0f));

            Vector2 spawnPosition = player.MountedCenter;

            if (swingState == 2f)
            {
                spawnPosition += throwDirection * AbyssalAnchorSwungAnchor.ThrowSpawnOffset;
            }

            Projectile.NewProjectile(source, spawnPosition, throwDirection * Item.shootSpeed, ProjectileType<AbyssalAnchorSwungAnchor>(), damage, knockback, player.whoAmI, swingState);

            Combo += 1;
            if (Combo > ComboLength)
                Combo = 0;

            if (swingState != 2f)
            {
                Projectile.NewProjectile(source, player.Center + Utils.SafeNormalize(velocity, Vector2.Zero) * 20, velocity, ProjectileType<AbyssCreature>(), (int)(damage * abyssCreatureDamageMultiplier), knockback, player.whoAmI);
            }

            Charge--;
            if (Charge < 0)
                Charge = 0;

            return false;
        }

        public override float UseTimeMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 1.5f; // 50% slower
            return 1f;
        }

        public override float UseAnimationMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 1.5f;
            return 1f;
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal) && !ModLoader.HasMod("InfernumMode"))
            {
                CreateRecipe()
                    .AddIngredient(ItemID.Anchor)
                    .AddIngredient(cal.Find<ModItem>("AuricBar"), 3)
                    .AddIngredient(cal.Find<ModItem>("ReaperTooth"), 4)
                    .AddIngredient(cal.Find<ModItem>("DepthCells"), 20)
                    .AddIngredient(cal.Find<ModItem>("RuinousSoul"), 2)
                    .AddTile(cal.Find<ModTile>("CosmicAnvil"))
                    .Register();
            }
            else if (cal == null)
            {
                CreateRecipe()
                    .AddIngredient(ItemID.Anchor)
                    .AddIngredient(ItemID.LunarBar, 5)
                    .AddIngredient(ItemID.SharkFin, 5)
                    .AddIngredient(ItemID.SoulofMight, 15)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
            }
        }

        public override ModItem Clone(Item item)
        {
            var clone = base.Clone(item);

            if (clone is AbyssalAnchor a && item.ModItem is AbyssalAnchor a2)
                a.Charge = a2.Charge;

            return clone;
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Charge);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Charge = reader.ReadSingle();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.Soopns"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
                line5.OverrideColor = new(196, 35, 44);
                tooltips.Add(line5);
            }
            else
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor"));
                line5.OverrideColor = new(196, 35, 44);
                tooltips.Add(line5);
            }
        }
    }
}
