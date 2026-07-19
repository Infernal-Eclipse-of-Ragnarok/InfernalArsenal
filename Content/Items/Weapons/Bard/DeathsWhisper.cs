using System;
using System.Collections.Generic;
using ThoriumMod.Items;
using ThoriumMod.Empowerments;
using ThoriumMod.Sounds;
using ThoriumMod;
using CalamityMod.Items;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [LegacyName("DogClarinet")]
    public class DeathsWhisper : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<LifeRegeneration>(2);
            Empowerments.AddInfo<ResourceConsumptionChance>(3);
            Empowerments.AddInfo<ResourceRegen>(3);
        }

        public override void SetBardDefaults()
        {
            Item.width = 62;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<DeathsWhisperPro>();
            Item.UseSound = ThoriumSounds.Clarinet_Sound;

            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.knockBack = 1.5f;
            Item.damage = 3175;
            Item.crit = 16;
            Item.shootSpeed = 11f;

            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();

            InspirationCost = 2;

            if (!ModLoader.HasMod("Look"))
            {
                Item.holdStyle = 3;
            }
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(6, 0);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += Utils.RotatedBy(new Vector2((float)(ModLoader.HasMod("Look") ? (-4) : (-6)), (float)(ModLoader.HasMod("Look") ? 6 : 8)) * player.Directions, (double)player.itemRotation, default(Vector2));
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 12 * player.direction;
            player.itemLocation.Y += 10;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            float offsetX = 36f;
            float offsetY = 14f;

            offsetX *= player.direction;

            Vector2 offset = new Vector2(offsetX, offsetY);

            offset = offset.RotatedBy(velocity.ToRotation());

            if (player.direction == -1)
            {
                offset.X *= -1f;
                offset.Y *= -1f;
            }

            position += offset;
        }

        public override bool BardShoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<DeathsWhisperPlayer>();

            SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/DoGFireball"), new Vector2?(player.position));

            if (modPlayer.whisperHitCounter >= 5)
            {
                modPlayer.whisperHitCounter = 0;

                int count = 8;
                float speed = velocity.Length();

                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count;
                    Vector2 newVelocity = angle.ToRotationVector2() * speed;

                    int proj = Projectile.NewProjectile(
                        source,
                        position,
                        newVelocity,
                        type,
                        (damage / 2),
                        knockback,
                        player.whoAmI
                    );

                    if (Main.projectile.IndexInRange(proj))
                    {
                        Main.projectile[proj].ai[0] = 2f; // enable homing
                    }
                }

                SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/DevourerAttack")
                {
                    Volume = 1f,
                    PitchVariance = 0.2f
                }, player.Center);

                return false;
            }

            return true;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<DeathsWhisperPlayer>();

            DeathsWhisperDrawHelper.DrawChargeBarInInventory(
                spriteBatch,
                position,
                frame,
                modPlayer.whisperHitCounter,
                scale
            );
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "UniversalLore1", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.UniversalLore1")) { OverrideColor = Color.MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "UniversalLore2", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.UniversalLore2")) { OverrideColor = Color.MediumPurple });
        }
    }

    public class DeathsWhisperPlayer : ModPlayer
    {
        public int whisperHitCounter = 0;

        public void IncrementHitCounter()
        {
            whisperHitCounter++;
        }
    }

    public static class DeathsWhisperDrawHelper
    {
        public static void DrawChargeBarInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, float charge, float scale)
        {
            if (charge <= 0f) return;

            Texture2D barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            Texture2D barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 origin = barBG.Size() * 0.5f;

            // position under item
            Vector2 drawPos = position + Vector2.UnitY * scale * (frame.Height - 5.5f);

            Rectangle crop = new Rectangle(0, 0, (int)(charge / 5f * barFG.Width), barFG.Height);

            // Purple to Cyan cycling
            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 3f) + 1f) / 2f);
            Color color = Color.Lerp(Color.Fuchsia, Color.Cyan, t);

            float barScale = 1.5f;

            spriteBatch.Draw(barBG, drawPos, null, color, 0f, origin, scale * barScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(barFG, drawPos, crop, color * 0.85f, 0f, origin, scale * barScale, SpriteEffects.None, 0f);
        }
    }

    public class DeathsWhisperChargeLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<DeathsWhisperPlayer>();

            if (player.HeldItem.ModItem is not DeathsWhisper)
                return;

            if (modPlayer.whisperHitCounter <= 0)
                return;

            Texture2D barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            Texture2D barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 origin = barBG.Size() * 0.5f;
            Vector2 drawPos = player.Top - Vector2.UnitY * 16f - Main.screenPosition;

            Rectangle crop = new Rectangle(0, 0,
                (int)(modPlayer.whisperHitCounter / 5f * barFG.Width),
                barFG.Height);

            // Purple - Cyan pulse
            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 3f) + 1f) / 2f);
            Color color = Color.Lerp(Color.Fuchsia, Color.Cyan, t);

            float scale = 0.95f;

            Main.spriteBatch.Draw(barBG, drawPos, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(barFG, drawPos, crop, color * 0.85f, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
