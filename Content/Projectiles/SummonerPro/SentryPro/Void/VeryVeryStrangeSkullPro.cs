using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SOTS;
using SOTS.Void;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.SentryPro.Void
{
    public class VeryVeryStrangeSkullPro : ModProjectile
    {
        public bool hoverDown = true;
        public int hoverAmount;
        public bool pulseShift;
        public float pulseAmount;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 11;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 66;
            Projectile.penetrate = -1;
            Projectile.DamageType = ModLoader.TryGetMod("SOTS", out Mod sots) ? sots.Find<DamageClass>("VoidSummon") : DamageClass.Summon;
            Projectile.sentry = true;
            Projectile.timeLeft = Projectile.SentryLifeTime;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            drawPosition.Y += Projectile.gfxOffY;
            Color color = lightColor * 0.25f * Projectile.Opacity;
            float scale = Projectile.scale + 0.1f + pulseAmount;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPosition, frame, color, Projectile.rotation, origin, scale, effects);
        }

        public override void AI()
        {
            // Hover up and down.
            hoverAmount++;

            if (hoverAmount % 15 == 0)
                Projectile.position.Y += hoverDown ? 1f : -1f;

            if (hoverAmount > 90)
            {
                hoverDown = !hoverDown;
                hoverAmount = 0;
            }

            // Pulse effect.
            if (!pulseShift)
            {
                pulseAmount += 1f / 400f;

                if (pulseAmount >= 0.15f)
                    pulseShift = true;
            }
            else
            {
                pulseAmount -= 1f / 400f;

                if (pulseAmount <= 0f)
                    pulseShift = false;
            }

            // Find the nearest enemy.
            NPC target = null;
            float targetDistance = (700f + 200);

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    float d = Vector2.Distance(npc.Center, Projectile.Center);
                    if (d < targetDistance)
                    {
                        targetDistance = d;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                Vector2 direction = target.Center - Projectile.Center;
                Projectile.rotation = direction.ToRotation() - MathHelper.PiOver2;

                if (Projectile.ai[2] == 0f)
                {
                    Projectile.ai[1]++;

                    if (Projectile.ai[1] == 60f)
                    {
                        // Only the owning player can consume Void and authorize the burst.
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Player owner = Main.player[Projectile.owner];

                            bool canFire = !ModLoader.HasMod("SOTS") ||
                                StrangeSkullVoidHelper.TryConsumeVoid(owner, 4);

                            if (canFire)
                            {
                                Projectile.ai[0] = 1f;
                                Projectile.ai[2] = -1f; // Authorized, waiting to fire.

                                Projectile.frame = 0;
                                Projectile.frameCounter = 0;
                            }
                            else
                            {
                                Projectile.ai[1] = 0f;
                                Projectile.ai[0] = 0f;
                                Projectile.ai[2] = 0f;

                                Projectile.frame = 0;
                                Projectile.frameCounter = 0;
                            }

                            Projectile.netUpdate = true;
                        }
                    }
                }
            }
            else
            {
                Projectile.rotation = 0f;

                if (Projectile.ai[2] == 0f)
                {
                    Projectile.ai[1] = 0f;
                    Projectile.ai[0] = 0f;
                    Projectile.frame = 0;
                    Projectile.frameCounter = 0;
                }
            }

            // Void was successfully consumed, so the burst is authorized.
            if (Projectile.ai[2] == -1f)
            {
                Projectile.ai[1]++;

                if (Projectile.ai[1] >= 90f)
                {
                    Projectile.ai[1] = 0f;
                    Projectile.ai[2] = 1f;
                    Projectile.localAI[0] = 0f;

                    Projectile.netUpdate = true;
                }
            }

            if (Projectile.ai[2] > 0f)
            {
                Projectile.ai[2]++;

                if (Projectile.ai[2] == 2f || Projectile.ai[2] == 12f || Projectile.ai[2] == 22f)
                {
                    NPC burstTarget = null;
                    float burstTargetDistance = 900f;

                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (!npc.CanBeChasedBy())
                            continue;

                        float distance = Vector2.Distance(npc.Center, Projectile.Center);

                        if (distance < burstTargetDistance)
                        {
                            burstTargetDistance = distance;
                            burstTarget = npc;
                        }
                    }

                    if (burstTarget != null && Main.myPlayer == Projectile.owner)
                    {
                        Vector2 velocity = (burstTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 7.5f;

                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<VeryVeryStrangeSkullShotPro>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }

                    Projectile.localAI[0]++;
                }

                if (Projectile.localAI[0] >= 3f)
                {
                    Projectile.ai[2] = 0f;
                    Projectile.localAI[0] = 0f;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width,  Projectile.height, DustID.GemAmethyst, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 100, default, 2f);

                Main.dust[dust].noGravity = true;
            }
        }

        public override void PostAI()
        {
            if (Projectile.ai[0] <= 0f)
            {
                Projectile.frame = 0;
                Projectile.frameCounter = 0;
                return;
            }

            Projectile.frameCounter++;

            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                if (Projectile.frame >= Main.projFrames[Type])
                {
                    Projectile.frame = 0;
                    Projectile.ai[0] = 0f;
                }
            }
        }
    }

    [JITWhenModsEnabled("SOTS")]
    [ExtendsFromMod("SOTS")]
    public class StrangeSkullVoidHelper
    {
        public static bool HasEnoughVoid(Player player, int voidNeeded)
        {
            VoidPlayer vp = player.VoidPlayer();

            return vp.voidMeter > voidNeeded;
        }

        public static bool TryConsumeVoid(Player player, int amount)
        {
            VoidPlayer vp = player.VoidPlayer();

            if (vp.voidMeter < amount)
                return false;

            vp.voidMeter -= amount;
            return true;
        }
    }
}
