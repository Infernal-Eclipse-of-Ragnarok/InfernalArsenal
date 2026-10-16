using InfernalEclipseWeaponsDLC.Content.Projectiles.RangedPro;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged
{
    public class PrismaGorgon : ModItem
    {
        private static int AmmoSavedPercent = 30;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        
        public override void SetDefaults()
        {
            Item.width = 98;
            Item.height = 38;
            Item.damage = 38;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.75f;
            Item.crit = 30;
            //Item.UseSound = SoundID.Item11;
            Item.value = Item.buyPrice(0, 60);
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = false; // Because holdout
            Item.shoot = ModContent.ProjectileType<PrismaGorgonHoldout>();
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 2f;
            Item.reuseDelay = 0; // Because holdout
            Item.channel = true; // Because holdout
            Item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool RangedPrefix() => true; //Can't scale with attack speed, but should still be able to recieve Unreal
        public override void HoldItem(Player player) => player.GetModPlayer<InfernalWeaponsPlayer>().mouseRotationListener = true;
        public override bool CanConsumeAmmo(Item ammo, Player player) => player.ownedProjectileCounts[Item.shoot] > 0 && Main.rand.Next(100) >= AmmoSavedPercent;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, position, velocity, Item.shoot, damage, knockback, player.whoAmI);
            holdout.velocity = (player.GetModPlayer<InfernalWeaponsPlayer>().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);

            return false;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            bool charged = Main.LocalPlayer.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored >= 1;

            if (!charged)
                return true;

            Texture2D texture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Ranged/PrismaGorgonIncarnon").Value;
            spriteBatch.Draw(texture, position, null, drawColor,0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            if (modPlayer.gorgonIncarnonShotsStored <= 0) return;

            Texture2D barBG = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/GenericBarBack", AssetRequestMode.ImmediateLoad).Value;
            Texture2D barFG = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/GenericBarFront", AssetRequestMode.ImmediateLoad).Value;

            float chargeRatio = MathHelper.Clamp(modPlayer.gorgonIncarnonShotsStored / 20f, 0f, 1f);

            float barScale = scale * 2.3f;
            float rotation = MathHelper.PiOver2;

            // Move the bar to the right of the weapon.
            Vector2 drawPos = position + new Vector2(70f * scale, 0f);

            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) / 2f);
            Color color = Color.Lerp(Color.CornflowerBlue, Color.LightBlue, t);

            // Draw the background vertically.
            Vector2 backgroundOrigin = barBG.Size() * 0.5f;

            spriteBatch.Draw(barBG, drawPos, null, color, rotation, backgroundOrigin, barScale, SpriteEffects.None, 0f);

            // Crop the foreground horizontally before rotating it.
            int filledWidth = (int)(chargeRatio * barFG.Width);

            if (filledWidth > 0)
            {
                Rectangle frameCrop = new(0, 0, filledWidth, barFG.Height);

                // Keep the left edge of the original texture anchored.
                // After rotation, this becomes the bottom of the vertical bar.
                Vector2 foregroundOrigin = new Vector2(filledWidth - barFG.Width * 0.5f, barFG.Height * 0.5f);

                spriteBatch.Draw(barFG, drawPos, frameCrop, color * 0.8f, rotation, foregroundOrigin, barScale, SpriteEffects.None, 0f);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.IsKeyDown(Keys.LeftShift))
            {
                TooltipLine line5 = new(Mod, "DedicatedItem", $"{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.DedTo", Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Dedicated.Heatstroke"))}\n{Language.GetTextValue("Mods.InfernalEclipseWeaponsDLC.ItemTooltip.Donor")}");
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

        public override void AddRecipes()
        {
            if (ModLoader.TryGetMod("Fargowiltas", out Mod mutant))
            {
                CreateRecipe()
                    .AddIngredient(mutant.Find<ModItem>("TravellingMerchant"))
                    .AddIngredient(ItemID.PlatinumCoin)
                    .AddIngredient(ItemID.ClockworkAssaultRifle)
                    .AddTile(TileID.TinkerersWorkbench)
                    .AddCondition(Condition.DownedPumpking)
                    .DisableDecraft()
                    .Register();
            }
        }
    }

    public class PrismaGorgonHoldout : ModProjectile, ILocalizedModType
    {
        public static int AssociatedItemID => ModContent.ItemType<PrismaGorgon>();
        public Vector2 GunTipPosition => Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * Projectile.width * 0.5f;
        public static float WeaponTurnSpeed => 0.2f;
        public static float RecoilResolveSpeed => 0.3f;

        public static float MaxOffsetLengthFromArm => 10f;
        public float OffsetXUpwards { get; }
        public float OffsetXDownwards { get; }
        public static float BaseOffsetY => -3f;
        public float OffsetYUpwards { get; }
        public virtual float OffsetYDownwards { get; }

        public Player Owner { get; private set; }
        public Item HeldItem { get; private set; }
        public bool KeepRefreshingLifetime { get; set; } = true;
        public bool SetUsage { get; set; } = true;
        public float OffsetLengthFromArm { get; set; }
        public float ExtraFrontArmRotation { get; set; }
        public float ExtraBackArmRotation { get; set; }
        public Player.CompositeArmStretchAmount FrontArmStretch { get; set; } = Player.CompositeArmStretchAmount.Full;
        public Player.CompositeArmStretchAmount BackArmStretch { get; set; } = Player.CompositeArmStretchAmount.Full;

        private static Type AssociatedItemType => ItemLoader.GetItem(AssociatedItemID).GetType();
        private static Asset<Texture2D> ItemTexture => TextureAssets.Item[AssociatedItemID];
        public static readonly SoundStyle TransformationSound = new($"{nameof(InfernalEclipseWeaponsDLC)}/Assets/Sounds/IncarnonWeaponVoidModeStart")
        {
            Volume = 0.5f
        };

        public int Time = 0;
        public int shotCounter = 0;
        public int framesBetweenShots = 0;

        public override LocalizedText DisplayName
        {
            get
            {
                if (AssociatedItemID < ItemID.Count)
                {
                    return Language.GetText("ItemName." + ItemID.Search.GetName(AssociatedItemID));
                }

                return ItemLoader.GetItem(AssociatedItemID).GetLocalization("DisplayName");
            }
        }

        public override string Texture => (AssociatedItemType.Namespace + "." + AssociatedItemType.Name).Replace('.', '/');

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = ItemTexture is null ? 1 : ItemTexture.Width();
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }

        public override void OnSpawn(IEntitySource source) => OffsetLengthFromArm = MaxOffsetLengthFromArm;
        public override bool ShouldUpdatePosition() => false;
        public override bool? CanDamage() => false;

        public override void AI()
        {
            Owner ??= Main.player[Projectile.owner];
            HeldItem ??= Owner.HeldItem;

            KillHoldoutLogic();
            ManageHoldout();
            HoldoutAI();
            Time++;
        }

        public static bool CantUseHoldout(Player player)
        {
            bool isChannel = player.channel;

            if (player != null && player.active && !player.dead && !(!isChannel) && !player.CCed)
            {
                return player.noItems;
            }

            return true;
        }

        public void KillHoldoutLogic()
        {
            if (CantUseHoldout(Owner) || HeldItem.type != AssociatedItemID)
            {
                shotCounter = 0;
                Time = 0;
                Projectile.Kill();
            }
        }

        public void ManageHoldout()
        {
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            float holdoutDirection = Projectile.velocity.ToRotation();

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 ownerToMouse = Owner.GetModPlayer<InfernalWeaponsPlayer>().mouseWorld - armPosition;

                float proximityLookingUpwards = Vector2.Dot(ownerToMouse.SafeNormalize(Vector2.Zero), -Vector2.UnitY * Owner.gravDir);

                int direction = MathF.Sign(ownerToMouse.X);

                Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * OffsetLengthFromArm;
                Vector2 armOffset = new Vector2(Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetXUpwards : OffsetXDownwards) * direction, BaseOffsetY * Owner.gravDir + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetYUpwards : OffsetYDownwards) * Owner.gravDir);
                Projectile.Center = armPosition + lengthOffset + armOffset;
                Projectile.velocity = holdoutDirection.AngleTowards(ownerToMouse.ToRotation(), WeaponTurnSpeed).ToRotationVector2();
                Projectile.rotation = holdoutDirection;

                Projectile.spriteDirection = direction;
                Owner.ChangeDir(direction);
            }
            else
            {
                Vector2 lengthOffset = Projectile.rotation.ToRotationVector2() * OffsetLengthFromArm;
                float proximityLookingUpwards = Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.Zero), -Vector2.UnitY * Owner.gravDir);
                int direction = Projectile.spriteDirection;

                Vector2 armOffset = new Vector2(Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetXUpwards : OffsetXDownwards) * direction, BaseOffsetY * Owner.gravDir + Utils.Remap(MathF.Abs(proximityLookingUpwards), 0f, 1f, 0f, proximityLookingUpwards > 0f ? OffsetYUpwards : OffsetYDownwards) * Owner.gravDir);
                Projectile.Center = armPosition + lengthOffset + armOffset;
                Projectile.velocity = Projectile.rotation.ToRotationVector2();

                Owner.ChangeDir(direction);
            }

            int currentDirection = Projectile.spriteDirection;

            Owner.heldProj = Projectile.whoAmI;
            if (SetUsage)
                Owner.itemTime = Owner.itemAnimation = 2;
            Owner.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

            float armRotation = (Projectile.rotation - MathHelper.PiOver2) * Owner.gravDir + (Owner.gravDir == -1 ? MathHelper.Pi : 0f);
            Owner.SetCompositeArmFront(true, FrontArmStretch, armRotation + ExtraFrontArmRotation * currentDirection);
            Owner.SetCompositeArmBack(true, BackArmStretch, armRotation + ExtraBackArmRotation * currentDirection);

            if (KeepRefreshingLifetime)
                Projectile.timeLeft = 2;

            if (OffsetLengthFromArm != MaxOffsetLengthFromArm)
                OffsetLengthFromArm = MathHelper.Lerp(OffsetLengthFromArm, MaxOffsetLengthFromArm, RecoilResolveSpeed);

            if (Projectile.owner == Main.myPlayer)
                ForceNetUpdate(Projectile);
        }

        private static void ForceNetUpdate(Projectile proj, bool ignoreCurrentNetSpam = true)
        {
            proj.netUpdate = true;
            if (proj.netSpam >= 10 || ignoreCurrentNetSpam)
            {
                proj.netSpam = 0;
            }
        }

        public void HoldoutAI()
        {
            ref bool incarnonForme = ref Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnon;

            if (Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonShotsHit == 20)
            {
                if (!incarnonForme)
                {
                    SoundEngine.PlaySound(TransformationSound, Owner.Center);
                    Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored++;
                    DoLargeDustExplosion();

                    incarnonForme = true;
                }
                else if (Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored < 20)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Owner.Center);
                    Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored++;
                    DoSmallDustExplosion();
                }

                Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonShotsHit = 0;
            }

            if (framesBetweenShots == 0)
            {
                if (incarnonForme && Main.mouseRight && Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored >= 1)
                {
                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 24f;

                    SoundStyle fire = SoundID.Item14;
                    SoundEngine.PlaySound(fire with { Volume = 0.3f, Pitch = 0.4f, PitchVariance = 0.1f, MaxInstances = -1 }, Projectile.Center);

                    framesBetweenShots = 20;
                    OffsetLengthFromArm -= 5f;

                    Owner.PickAmmo(Owner.HeldItem, out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _);
                    DoLargeDustExplosion();

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity, ModContent.ProjectileType<PrismaGorgonLaser>(), Projectile.damage * 8, Projectile.knockBack * 2f, Projectile.owner);
                    }

                    shotCounter = 0;

                    Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored--;
                    if (Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored <= 0)
                        incarnonForme = false;
                }
                else
                {
                    if (Time < 3)
                        return;

                    Vector2 shootVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 19;

                    SoundStyle fire = SoundID.Item11;
                    SoundEngine.PlaySound(fire with { Volume = 0.3f, Pitch = 0.4f, PitchVariance = 0.1f, MaxInstances = -1 }, Projectile.Center);

                    //How many frames between firing projectiles, and how far the gun moves backward to give the effect of recoil. Change this number to edit fire rate
                    framesBetweenShots = 20 - (int)(Math.Min(shotCounter, 7) * 2.3);
                    OffsetLengthFromArm -= 3f;

                    Owner.PickAmmo(Owner.HeldItem, out int bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out _);
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile bullet = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), GunTipPosition, shootVelocity.RotatedByRandom(MathHelper.ToRadians(1.5f)), bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
                        bullet.GetGlobalProjectile<GorgonGlobalProjectile>().gorgonBullet = true;
                        bullet.GetGlobalProjectile<GorgonGlobalProjectile>().bulletOwner = Owner;
                    }
                    shotCounter++;
                }
            }

            if (framesBetweenShots > 0)
                framesBetweenShots--;
        }

        private void DoLargeDustExplosion()
        {
            Vector2 explosionPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() * 30f;

            for (int i = 0; i < 45; i++)
            {

                Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 9f);

                int dustType = Main.rand.Next(3) switch
                {
                    0 => DustID.Ice,
                    1 => DustID.IceTorch,
                    _ => DustID.BlueTorch
                };

                Dust dust = Dust.NewDustPerfect(explosionPosition, dustType, velocity, 100, Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(1.2f, 2f));
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // Bright icy particles near the center
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(explosionPosition, DustID.IceTorch, Main.rand.NextVector2Circular(3f, 3f), 0, Color.White, Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }
        }

        private void DoSmallDustExplosion()
        {
            Vector2 explosionPosition = Projectile.Center + Projectile.rotation.ToRotationVector2() * 30f;

            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 4f);

                int dustType = Main.rand.Next(3) switch
                {
                    0 => DustID.Ice,
                    1 => DustID.IceTorch,
                    _ => DustID.BlueTorch
                };

                Dust dust = Dust.NewDustPerfect(explosionPosition, dustType, velocity, 100, Color.Lerp(Color.Cyan, Color.LightBlue, Main.rand.NextFloat()), Main.rand.NextFloat(0.7f, 1.2f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }

            // Small bright icy particles near the center
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(explosionPosition, DustID.IceTorch, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Color.White, Main.rand.NextFloat(0.8f, 1.4f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Owner.GetModPlayer<InfernalWeaponsPlayer>().gorgonIncarnonShotsStored >= 1 ? ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Ranged/PrismaGorgonIncarnon").Value : TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 rotationPoint = texture.Size() * 0.5f;
            float drawRotation = Projectile.rotation;

            SpriteEffects flipSprite = SpriteEffects.None;

            if (Owner.gravDir == 1f)
            {
                if (Projectile.spriteDirection == -1)
                    flipSprite = SpriteEffects.FlipVertically;
            }
            else
            {
                rotationPoint.Y = texture.Height - rotationPoint.Y;

                if (Projectile.spriteDirection == 1)
                    flipSprite = SpriteEffects.FlipVertically;
            }

            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), drawRotation, rotationPoint, Projectile.scale, flipSprite, 0);
            return false;
        }

        public sealed override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.rotation);
            writer.Write(KeepRefreshingLifetime);
            writer.Write(OffsetLengthFromArm);
            writer.Write7BitEncodedInt(Projectile.spriteDirection);
        }

        public sealed override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.rotation = reader.ReadSingle();
            KeepRefreshingLifetime = reader.ReadBoolean();
            OffsetLengthFromArm = reader.ReadSingle();
            Projectile.spriteDirection = reader.Read7BitEncodedInt();
        }
    }

    public class GorgonGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool gorgonBullet = false;
        public bool alreadyHitTarget = false;
        public Player bulletOwner;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (gorgonBullet & !alreadyHitTarget)
            {
                if (bulletOwner.GetModPlayer<InfernalWeaponsPlayer>().gorgonShotsHit < 20)
                    bulletOwner.GetModPlayer<InfernalWeaponsPlayer>().gorgonShotsHit++;
            }

            alreadyHitTarget = true;
        }
    }

    public class GorgonStockpileLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            if (player.HeldItem.ModItem is not PrismaGorgon) return;
            if (modPlayer.gorgonIncarnonShotsStored <= 0) return;

            Texture2D barBG = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/GenericBarBack", AssetRequestMode.ImmediateLoad).Value;
            Texture2D barFG = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/GenericBarFront", AssetRequestMode.ImmediateLoad).Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;
            float barScale = 0.95f;
            Vector2 drawPos = player.Top + Vector2.UnitY * -16f - Main.screenPosition;

            Rectangle frameCrop = new Rectangle(0, 0, (int)(modPlayer.gorgonIncarnonShotsStored / 20f * barFG.Width), barFG.Height);

            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) / 2f);
            Color color = Color.Lerp(Color.CornflowerBlue, Color.LightBlue, t);

            Main.spriteBatch.Draw(barBG, drawPos, null, color, 0f, barOrigin, barScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(barFG, drawPos, frameCrop, color * 0.8f, 0f, barOrigin, barScale, SpriteEffects.None, 0f);
        }
    }
}