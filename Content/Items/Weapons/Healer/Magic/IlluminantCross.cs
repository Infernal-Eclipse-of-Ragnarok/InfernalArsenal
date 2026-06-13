using CalamityMod.Items;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Projectiles.Healer;
using ThoriumMod.Tiles;
using System;
using ThoriumMod.Buffs.Healer;
using InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro;
using Terraria.GameContent.RGB;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Magic
{
    [ExtendsFromMod("ThoriumMod")]
    public class IlluminantCross : ThoriumItem
    {
        public override void SetDefaults()
        {
            Item.Size = new Vector2(10, 20);
            Item.damage = 30;
            Item.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
            Item.noMelee = true;
            Item.mana = 5;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.autoReuse = true;

            Item.channel = true;

            Item.knockBack = 6f;

            Item.rare = ItemRarityID.Blue;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.UseSound = SoundID.Item24;

            Item.shoot = ModContent.ProjectileType<IlluminantCrossPro>();
            Item.shootSpeed = 0f;

            isHealer = true;
            healType = HealType.Ally;
            healAmount = 5;
            healDisplay = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-14f, -7f);
        }

        public override void AddRecipes()
        {
            if (!ModLoader.TryGetMod("SOTS", out Mod _))
            {
                CreateRecipe()
                    .AddIngredient<PalmCross>(1)
                    .AddIngredient(ItemID.Obsidian, 20)
                    .AddIngredient(ItemID.GlowingMushroom, 50)
                    .AddTile(ModContent.TileType<ArcaneArmorFabricator>())
                    .Register();
            }
        }
    }
}



