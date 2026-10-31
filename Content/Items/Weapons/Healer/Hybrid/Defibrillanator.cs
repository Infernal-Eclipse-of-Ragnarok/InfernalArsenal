using CalamityMod.Items;
using CalamityMod.Projectiles.Magic;
using InfernalEclipseWeaponsDLC.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.CustomRecipes;
using Terraria.Audio;
using CalamityMod.Items.Placeables.SunkenSea;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Hybrid
{
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class Defibrillanator : ThoriumItem
    {
        static Asset<Texture2D> inventoryTexture;

        public enum UseMode
        {
            Damage = 0,
            Healing = 1
        }

        private UseMode currentMode = UseMode.Damage;

        public override void SetStaticDefaults()
        {
            inventoryTexture = Mod.Assets.Request<Texture2D>(
                "Content/Items/Weapons/Healer/Hybrid/Defibrillanator_Inventory",
                AssetRequestMode.ImmediateLoad
            );
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(10, 10);
            Item.value = Item.sellPrice(gold: 3);

            Item.damage = 56;
            Item.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
            Item.noMelee = true;
            Item.mana = 3;
            radiantLifeCost = 2;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Orange;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.UseSound = SoundID.Item24;

            Item.shoot = ModContent.ProjectileType<DamagingLightningArc>();
            Item.shootSpeed = 8f;

            isHealer = true;
            healDisplay = true;
            healAmount = 5;
            healType = HealType.Ally;

            Item.noUseGraphic = true;

            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override bool CanUseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<DefibrillanatorPlayer>();

            radiantLifeCost = modPlayer.fullyCharged ? 0 : 3;

            return radiantLifeCost < player.statLife;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.altFunctionUse == 2)
                type = ModContent.ProjectileType<HealingLightningArc>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<DefibrillanatorPlayer>();

            if (player.altFunctionUse == 2)
                currentMode = UseMode.Healing;
            else
                currentMode = UseMode.Damage;

            // --- Charging mode (Left-click) ---
            if (!modPlayer.fullyCharged)
            {
                // Increment charge
                modPlayer.defibrillanatorCharge++;

                // Charging feedback
                SoundEngine.PlaySound(SoundID.Item93, player.Center);
                Dust.NewDust(player.Center, 10, 10, DustID.Electric, 0f, -2f, 150, Color.LightCyan, 1.3f);
                Dust.NewDust(player.Center, 10, 10, DustID.Torch, 0f, -2f, 150, Color.White, 1.3f);

                // Cap & trigger full charge
                if (modPlayer.defibrillanatorCharge >= 5)
                {
                    modPlayer.defibrillanatorCharge = 5;
                    modPlayer.fullyCharged = true;

                    for (int i = 0; i < 5; i++)
                    {
                        Dust.NewDust(player.Center, 10, 10, DustID.Electric, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, Color.White, 0.75f);
                        Dust.NewDust(player.Center, 10, 10, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 150, Color.White, 0.75f);
                    }
                    SoundEngine.PlaySound(SoundID.Item93, player.Center);
                }

                // No projectile fired during charge
                return false;
            }
            else
            {
                if (player.altFunctionUse != 2)
                {
                    radiantLifeCost = 0;
                }

                int shots = 5;
                modPlayer.fullyCharged = false;
                modPlayer.defibrillanatorCharge = 0;

                SoundEngine.PlaySound(SoundID.Item122, player.Center);

                for (int i = 0; i < shots; i++)
                {
                    Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
                    Vector2 shootPos = position + perturbedSpeed;
                    Projectile proj = Projectile.NewProjectileDirect(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
                    proj.DamageType = ThoriumDamageBase<HealerDamage>.Instance;
                }
            }

            return false; // handled manually
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MysteriousCircuitry>(5)
                .AddIngredient<DubiousPlating>(4)
                .AddIngredient<AerialiteBar>(3)
                .AddIngredient<SeaPrism>(6)
                .AddTile(TileID.Anvils)
                .AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(1, out Func<bool> condition), condition)
                .Register();
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            DrawTexture(spriteBatch, position, drawColor, scale);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            DrawTexture(spriteBatch, Item.position - Main.screenPosition + Item.Size * 0.5f, lightColor, scale);
            return false;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<DefibrillanatorPlayer>();

            // Pass frame correctly
            DefibrillanatorDrawHelper.DrawChargeBarInInventory(spriteBatch, position, frame, modPlayer.defibrillanatorCharge, scale);
        }

        private void DrawTexture(SpriteBatch spriteBatch, Vector2 position, Color color, float scale)
        {
            Texture2D tex = currentMode switch
            {
                UseMode.Damage =>
                    ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Hybrid/Defibrillanator_Inventory").Value,
                UseMode.Healing =>
                    ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Hybrid/Defibrillanator_Inventory_Healing").Value,
                _ =>
                    ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Hybrid/Defibrillanator_Inventory").Value,
            };

            Vector2 origin = tex.Size() * 0.5f;
            spriteBatch.Draw(tex, position, null, color, 0f, origin, ((scale / 3) * 2), SpriteEffects.None, 0f);
        }
    }

    public static class DefibrillanatorDrawHelper
    {
        public static void DrawChargeBarInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, float charge, float scale)
        {
            if (charge <= 0f) return;

            Texture2D barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack", AssetRequestMode.ImmediateLoad).Value;
            Texture2D barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront", AssetRequestMode.ImmediateLoad).Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;

            // Position below the item slot
            float yOffset = -8f;
            Vector2 drawPos = position + Vector2.UnitY * scale * (frame.Height - yOffset);

            // Crop the foreground based on charge
            Rectangle frameCrop = new Rectangle(0, 0, (int)(charge / 5f * barFG.Width), barFG.Height);

            // Smooth lerp color (you can change to any color you like)
            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) / 2f);
            Color color = Color.Lerp(new Color(135, 206, 250), Color.Gold, t);

            float barScale = 0.75f;

            // Draw background
            spriteBatch.Draw(barBG, drawPos, null, color, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
            // Draw foreground
            spriteBatch.Draw(barFG, drawPos, frameCrop, color * 0.8f, 0f, barOrigin, scale * barScale, SpriteEffects.None, 0f);
        }
    }


    // --- Projectiles ---
    [ExtendsFromMod("CalamityMod")]
    public class HealingLightningArc : LightningArc
    {
        public HashSet<Player> healedAlready = new();
        public int prevX;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.damage = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 16;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Torch,
                    Projectile.velocity * -0.25f,
                    150,
                    Color.Pink,
                    1.2f
                ).noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1.0f, 0.85f, 0.4f);

            if (Projectile.localAI[0] == 0f)
            {
                AdjustMagnitude(ref Projectile.velocity);
                Projectile.localAI[0] = 1f;
            }

            Player target = null;
            float closestDist = 200f;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && p.whoAmI != owner.whoAmI)
                {
                    if (owner.team != 0 && owner.team == p.team)
                    {
                        float dist = Vector2.Distance(Projectile.Center, p.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = p;
                        }
                    }
                }
            }

            Vector2 arcVec;
            float jitterScale = 10;

            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();
                toTarget.Normalize();
                arcVec = toTarget * dist * 0.15f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * jitterScale;
            }
            else
            {
                arcVec = Projectile.velocity * 5f + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6)) * 5f;
            }

            Vector2 start = Projectile.Center;
            for (int j = 0; j < 20; j++)
            {
                int dust = Dust.NewDust(start, 0, 0, DustID.Torch, 0f, 0f, 150, Color.Pink, 1.2f);
                Main.dust[dust].noGravity = true;
                start += arcVec / 20f;
            }

            Projectile.position = start;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && p.whoAmI != owner.whoAmI)
                {
                    if (owner.team != 0 && owner.team == p.team)
                    {
                        if (Projectile.Hitbox.Intersects(p.Hitbox))
                        {
                            HealTeammateThorium(owner, p, 5);
                            Projectile.Kill();
                            break;
                        }
                    }
                }
            }
        }

        private void HealTeammateThorium(Player healer, Player target, int baseHeal)
        {
            if (healer.whoAmI != Main.myPlayer) return;
            if (healer == target) return;
            if (healer.team == 0 || healer.team != target.team) return;

            if (baseHeal <= 0 && healer.GetModPlayer<ThoriumPlayer>().healBonus <= 0) return;

            HealerHelper.HealPlayer(healer, target, baseHeal, 60, true);
        }

        private void AdjustMagnitude(ref Vector2 vector)
        {
            float length = vector.Length();
            if (length > 6f) vector *= 6f / length;
        }
    }

    [ExtendsFromMod("CalamityMod")]
    public class DamagingLightningArc : LightningArc
    {
        public HashSet<Player> healedAlready = new();
        public int prevX;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 16;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (Main.rand.NextBool(3))
            {
                Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Electric,
                    Projectile.velocity * -0.25f,
                    150,
                    Color.Cyan,
                    1.2f
                ).noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 1.2f);

            if (Projectile.localAI[0] == 0f)
            {
                AdjustMagnitude(ref Projectile.velocity);
                Projectile.localAI[0] = 1f;
            }

            NPC target = null;
            float closestDist = 200f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(this))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
            }

            Vector2 arcVec;
            float jitterScale = 10f;
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();
                toTarget.Normalize();
                arcVec = toTarget * dist * 0.2f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * jitterScale;
            }
            else
            {
                arcVec = Projectile.velocity * 5f + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6)) * 5f;
            }

            Vector2 start = Projectile.Center;

            for (int j = 0; j < 20; j++) start += arcVec / 20f;
            for (int j = 0; j < 20; j++)
            {
                int dust = Dust.NewDust(start, 0, 0, DustID.Electric, 0f, 0f, 0, Color.LightBlue, 0.75f);
                Main.dust[dust].noGravity = true;
            }

            Projectile.position = start;
        }

        private void AdjustMagnitude(ref Vector2 vector)
        {
            float length = vector.Length();
            if (length > 6f) vector *= 6f / length;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 180);
        }
    }

    // --- Player Data ---
    public class DefibrillanatorPlayer : ModPlayer
    {
        public int defibrillanatorCharge;
        public bool fullyCharged;

        public override void PostUpdate()
        {
            if (fullyCharged)
            {
                Lighting.AddLight(Player.Center, 0.2f, 0.4f, 1.0f);
                if (Main.rand.NextBool(5))
                    Dust.NewDust(Player.Center, 10, 10, DustID.Electric, 0, 0, 150, Color.Cyan, 0.75f);
            }
        }
    }

    // --- Charge bar (player) ---
    public class DefibrillanatorChargeLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<DefibrillanatorPlayer>();

            if (player.HeldItem.ModItem is not Defibrillanator) return;
            if (modPlayer.defibrillanatorCharge <= 0) return;

            Texture2D barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack", AssetRequestMode.ImmediateLoad).Value;
            Texture2D barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront", AssetRequestMode.ImmediateLoad).Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;
            float barScale = 0.95f;
            Vector2 drawPos = player.Top + Vector2.UnitY * -16f - Main.screenPosition;

            Rectangle frameCrop = new Rectangle(0, 0, (int)(modPlayer.defibrillanatorCharge / 5f * barFG.Width), barFG.Height);

            float t = (float)((Math.Sin(Main.GlobalTimeWrappedHourly * 2f) + 1f) / 2f);
            Color color = Color.Lerp(new Color(135, 206, 250), Color.Gold, t);

            Main.spriteBatch.Draw(barBG, drawPos, null, color, 0f, barOrigin, barScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(barFG, drawPos, frameCrop, color * 0.8f, 0f, barOrigin, barScale, SpriteEffects.None, 0f);
        }
    }
}
