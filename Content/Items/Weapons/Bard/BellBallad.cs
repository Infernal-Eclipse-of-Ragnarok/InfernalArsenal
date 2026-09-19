using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBallad : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<MovementSpeed>(2);
            Empowerments.AddInfo<FlightTime>(2);
            Empowerments.AddInfo<Damage>(1);
            Empowerments.AddInfo<LifeRegeneration>(1);
        }

        public override void SetBardDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.holdStyle = ItemHoldStyleID.HoldFront;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ProjectileID.None;
            Item.UseSound = SoundID.Item35;

            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.knockBack = 1.5f;
            Item.damage = 75;
            Item.shootSpeed = 14f;

            Item.value = Item.buyPrice(0, 45);
            Item.rare = ItemRarityID.Lime;

            InspirationCost = 1;
        }

        public override void BardHoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.ownedProjectileCounts[ModContent.ProjectileType<BellBalladEleum>()] < 1)
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<BellBalladEleum>(), Item.damage, Item.knockBack, Main.myPlayer, ai0: 0);

                if (player.ownedProjectileCounts[ModContent.ProjectileType<BellBalladHavoc>()] < 1)
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<BellBalladHavoc>(), Item.damage, Item.knockBack, Main.myPlayer, ai0: 1);

                if (player.ownedProjectileCounts[ModContent.ProjectileType<BellBalladSunlight>()] < 1)
                    Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<BellBalladSunlight>(), Item.damage, Item.knockBack, Main.myPlayer, ai0: 2);
            }
        }

        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 12 * player.direction;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 12 * player.direction;
        }

        public override bool? BardUseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.owner == Main.myPlayer)
                    {
                        // BellBalladHavoc & BellBalladSunlight derive from BellBalladEleum
                        // So here they are NOT excluded!
                        if (proj.ModProjectile is BellBalladEleum bell)
                        {
                            bell.Shoot(Item.damage, Item.knockBack);
                        }
                    }
                }
                return true;
            }
            return base.BardUseItem(player);
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod cal))
            {
                CreateRecipe()
                    .AddIngredient(ItemID.Bell, 1)
                    .AddIngredient(cal.Find<ModItem>("EssenceofEleum").Type, 3)
                    .AddIngredient(cal.Find<ModItem>("EssenceofHavoc").Type, 3)
                    .AddIngredient(cal.Find<ModItem>("EssenceofSunlight").Type, 3)
                    .AddIngredient(cal.Find<ModItem>("AshesofCalamity").Type, 5)
                    .AddTile(TileID.MythrilAnvil)
                    .Register();
            }
            else
            {

            }
        }
    }
}
