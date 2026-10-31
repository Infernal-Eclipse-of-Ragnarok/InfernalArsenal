using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using InfernalEclipseWeaponsDLC.Core.NewFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.SentryPro
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class HollowbrookMonolithPro : ModProjectile
    {
        public int AuraRadius;
        public static Color AuraColor => new(145, 175, 75, 50);
        public float fade = 1.2f;
        public virtual bool DisplayRing => true;
        private static bool isCleaningUp;

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 36;

            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.sentry = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            InfernalWeaponsPlayer modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            if (modPlayer.hollowbrookLocation.HasValue)
                Projectile.Center = modPlayer.hollowbrookLocation.Value;

            AuraRadius = 200 + 50 * modPlayer.hollowbrookTier;

            // Apply gravity.
            Projectile.velocity.Y += 1f;

            // Fade the aura.
            if (fade > 0.15f)
                fade = Math.Max(0.15f, fade - 0.01f);

            ApplyAuraBuff();

            CheckForCritCannon();

            // Save the monolith's current position.
            modPlayer.hollowbrookLocation = Projectile.Center;
        }

        private void ApplyAuraBuff()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float radiusSquared = AuraRadius * AuraRadius;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.townNPC)
                    continue;

                if (Vector2.DistanceSquared(npc.Center, Projectile.Center) > radiusSquared)
                    continue;

                npc.AddBuff(ModContent.BuffType<HollowbrookTag>(), 2);
            }
        }

        private void CheckForCritCannon()
        {
            Player owner = Main.player[Projectile.owner];
            InfernalWeaponsPlayer modPlayer = owner.GetModPlayer<InfernalWeaponsPlayer>();

            if (modPlayer.hollowbrookTier < 3)
                return;

            // Handle the cannon on the server.
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            bool cannonReady = false;

            // Allow any player's critical hits to trigger the cannon.
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.dead)
                    continue;

                InfernalWeaponsPlayer otherModPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

                if (otherModPlayer.hollowbrookCrits >= 10)
                {
                    cannonReady = true;
                    otherModPlayer.hollowbrookCrits = 0;
                    break;
                }
            }

            if (!cannonReady)
                return;

            float radiusSquared = AuraRadius * AuraRadius;

            Projectile cannon = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Top, -Vector2.UnitY * 10f, ProjectileID.CannonballFriendly, 300, 0, Projectile.owner);

            cannon.DamageType = DamageClass.Summon;

            // Grant random buffs to players within the aura.
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.dead)
                    continue;

                if (Vector2.DistanceSquared(player.Center, Projectile.Center) > radiusSquared)
                    continue;

                InfernalWeaponsPlayer.GrantRandomBuff(player);
            }

            // Inflict random debuffs on enemies within the aura.
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.townNPC)
                    continue;

                if (Vector2.DistanceSquared(npc.Center, Projectile.Center) > radiusSquared)
                    continue;

                InfernalWeaponsPlayer.InflictRandomDebuff(npc);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            InfernalWeaponsPlayer modPlayer = player.GetModPlayer<InfernalWeaponsPlayer>();

            int tier = modPlayer.hollowbrookTier;

            // Select the texture based on the current tier.
            string texturePath = tier switch
            {
                2 => Texture + "T2",
                3 => Texture + "T3",
                _ => Texture
            };

            Texture2D projectileTexture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;

            Vector2 drawPosition = Projectile.Bottom - Main.screenPosition;
            Vector2 origin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height);

            // Draw the aura behind the projectile.
            if (DisplayRing)
            {
                Texture2D auraTexture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/Circle408", AssetRequestMode.ImmediateLoad).Value;

                Vector2 auraPosition = Projectile.Center - Main.screenPosition;

                Vector2 auraOrigin = auraTexture.Size() * 0.5f;

                int radius = 200 + 50 * tier;
                float ringScaling = radius / 204f;

                Main.EntitySpriteDraw(auraTexture, auraPosition, null,  AuraColor * fade, 0f, auraOrigin, ringScaling, SpriteEffects.None, 0f);
            }

            // Draw the monolith sprite based on its tier.
            Main.EntitySpriteDraw(projectileTexture, drawPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y > 0f)
                Projectile.velocity.Y = 0f;

            return false;
        }

        //Niche fix for Entropy's sentry remover allowing for upgrades to be "removed" while the original remains still upgraded, allowing for free sentry slots
        public static void KillAllHollowbrookProjectiles(Projectile dyingProjectile)
        {
            if (isCleaningUp)
                return;

            isCleaningUp = true;

            try
            {
                int mainType = ModContent.ProjectileType<HollowbrookMonolithPro>();
                int upgradeType = ModContent.ProjectileType<HollowbrookMonolithUpgradePro>();

                foreach (Projectile projectile in Main.projectile)
                {
                    if (!projectile.active ||
                        projectile.owner != dyingProjectile.owner)
                        continue;

                    if (projectile.type == mainType || projectile.type == upgradeType)
                    {
                        projectile.Kill();
                    }
                }
            }
            finally
            {
                isCleaningUp = false;
            }
        }

        public override void OnKill(int timeLeft)
        {
            KillAllHollowbrookProjectiles(Projectile);
        }
    }

    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class HollowbrookMonolithUpgradePro : ModProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 36;

            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Summon;

            Projectile.sentry = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            int hollowbrookType = ModContent.ProjectileType<HollowbrookMonolithPro>();

            Projectile mainSentry = null;

            foreach (Projectile projectile in Main.projectile)
            {
                if (projectile.active &&
                    projectile.owner == Projectile.owner &&
                    projectile.type == hollowbrookType)
                {
                    mainSentry = projectile;
                    break;
                }
            }

            if (mainSentry == null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = mainSentry.Center;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            HollowbrookMonolithPro.KillAllHollowbrookProjectiles(Projectile);
        }
    }
}