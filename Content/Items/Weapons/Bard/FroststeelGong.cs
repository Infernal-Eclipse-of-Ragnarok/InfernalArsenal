using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class FroststeelGong : BigInstrumentItemBase
    {
        public override bool ForceDisableAutoReuse => true;

        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;

        public override bool CanChangePitch => false;

        private int froststeelCombo = 0; // combo tracker — stays within this item

        public override void SafeSetStaticDefaults()
        {
            Empowerments.AddInfo<AquaticAbility>(2);
            Empowerments.AddInfo<FlightTime>(2);
            Empowerments.AddInfo<JumpHeight>(2);
            Empowerments.AddInfo<MovementSpeed>(2);
            Empowerments.AddInfo<InvincibilityFrames>(2);
            Empowerments.AddInfo<ResourceMaximum>(2);
            Empowerments.AddInfo<ResourceRegen>(2);
        }

        public override void SafeSetBardDefaults()
        {
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.holdStyle = 3;
            Item.width = 30;
            Item.height = 30;
            Item.shootSpeed = 20;
            Item.shoot = ModContent.ProjectileType<FroststeelPulse>();
            Item.autoReuse = false;
            Item.damage = 600;
            Item.knockBack = 4f;
            Item.UseSound = ThoriumSounds.Gong_Sound;
            Item.rare = ModLoader.TryGetMod("Clamity", out Mod clamity) ? clamity.Find<ModRarity>("EvercoldCyan").Type : ModContent.RarityType<BurnishedAuric>();
            Item.value = Item.buyPrice(2, 40);
            InspirationCost = 10;
        }

        public override void UseItemFrame(Player player)
        {
            base.UseItemFrame(player);
        }

        public override void AddRecipes()
        {
            // Always get Thorium (we depend on it anyway)
            Mod thorium = ModLoader.GetMod("ThoriumMod");
            ModLoader.TryGetMod("CalamityMod", out Mod calamity);
            ModLoader.TryGetMod("Clamity", out Mod clamity);

            Recipe recipe = CreateRecipe();
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
            {
                recipe.AddIngredient(thoriumMod.Find<ModItem>("FrostwindCymbals").Type, 1);
                recipe.AddIngredient(thoriumMod.Find<ModItem>("SteelDrum").Type, 1);
            }
            if (clamity != null)
            {
                recipe.AddIngredient(clamity.Find<ModItem>("EnchantedMetal").Type, 8);
            }
            else
            {
                recipe.AddIngredient(calamity.Find<ModItem>("AuricBar").Type, 8);
                recipe.AddIngredient(calamity.Find<ModItem>("EndothermicEnergy").Type, 20);
            }
            recipe.AddTile(ModContent.TileType<CosmicAnvil>());
            recipe.Register();
            base.AddRecipes();
        }

        public override void Shoot_OnSuccess(Player player)
        {
            froststeelCombo++;

            for (int i = 0; i < 20; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(1f, 1f) * 20f;
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.Snow, offset.SafeNormalize(Vector2.Zero) * 50f, 100, default, 1.2f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item50, player.Center); // frosty chime sound
        }

        public override void SafeBardShoot(int success, int level, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire Froststeel Pulse directly from the player’s center
            Projectile.NewProjectile(
                source,
                player.Center,
                Vector2.Zero, // no movement, it grows in place
                ModContent.ProjectileType<FroststeelPulse>(),
                damage,
                knockback,
                player.whoAmI
            );

            // Every 3rd successful use, release FrostwindCymbals ring
            if (froststeelCombo >= 3)
            {
                froststeelCombo = 0; // reset combo

                Mod thorium = ModLoader.GetMod("ThoriumMod");
                if (thorium != null)
                {
                    int projType = thorium.Find<ModProjectile>("FrostwindCymbalsPro1").Type;
                    int count = 8;
                    float speed = Item.shootSpeed;

                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.ToRadians(360f / count * i);
                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        Projectile.NewProjectile(
                            source,
                            player.Center,
                            dir * speed,
                            projType,
                            (int)(damage * 1f),
                            knockback,
                            player.whoAmI
                        );
                    }

                    // Dust & sound burst
                    for (int i = 0; i < 40; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f) * 50f;
                        Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                            offset.SafeNormalize(Vector2.Zero) * 6f, 150, default, 1.4f);
                        d.noGravity = true;
                    }
                }
            }
        }
    }
}
