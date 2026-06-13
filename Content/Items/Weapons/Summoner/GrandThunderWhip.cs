using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items;
using CalamityMod.Projectiles.Melee;
using Microsoft.CodeAnalysis.Operations;
using Terraria.DataStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.WhipPro;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner
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
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        }

        public override bool MeleePrefix() => true;

        public override bool Shoot(
          Player player,
          EntitySource_ItemUse_WithAmmo source,
          Vector2 position,
          Vector2 velocity,
          int type,
          int damage,
          float knockback
        )
        {
            return player.ownedProjectileCounts[type] < 1;
        }
    }
}
