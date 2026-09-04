using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.SummonerPro.MinionPro
{
    public class EvilPumpkinMinion : ModProjectile
    {
        private const int State_Follow = 0;
        private const int State_Attack = 1;
        private const int State_Respawn = 2;

        public ref float State => ref Projectile.ai[0];
        public ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active) player.ClearBuff(ModContent.BuffType<Buffs.EvilPumpkinBuff>());
            if (player.HasBuff(ModContent.BuffType<Buffs.EvilPumpkinBuff>())) Projectile.timeLeft = 2;

            if (Projectile.velocity.X != 0f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;
            }

            if (State != State_Respawn)
            {
                Projectile.frameCounter++;
                int maxFrames = State == State_Attack ? 8 : 4;
                if (Projectile.frameCounter >= 6)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % maxFrames;
                }
            }
            else
            {
                Projectile.alpha = 255;
                Projectile.friendly = false;
                Projectile.position = player.Center;
                Timer++;

                if (Timer > 60) // Waits for 1 second before "respawning" for a rapid fire feeling -Arkangel
                {
                    State = State_Follow;
                    Timer = 0;
                    Projectile.alpha = 0;
                    Projectile.friendly = true;
                }
                return;
            }

            Vector2 targetCenter = player.Center;
            bool foundTarget = false;
            float distanceFromTarget = 1000f;

            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                if (between < 2000f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;

                        if ((closest && inRange) || !foundTarget)
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            if (foundTarget)
            {
                State = State_Attack;
                Vector2 direction = targetCenter - Projectile.Center;
                direction = direction.SafeNormalize(Vector2.Zero) * 12f;
                Projectile.velocity = (Projectile.velocity * 40f + direction) / 41f;
            }
            else
            {
                State = State_Follow;
                Vector2 direction = targetCenter - Projectile.Center;
                float distance = direction.Length();

                if (distance > 2000f) Projectile.position = targetCenter;
                else if (distance > 80f)
                {
                    direction = direction.SafeNormalize(Vector2.Zero) * 8f;
                    Projectile.velocity = (Projectile.velocity * 20f + direction) / 21f;
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }
            }

            if (State != State_Respawn)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (other.active && other.owner == Projectile.owner && other.type == Projectile.type && other.whoAmI != Projectile.whoAmI)
                    {
                        if (other.ai[0] != State_Respawn)
                        {
                            float distance = Vector2.Distance(Projectile.Center, other.Center);
                            if (distance < Projectile.width * 1.5f) 
                            {
                                Vector2 push = (Projectile.Center - other.Center).SafeNormalize(Vector2.UnitY);
                                Projectile.velocity += push * 0.15f; 
                            }
                        }
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (State != State_Respawn)
            {
                Explode();
                State = State_Respawn;
                Timer = 0;
            }
        }

        private void Explode()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];
                if (target.active && !target.friendly && !target.dontTakeDamage && Vector2.Distance(Projectile.Center, target.Center) < 100f)
                {
                    target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f);
                }
            }

            for (int i = 0; i < 30; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Smoke;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2f);
                dust.velocity *= 3f;
                if (dust.type == DustID.Torch) dust.noGravity = true;
            }

            if (Main.netMode != NetmodeID.Server)
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Mod.Find<ModGore>("EvilPumpkinGore_1").Type);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Mod.Find<ModGore>("EvilPumpkinGore_2").Type);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Mod.Find<ModGore>("EvilPumpkinGore_3").Type);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Mod.Find<ModGore>("EvilPumpkinGore_4").Type);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string basePath = "InfernalEclipseWeaponsDLC/Content/Projectiles/SummonerPro/MinionPro/";

            Texture2D texture = State == State_Attack //not sure if this is the way it was intended but it does the "night" version of the minion while attacking instead of being based on time of day -Arkangel
                ? ModContent.Request<Texture2D>(basePath + "EvilPumpkinMinion_Night").Value
                : ModContent.Request<Texture2D>(basePath + "EvilPumpkinMinion").Value;

            int totalFrames = State == State_Attack ? 8 : 4;
            int frameHeight = texture.Height / totalFrames;
            if (Projectile.frame >= totalFrames) Projectile.frame = 0;

            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Color drawColor = Projectile.GetAlpha(lightColor);

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            return false;
        }
    }
}