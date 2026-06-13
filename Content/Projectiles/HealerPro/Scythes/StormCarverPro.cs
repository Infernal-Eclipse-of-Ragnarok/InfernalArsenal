using System;
using ThoriumMod.Projectiles.Scythe;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.Audio;
using ReLogic.Content;
using System.IO;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.HealerPro.Scythes
{
    public class StormCarverPro : ScythePro
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Items/Weapons/Healer/Melee/Scythes/StormCarver";

        public override void SafeSetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 70;
            Projectile.idStaticNPCHitCooldown = 8;
            Projectile.alpha = 255;
            Projectile.manualDirectionChange = true;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.scale = player.GetAdjustedItemScale(player.HeldItem);

            if (Projectile.ai[1] <= 0f || player.dead)
            {
                Projectile.Kill();
                return false;
            }

            Projectile.timeLeft = (int)Projectile.ai[1];
            player.itemTime = (int)Projectile.ai[1];
            player.itemAnimation = (int)Projectile.ai[1];
            player.heldProj = Projectile.whoAmI;
            player.compositeFrontArm.enabled = true;

            if (Projectile.velocity.X != 0f)
                player.ChangeDir(Projectile.velocity.X > 0f ? 1 : -1);

            // Calculate rotation / swing
            float swingTime = 0f;
            float swing = 0f;
            float armSwing = 0f;

            if (Projectile.direction == -1)
            {
                if (Projectile.ai[1] / Projectile.ai[0] > 0.75f)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                    }
                    swingTime = ((Projectile.ai[1] / Projectile.ai[0] - 0.75f) * 4f);
                    swing = Utils.ToRotation(Projectile.velocity) + MathHelper.ToRadians(MathHelper.SmoothStep(135f, 80f, swingTime) * player.direction);
                    armSwing = Utils.ToRotation(Projectile.velocity) + MathHelper.Lerp(MathHelper.PiOver2, MathHelper.PiOver4, swingTime) * player.direction;
                }
                else
                {
                    swingTime = Projectile.ai[1] / (Projectile.ai[0] * 0.75f);
                    for (int i = 0; i < 4; i++)
                        swingTime = MathHelper.SmoothStep(0f, 1f, swingTime);
                    swing = MathHelper.Lerp(MathHelper.ToRadians(100f) * -player.direction, MathHelper.ToRadians(135f) * player.direction, swingTime) + Utils.ToRotation(Projectile.velocity);
                    armSwing = MathHelper.Lerp(MathHelper.PiOver2 * -player.direction, MathHelper.PiOver2 * player.direction, swingTime) + Utils.ToRotation(Projectile.velocity);
                }
                Projectile.spriteDirection = -player.direction;
            }
            else
            {
                if (Projectile.ai[1] / Projectile.ai[0] > 0.75f)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                    }
                    swingTime = ((Projectile.ai[1] / Projectile.ai[0] - 0.75f) * 4f);
                    swing = Utils.ToRotation(Projectile.velocity) - MathHelper.ToRadians(MathHelper.SmoothStep(135f, 80f, swingTime) * player.direction);
                    armSwing = Utils.ToRotation(Projectile.velocity) - MathHelper.Lerp(MathHelper.PiOver2, MathHelper.PiOver4, swingTime) * player.direction;
                }
                else
                {
                    swingTime = Projectile.ai[1] / (Projectile.ai[0] * 0.75f);
                    for (int i = 0; i < 4; i++)
                        swingTime = MathHelper.SmoothStep(0f, 1f, swingTime);
                    swing = MathHelper.Lerp(MathHelper.ToRadians(100f) * player.direction, MathHelper.ToRadians(135f) * -player.direction, swingTime) + Utils.ToRotation(Projectile.velocity);
                    armSwing = MathHelper.Lerp(MathHelper.PiOver2 * player.direction, MathHelper.PiOver2 * -player.direction, swingTime) + Utils.ToRotation(Projectile.velocity);
                }
                Projectile.spriteDirection = player.direction;
            }

            // Handle localAI[2] for any swing effects
            if (Projectile.localAI[2] == 0f && Projectile.ai[1] <= Projectile.ai[0] * 0.5f)
            {
                SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, player.position);
                Projectile.localAI[2] = 1f;
            }

            if (Projectile.alpha > 0)
                Projectile.alpha -= 51;

            player.compositeFrontArm.rotation = armSwing - MathHelper.PiOver2 - (player.gravDir - 1f) * MathHelper.PiOver2;
            Projectile.Center = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
            Projectile.rotation = swing;

            if (Projectile.ai[1] > 0f)
                Projectile.ai[1] -= 1f;

            // === Tile Collision check for ThunderGust spawn ===
            // Only spawn once and only during active hitbox
            if (Projectile.localAI[0] == 0f &&
                Projectile.ai[1] < Projectile.ai[0] * 0.6f &&
                Projectile.ai[1] > Projectile.ai[0] * 0.1f)
            {
                Vector2 tipOffset = Utils.ToRotationVector2(Projectile.rotation) * Projectile.height * Projectile.scale; // full length of scythe
                Vector2 bladeStart = Projectile.Center + tipOffset * 0.3f; // squish start up the blade (30% from base)
                Vector2 bladeEnd = Projectile.Center + tipOffset; // tip of blade

                int steps = (int)(bladeEnd - bladeStart).Length() / 4;

                for (int i = 0; i <= steps; i++)
                {
                    Vector2 checkPos = bladeStart + (bladeEnd - bladeStart) * (i / (float)steps);
                    int tileX = (int)(checkPos.X / 16);
                    int tileY = (int)(checkPos.Y / 16);

                    if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
                    {
                        Tile tile = Main.tile[tileX, tileY];
                        if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            // Spawn ThunderGust horizontally based on player direction at collision tip
                            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
                            {
                                int projType = thorium.Find<ModProjectile>("ThunderGust")?.Type ?? 0;
                                if (projType > 0)
                                {
                                    Projectile newProj = Main.projectile[Projectile.NewProjectile(
                                        Projectile.GetSource_FromThis(),
                                        checkPos + new Vector2(0, -20), // optional vertical adjustment
                                        new Vector2(5f * player.direction, 0f), // horizontal based on facing
                                        projType,
                                        Projectile.damage,
                                        Projectile.knockBack,
                                        player.whoAmI
                                    )];

                                    if (newProj != null && newProj.active)
                                    {
                                        newProj.friendly = true;
                                        newProj.hostile = false;
                                        newProj.penetrate = 3;
                                        newProj.DamageType = Projectile.DamageType;

                                        // Set flag for GlobalProjectile
                                        if (newProj.GetGlobalProjectile<ThoriumThunderGustGlobal>() is ThoriumThunderGustGlobal globalProj)
                                        {
                                            globalProj.fromScythe = true;
                                        }

                                        // === Dust burst for flair ===
                                        for (int d = 0; d < 5; d++) // 10 particles
                                        {
                                            Vector2 velocity = new Vector2(5f * player.direction, -2f) + Main.rand.NextVector2Circular(1f, 1f);
                                            Dust dust = Dust.NewDustPerfect(
                                                checkPos + new Vector2(0, 20),
                                                DustID.Smoke,
                                                velocity,
                                                100,
                                                Color.LightGray,
                                                1.0f
                                            );
                                            dust.noGravity = false;
                                            dust.fadeIn = 0.5f;
                                        }
                                    }
                                }
                            }

                            Projectile.localAI[0] = 1f; // mark as spawned
                            break;
                        }
                    }
                }
            }

            return false;
        }


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(Utils.TopLeft(targetHitbox), Utils.Size(targetHitbox), ((Entity)((ModProjectile)this).Projectile).Center, ((Entity)((ModProjectile)this).Projectile).Center + Utils.ToRotationVector2(((ModProjectile)this).Projectile.rotation) * (float)((Entity)((ModProjectile)this).Projectile).height * ((ModProjectile)this).Projectile.scale, (float)((Entity)((ModProjectile)this).Projectile).width * ((ModProjectile)this).Projectile.scale * 0.5f, ref point);
        }

        public override void SafeOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 120, false);
        }

        private void SpawnThunderGust(Player player)
        {
            if (!ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
                return;

            int projType = thorium.Find<ModProjectile>("ThunderGust")?.Type ?? 0;
            if (projType <= 0)
                return;

            Vector2 shootDirection = Utils.ToRotationVector2(Projectile.rotation);

            Projectile proj = Main.projectile[Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                shootDirection * 5f,
                projType,
                Projectile.damage,
                Projectile.knockBack,
                player.whoAmI
            )];

            if (proj != null && proj.active)
            {
                proj.friendly = true;
                proj.hostile = false;
                proj.penetrate = 3;
                proj.DamageType = Projectile.DamageType;
            }

            if (player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.DD2_BookStaffCast, player.position);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 lighting = ((Entity)((ModProjectile)this).Projectile).Center + Utils.ToRotationVector2(((ModProjectile)this).Projectile.rotation) * (float)((Entity)((ModProjectile)this).Projectile).height * ((ModProjectile)this).Projectile.scale * 0.5f;
            lightColor = Lighting.GetColor((int)lighting.X / 16, (int)lighting.Y / 16) * MathHelper.Lerp(1f, 0f, (float)((ModProjectile)this).Projectile.alpha / 255f);
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(((ModProjectile)this).Texture, (AssetRequestMode)2);
            Main.EntitySpriteDraw(texture, ((Entity)((ModProjectile)this).Projectile).Center - Main.screenPosition, (Rectangle?)null, lightColor, ((ModProjectile)this).Projectile.rotation + (float)Math.PI / 2f - MathHelper.ToRadians((float)((ModProjectile)this).Projectile.spriteDirection * 15f), new Vector2((float)(texture.Width / 2 - ((ModProjectile)this).Projectile.spriteDirection * 8), (float)(texture.Height - 14)), ((ModProjectile)this).Projectile.scale, (Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0f);
            if (((ModProjectile)this).Projectile.ai[1] / ((ModProjectile)this).Projectile.ai[0] < 0.75f)
            {
                Player player = Main.player[((ModProjectile)this).Projectile.owner];
                float swingTime = ((ModProjectile)this).Projectile.ai[1] / (((ModProjectile)this).Projectile.ai[0] * 0.75f);
                for (int e = 0; e < 4; e++)
                {
                    swingTime = MathHelper.SmoothStep(0f, 1f, swingTime);
                }

                lightColor.A = 0;
                Color forestGreen = Color.Yellow;

                if (lightColor.R > forestGreen.R)
                    lightColor.R = forestGreen.R;

                if (lightColor.G > forestGreen.G)
                    lightColor.G = forestGreen.G;

                if (lightColor.B > forestGreen.B)
                    lightColor.B = forestGreen.B;

                swingTime = Utils.RotatedBy(Vector2.UnitX, (double)(swingTime * (float)Math.PI), default(Vector2)).Y - 0.4f;
                if (swingTime < 0f)
                {
                    swingTime = 0f;
                }
                texture = (Texture2D)ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Assets/Textures/Slash_3", (AssetRequestMode)2);
                Main.EntitySpriteDraw(texture,
                    player.MountedCenter - new Vector2(4f, 2f) * player.Directions - Main.screenPosition, 
                    (Rectangle?)null, lightColor * swingTime * 0.3f, 
                    ((ModProjectile)this).Projectile.rotation + MathHelper.ToRadians((float)((ModProjectile)this).Projectile.spriteDirection * 15f), 
                    Utils.Size(texture) / 2f, ((ModProjectile)this).Projectile.scale * 2f,
                    (((ModProjectile)this).Projectile.localAI[1] == 1f) ? SpriteEffects.FlipVertically : SpriteEffects.None,
                    0f);
            }
            return false;
        }

        public override bool? CanDamage()
        {
            if (!(((ModProjectile)this).Projectile.ai[1] < ((ModProjectile)this).Projectile.ai[0] * 0.6f) || !(((ModProjectile)this).Projectile.ai[1] > ((ModProjectile)this).Projectile.ai[0] * 0.1f))
            {
                return false;
            }
            return null;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(((Entity)((ModProjectile)this).Projectile).direction);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ((Entity)((ModProjectile)this).Projectile).direction = reader.ReadInt32();
        }
    }

    public class ThoriumThunderGustGlobal : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // Flag to mark scythe-spawned storms
        public bool fromScythe = false;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (fromScythe)
            {
                target.AddBuff(BuffID.Electrified, 120);
            }
        }
    }
}
