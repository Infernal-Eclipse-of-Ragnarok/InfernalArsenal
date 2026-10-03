using CalamityMod.Items;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class JollyTune : BigInstrumentItemBase
    {
        public override bool ForceDisableAutoReuse => true;

        public override BardInstrumentType InstrumentType => BardInstrumentType.Brass;

        public override bool CanChangePitch => false;

        private int GrinderFrame = 0;
        private int JollyTuneProjectileFrame = 0;

        private Texture2D GrinderAnimationTexture => ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Bard/JollyTuneSheet").Value;

        private static readonly SoundStyle GrinderSong = new SoundStyle("InfernalEclipseWeaponsDLC/Assets/Sounds/JollyTune"){IsLooped = true, Volume = 1f};

        public override void SafeSetStaticDefaults()
        {
            Empowerments.AddInfo<AttackSpeed>(2);
            Empowerments.AddInfo<CriticalStrikeChance>(2);
            Empowerments.AddInfo<MovementSpeed>(2);
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
            Item.shoot = ModContent.ProjectileType<JollyTunePro>();
            Item.autoReuse = true;
            Item.damage = 400;
            Item.knockBack = 4f;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            InspirationCost = 10;
            Item.UseSound = null;
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(24, 0) * player.Directions;
        }

        public override void UseItemFrame(Player player)
        {
            ((ModItem)this).HoldItemFrame(player);
        }

        public override void AddRecipes()
        {
            ModLoader.TryGetMod("CalamityMod", out Mod calamity);

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.MusicBox, 1);
            recipe.AddIngredient(ItemID.Present, 2);

            recipe.AddIngredient(calamity.Find<ModItem>("EndothermicEnergy").Type, 25);

            recipe.AddTile(ModContent.TileType<CosmicAnvil>());
            recipe.Register();
            base.AddRecipes();
        }

        public override void Shoot_OnSuccess(Player player)
        {
            GrinderFrame++;
            if (GrinderFrame >= 8)
                GrinderFrame = 0;

            SoundEngine.PlaySound(SoundID.MenuTick, player.Center);

            RefreshGrinderSong(player);
        }

        public override void SafeBardShoot(int success, int level, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int currentCount = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (!projectile.active ||
                    projectile.type != ModContent.ProjectileType<JollyTunePro>() ||
                    projectile.owner != player.whoAmI)
                    continue;

                projectile.timeLeft = (60 * 10);

                if (projectile.ai[2] == 0f)
                    currentCount++;
            }

            int newProjectileIndex = currentCount;

            int orbitCount = currentCount + 1;

            float angleStep = MathHelper.TwoPi / orbitCount;

            float angle =
                Main.GameUpdateCount * JollyTunePro.OrbitSpeed + newProjectileIndex * angleStep;

            Vector2 spawnPosition = player.Center + angle.ToRotationVector2() * JollyTunePro.OrbitRadius;

            Projectile newProjectile = Projectile.NewProjectileDirect(
                source,
                spawnPosition,
                Vector2.Zero,
                ModContent.ProjectileType<JollyTunePro>(),
                damage,
                knockback,
                player.whoAmI
            );

            // Which sprite this projectile uses
            newProjectile.ai[0] = JollyTuneProjectileFrame;

            JollyTuneProjectileFrame++;

            if (JollyTuneProjectileFrame >= 2)
                JollyTuneProjectileFrame = 0;

            // This projectile's position in the orbiting ring
            newProjectile.ai[1] = currentCount;

            // ai[2] = 0 - orbiting
            // ai[2] = 1 - fired/homing
            newProjectile.ai[2] = 0f;

            // The 8th projectile causes the entire ring to fire
            if (currentCount + 1 >= 8)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile projectile = Main.projectile[i];

                    if (!projectile.active ||
                        projectile.type != ModContent.ProjectileType<JollyTunePro>() ||
                        projectile.owner != player.whoAmI ||
                        projectile.ai[2] != 0f)
                        continue;

                    // 2 = waiting to be released.
                    // This gives the newly spawned projectile time to enter the ring.
                    projectile.ai[2] = 2f;

                    projectile.netUpdate = true;
                    projectile.penetrate = 1;
                }
            }
        }

        private void RefreshGrinderSong(Player player)
        {
            JollyTunePlayer modPlayer = player.GetModPlayer<JollyTunePlayer>();

            modPlayer.SongVolume = JollyTunePlayer.MaxSongVolume;

            if (SoundEngine.TryGetActiveSound(modPlayer.SongSlot, out ActiveSound activeSound))
            {
                activeSound.Volume = modPlayer.SongVolume;

                if (!activeSound.IsPlaying)
                    activeSound.Resume();

                return;
            }

            modPlayer.SongSlot = SoundEngine.PlaySound(
                GrinderSong,
                player.Center
            );
        }

        public override bool ModifyItemDraw( ref PlayerDrawSet drawInfo, ref DrawData drawData, ref DrawData? coloredDrawData, ref DrawData? glowMaskDrawData)
        {
            Texture2D texture = GrinderAnimationTexture;

            int frameHeight = texture.Height / 8;

            Rectangle sourceRectangle = new Rectangle(
                0,
                GrinderFrame * frameHeight,
                texture.Width,
                frameHeight
            );

            Vector2 frameOrigin = new Vector2(
                texture.Width / 2f,
                frameHeight / 2f
            );

            // Use the animation sheet instead of the item's normal texture.
            drawData.texture = texture;
            drawData.sourceRect = sourceRectangle;
            drawData.origin = frameOrigin;

            // Colored draw
            if (coloredDrawData.HasValue)
            {
                DrawData colored = coloredDrawData.Value;

                colored.texture = texture;
                colored.sourceRect = sourceRectangle;
                colored.origin = frameOrigin;

                coloredDrawData = colored;
            }

            // Glow mask draw
            if (glowMaskDrawData.HasValue)
            {
                DrawData glow = glowMaskDrawData.Value;

                glow.texture = texture;
                glow.sourceRect = sourceRectangle;
                glow.origin = frameOrigin;

                glowMaskDrawData = glow;
            }

            return true;
        }

        public override bool PreDrawInInventory( SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D texture = GrinderAnimationTexture;

            int frameHeight = texture.Height / 8;

            Rectangle sourceRectangle = new Rectangle(
                0,
                GrinderFrame * frameHeight,
                texture.Width,
                frameHeight
            );

            Vector2 frameOrigin = new Vector2(
                texture.Width / 2f,
                frameHeight / 2f
            );

            spriteBatch.Draw(
                texture,
                position,
                sourceRectangle,
                drawColor,
                0f,
                frameOrigin,
                scale * 1,
                SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.orpheus"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
                line5.OverrideColor = new Color(196, 35, 44);
                tooltips.Add(line5);
            }
            else
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor"));
                line5.OverrideColor = new Color(196, 35, 44);
                tooltips.Add(line5);
            }
        }
    }

    public class JollyTunePlayer : ModPlayer
    {
        public SlotId SongSlot;
        public float SongVolume;
        public const float MaxSongVolume = 0.5f;
        public const float SongVolumeDrainTime = 100f;

        public override void PostUpdate()
        {
            bool hasJollyTune = false;

            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == ModContent.ItemType<JollyTune>())
                {
                    hasJollyTune = true;
                    break;
                }
            }

            bool holdingJollyTune =
                Player.HeldItem.type == ModContent.ItemType<JollyTune>();

            if (!hasJollyTune)
            {
                if (SoundEngine.TryGetActiveSound(SongSlot, out ActiveSound sound))
                {
                    if (sound.IsPlaying)
                        sound.Pause();
                }

                return;
            }

            if (!holdingJollyTune)
            {
                if (SoundEngine.TryGetActiveSound(SongSlot, out ActiveSound sound))
                {
                    if (sound.IsPlaying)
                        sound.Pause();
                }

                return;
            }

            if (!SoundEngine.TryGetActiveSound(SongSlot, out ActiveSound activeSound))
                return;

            if (!activeSound.IsPlaying && SongVolume > 0f)
                activeSound.Resume();

            SongVolume -= MaxSongVolume / SongVolumeDrainTime;

            if (SongVolume <= 0f)
            {
                SongVolume = 0f;
                activeSound.Volume = 0f;
                activeSound.Pause();
                return;
            }

            activeSound.Volume = SongVolume;
            activeSound.Position = Player.Center;
        }
    }
}
