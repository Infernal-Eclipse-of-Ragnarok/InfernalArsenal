using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;
using ThoriumMod;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;
using CalamityMod.Items.Materials;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using System.IO;
using System.Collections.Generic;
using Terraria.Localization;
using CalamityMod.CustomRecipes;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class Infrariff : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<DamageReduction>(2);
            Empowerments.AddInfo<Defense>(2);
            Empowerments.AddInfo<EmpowermentProlongation>(2);
        }

        public override void SetBardDefaults()
        {
            Item.width = 66;
            Item.height = 66;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.holdStyle = 5;
            Item.useStyle = ItemUseStyleID.Guitar;
            Item.reuseDelay = 30;
            Item.autoReuse = true;

            Item.damage = 70;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.UseSound = SoundID.Item47;
            Item.value = Item.buyPrice(0, 10, 0, 0);
            Item.rare = ItemRarityID.Yellow;

            Item.shoot = ModContent.ProjectileType<InfrariffProjectile>();
            Item.shootSpeed = 1f;

            InspirationCost = 3;
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float aimAngle = (Main.MouseWorld - player.Center).ToRotation();
            float spread = MathHelper.ToRadians(45f);

            // Sweep left -> right
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer,
                ai0: aimAngle - spread, ai1: aimAngle + spread);

            // Sweep right -> left
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer,
                ai0: aimAngle + spread, ai1: aimAngle - spread);

            // Third purple beam: same logic, just randomized start/target angle
            // Editable angles in degrees
            float randomStartDegrees = 30f;  // ± from aim
            float maxSweepDegrees = 45f;     // maximum sweep toward mouse

            // Convert start to radians
            float randomStart = aimAngle + MathHelper.ToRadians(Main.rand.NextFloat(-randomStartDegrees, randomStartDegrees));

            // Angle directly to mouse
            float directionToMouse = (Main.MouseWorld - player.Center).ToRotation();

            // Clamp the target so it moves toward the mouse
            float deltaAngle = MathHelper.WrapAngle(directionToMouse - randomStart);

            // Limit deltaAngle to ±maxSweepDegrees, but keep its sign (toward the mouse)
            deltaAngle = MathHelper.Clamp(deltaAngle, -MathHelper.ToRadians(maxSweepDegrees), MathHelper.ToRadians(maxSweepDegrees));

            // The purple beam target
            float randomEnd = randomStart + deltaAngle;

            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<InfrariffProjectile>(),
                damage, knockback, Main.myPlayer, ai0: randomStart, ai1: randomEnd, ai2: 1f);


            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-12, 12);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(-12, 12) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = new Vector2(-12, 12) * player.Directions;

            player.itemLocation += offset;
        }

        public override void BardModifyTooltips(List<TooltipLine> tooltips)
        {
            // Lore lines
            tooltips.Add(new TooltipLine(Mod, "PBGALore1", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.NeonRipperLore1")) { OverrideColor = Color.MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "PBGALore2", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.PBGBardALore2")) { OverrideColor = Color.MediumPurple });
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MysteriousCircuitry>(15)
                .AddIngredient<DubiousPlating>(15)
                .AddIngredient<InfectedArmorPlating>(10)
                .AddIngredient<LifeAlloy>(5)
                .AddTile(TileID.MythrilAnvil)
                .AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(3, out Func<bool> condition), condition)
                .Register();
        }
    }

    [ExtendsFromMod("ThoriumMod")]
    public class InfrariffProjectile : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        // Using Providence beam texture (can swap to neutral if needed)
        public override string Texture => "CalamityMod/Projectiles/Boss/ProvidenceHolyRayNight";

        public override void SetBardDefaults()
        {
            Projectile.Size = new Vector2(48, 48);
            Projectile.timeLeft = 50;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = false; // global immunity
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 3;     // 3 ticks between hits per NPC
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.localAI[1]);
        public override void ReceiveExtraAI(BinaryReader reader) => Projectile.localAI[1] = reader.ReadSingle();

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float startAngle = Projectile.ai[0];
            float targetAngle = Projectile.ai[1];

            float lifetime = 50f;
            float progress = 1f - (Projectile.timeLeft / lifetime);
            progress = MathHelper.Clamp(progress, 0f, 1f);

            // Smoothstep easing
            float easedProgress = 0.5f - 0.5f * (float)Math.Cos(progress * Math.PI);
            float currentAngle = MathHelper.Lerp(startAngle, targetAngle, easedProgress);

            // Position & rotation
            Projectile.Center = player.Center + Vector2.UnitX.RotatedBy(currentAngle) * 8f;
            Projectile.velocity = Projectile.Center.DirectionFrom(player.Center);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Laser scan length
            float[] array = new float[3];
            Collision.LaserScan(Projectile.Center, Projectile.velocity, Projectile.scale, 800f, array);
            float avgLength = (array[0] + array[1] + array[2]) / 3f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], avgLength, 0.5f);

            // Continuous beam projectiles
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    Projectile.velocity * 8,
                    ModContent.ProjectileType<InfrariffLaser>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        Texture2D texture2D => TextureAssets.Projectile[Type].Value;
        Texture2D textureStart = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/ProvidenceHolyRayNight", AssetRequestMode.ImmediateLoad).Value;
        Texture2D texture2D2 = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/ProvidenceHolyRayMidNight", AssetRequestMode.ImmediateLoad).Value;
        Texture2D texture2D3 = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Lasers/ProvidenceHolyRayEndNight", AssetRequestMode.ImmediateLoad).Value;

        public float StartOffset = 24f; // pixels in front of the player

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.velocity == Vector2.Zero)
                return false;

            float beamLength = Projectile.localAI[1];
            float lifetime = 50f;
            float progress = 1f - (Projectile.timeLeft / lifetime);

            // Fade in/out
            float maxOpacity = 1f;
            float fadeFraction = 0.4f;

            float fadeIn = MathHelper.Clamp(progress / fadeFraction, 0f, 1f);
            float fadeOut = MathHelper.Clamp((1f - progress) / fadeFraction, 0f, 1f);
            float fade = Math.Min(fadeIn, fadeOut) * maxOpacity;

            Color desiredColor;
            if (Projectile.ai[2] == 1f)
                desiredColor = Color.DarkOrchid;           // purple beam
            else if (Projectile.ai[0] < Projectile.ai[1])
                desiredColor = new Color(255, 69, 0); // red
            else
                desiredColor = Color.Cyan;            // cyan

            Color color = desiredColor * fade;

            // Beam origin offset in front of player
            Vector2 beamOrigin = Projectile.Center + Projectile.velocity * StartOffset;

            // Draw start section
            Vector2 startPos = beamOrigin - Main.screenPosition;
            Main.spriteBatch.Draw(textureStart, startPos, null, color, Projectile.rotation,
                textureStart.Frame().Center(), Projectile.scale, SpriteEffects.None, 0f);

            // Calculate remaining length for mid section
            float midBeamLength = beamLength - (textureStart.Height + texture2D3.Height) * Projectile.scale;
            if (midBeamLength > 0f)
            {
                float startMidOffset = 10f; // extra distance between start and mid
                Vector2 center = beamOrigin + Projectile.velocity * (textureStart.Height * Projectile.scale + startMidOffset);

                float traveled = 0f;

                Rectangle frame = new Rectangle(0, 36 * (Projectile.timeLeft / 3 % 4), texture2D2.Width, 36);

                while (traveled + 1f < midBeamLength)
                {
                    Main.spriteBatch.Draw(texture2D2, center - Main.screenPosition, frame, color,
                        Projectile.rotation, new Vector2(frame.Width / 2, 0f), Projectile.scale,
                        SpriteEffects.None, 0f);

                    traveled += frame.Height * Projectile.scale;
                    center += Projectile.velocity * frame.Height * Projectile.scale;

                    frame.Y += 36;
                    if (frame.Y + frame.Height > texture2D2.Height)
                        frame.Y = 0;
                }

                // Draw end section
                Vector2 endPos = center - Main.screenPosition;
                Main.spriteBatch.Draw(texture2D3, endPos, null, color, Projectile.rotation + MathHelper.Pi,
                    texture2D3.Frame().Center() + (Vector2.UnitY * 24), Projectile.scale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class InfrariffLaser : BardProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Zenith}"; // hidden, not drawn
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetBardDefaults()
        {
            Projectile.Size = new Vector2(24, 24);
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.extraUpdates = 60;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet;

            Projectile.usesLocalNPCImmunity = false; // global immunity
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 3;     // 3 ticks between hits per NPC
        }

        // This is where the debuff gets applied
        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 180 ticks = 3 seconds
            target.AddBuff(BuffID.ShadowFlame, 180);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
