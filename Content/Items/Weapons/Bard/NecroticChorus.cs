using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NecrooticChorus;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class NecroticChorus : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<CriticalStrikeChance>(3);
            Empowerments.AddInfo<Damage>(2);
            Empowerments.AddInfo<AttackSpeed>(2);
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetBardDefaults()
        {
            Item.damage = 50;
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 20f;
            Item.value = Item.buyPrice(0, 60);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = ThoriumSounds.Bard_Horn;
            Item.shoot = ModContent.ProjectileType<NecroticChorusWisp>();
            Item.shootSpeed = 4f;

            InspirationCost = 3;

            Item.useStyle = ItemUseStyleID.Shoot;
            if (!ModLoader.HasMod("Look"))
            {
                Item.holdStyle = 3;
            }
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanPlayInstrument(Player player) => player.altFunctionUse == 2 || player.ownedProjectileCounts[Item.shoot] < 10;

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(8, -15);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 5 * player.direction;
            player.itemLocation.Y -= 4;
            player.itemRotation = 0;

        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: Shoot 7 blood bolts in shotgun spread
                Vector2 shootPosition = position;
                shootPosition.X += 38 * player.direction;
                shootPosition.Y -= 18;

                int projectileType = ModContent.ProjectileType<NecroticChorusPro>();
                int boltDamage = (int)(damage * 0.8f);
                float boltSpeed = 20f;
                
                Vector2 baseVelocity = Vector2.Normalize(velocity) * boltSpeed;

                for (int i = 0; i < 7; i++)
                {
                    float spreadAngle = (i - 3) * 0.087f;
                    
                    float randomSpread = Main.rand.Next(-5, 6) * ((float)Math.PI / 4f) * 0.01f;
                    
                    float speedVariation = Main.rand.NextFloat(0.9f, 1.1f);
                    
                    Vector2 spreadVelocity = baseVelocity.RotatedBy(spreadAngle + randomSpread) * speedVariation;
                    
                    Projectile.NewProjectile(source, shootPosition, spreadVelocity, projectileType, boltDamage, knockback, player.whoAmI);
                }
                
                return false;
            }
            else
            {
                // Left-click: Default behavior (wisps)
                return true;
            }
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Item.shootSpeed = 4f;
            // Right-click logic now handled by BardShoot method
        }
    }
}