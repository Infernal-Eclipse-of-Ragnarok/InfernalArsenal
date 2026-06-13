using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoriumMod.Items;
using ThoriumMod.Empowerments;
using ThoriumMod.Sounds;
using ThoriumMod;
using CalamityMod.Items;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using CalamityMod.CustomRecipes;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    public class TheParallel : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<InvincibilityFrames>(4);
            Empowerments.AddInfo<Defense>(2);
            Empowerments.AddInfo<DamageReduction>(2);
        }

        public override void SetBardDefaults()
        {
            Item.width = 44;
            Item.height = 46;
            Item.holdStyle = 5;
            Item.useStyle = ItemUseStyleID.Guitar;
            Item.holdStyle = 5;
            Item.useTime = 4;
            Item.useAnimation = 12;
            Item.reuseDelay = 20;
            Item.damage = 131;
            Item.autoReuse = true;
            Item.knockBack = 1.5f;
            Item.noMelee = true;
            Item.shootSpeed = 14f;
            Item.shoot = ModContent.ProjectileType<TheParallellPro>();
            Item.DamageType = ThoriumDamageBase<BardDamage>.Instance;
            Item.UseSound = ThoriumSounds.SunflareString_Sound;
            InspirationCost = 1;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int count = 3;                  // number of daggers per use
            float maxRadius = 150f;         // max distance from player to spawn
            float daggerSpeed = 14f;        // projectile speed

            for (int i = 0; i < count; i++)
            {
                // Random position within radius
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float distance = Main.rand.NextFloat(maxRadius);
                Vector2 spawnPos = player.Center + angle.ToRotationVector2() * distance;

                // Direction toward cursor
                Vector2 shootDir = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.Zero);

                // Spawn projectile
                int proj = Projectile.NewProjectile(
                    source,
                    spawnPos,
                    shootDir * daggerSpeed,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                // Optional: store phase/AI info for trails or effects
                if (Main.projectile.IndexInRange(proj))
                    Main.projectile[proj].ai[1] = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            return false; // suppress default projectile spawn
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-3, 3);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(-3, 3f) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = new Vector2(-3, 3f) * player.Directions;

            player.itemLocation += offset;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ParallelLore1", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.ParallelLore1")) { OverrideColor = Color.MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "ParallelLore2", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.ParallelLore2")) { OverrideColor = Color.MediumPurple });
        }
    }
}
