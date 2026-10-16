using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;
using CalamityMod;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NukePros
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class NukeMissile : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override string Texture => "CalamityMod/Projectiles/Boss/ApolloRocket";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public sealed override void SetBardDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 66;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 999999999;
            Projectile.tileCollide = false;
        }


        public int target = -1;
        public int random;
        public bool hit;
        public float groundY = 0;
        public bool firstFrame = true;
        public float mouseY = 0;
        public bool ignoreMousePos = false;
        private bool isHoming;

        public override void AI()
        {
            if (firstFrame)
            {
                firstFrame = false;

                mouseY = Main.MouseWorld.Y;

                Tile tile = Main.tile[
                    new Point(
                        (int)Main.MouseWorld.X / 16,
                        (int)Main.MouseWorld.Y / 16
                    )
                ];

                ignoreMousePos = tile.HasTile && Main.tileSolid[tile.TileType];

                // Snap the missile's velocity towards the mouse.
                Vector2 direction = Main.MouseWorld - Projectile.Center;

                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                    Projectile.velocity = direction * Projectile.velocity.Length();
                }

                // Rotate the sprite to match its initial direction.
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            if (Projectile.frameCounter > 500)
            {
                Projectile.Kill();
            }

            if (Projectile.frameCounter % 2 == 0)
            {
                Projectile.frame = Projectile.frame++ % 5;
            }


            if (Projectile.scale > 0 && hit)
            {
                Projectile.scale -= 0.01f;
            }


            if (hit && Projectile.scale < 0.01f)
            {
                Projectile.Kill();
            }

            bool windHoming = Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming;

            isHoming = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.CanBeChasedBy(Projectile.owner) &&
                    Vector2.DistanceSquared(npc.Center, Projectile.Center) < 1000 * 1000)
                {
                    Vector2 vector = npc.Center - Projectile.Center;
                    float speed = Projectile.velocity.Length();

                    if (vector != Vector2.Zero)
                    {
                        vector.Normalize();
                        vector *= speed;

                        float inertia = windHoming ? 5f : 10f;

                        Projectile.velocity =
                            (Projectile.velocity * inertia + vector) / (inertia + 1f);

                        if (Projectile.velocity != Vector2.Zero)
                        {
                            Projectile.velocity.Normalize();
                            Projectile.velocity *= speed;
                        }

                        isHoming = true;
                    }

                    break;
                }
            }

            if (hit)
            {
                Projectile.velocity.X = 0;
                Projectile.velocity.Y = 0;
                Projectile.Center += new Vector2(0, ((1 - Projectile.scale) * Projectile.height) / 100);
            }
            else
            {
                SpawnSmokeTrail();
                Lighting.AddLight(Projectile.Center, Color.Lime.ToVector3() * 0.8f);
                Projectile.rotation = Projectile.velocity.ToRotation();
                if (!isHoming && (Projectile.Center.Y > mouseY || ignoreMousePos))
                {
                    Tile tile = Main.tile[new Point((int)(Projectile.Center.X + Projectile.rotation.ToRotationVector2().X * Projectile.height / 2) / 16, (int)(Projectile.Center.Y + Projectile.rotation.ToRotationVector2().Y * Projectile.height / 2) / 16)];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        groundY = Projectile.Center.Y;
                        // Projectile.Center -= Projectile.rotation.ToRotationVector2() * Projectile.height * 0.8f;
                        hit = true;
                        Projectile projectile = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<NukeBoom>(),
                        Projectile.damage, // explosion damage
                        1f  // explosion knockback
                        );

                        float j = 5;
                        projectile.ai[1] = 320f + j * 45f;
                        projectile.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f);
                        projectile.Opacity = MathHelper.Lerp(0.18f, 0.6f, j / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
                        projectile.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
                        //Screenshake
                        if (WeaponConfig.Instance.EnableScreenEffects)
                        {
                            float screenShakePower = Utils.GetLerpValue(
                                1300f,
                                0f,
                                Projectile.Distance(Main.LocalPlayer.Center),
                                true
                            );

                            Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                                screenShakePower * 6f;
                        }

                        Projectile.Kill();
                    }
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Projectile.velocity.Y += 0.275f;
            }
        }

        private void SpawnSmokeTrail()
        {
            if (hit)
                return;

            Vector2 smokePosition = Projectile.Center -
                Projectile.velocity.SafeNormalize(Vector2.UnitY) * (Projectile.height * 0.35f);

            Dust smoke = Dust.NewDustPerfect(
                smokePosition,
                DustID.Smoke,
                -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                60,
                Color.Gray,
                Main.rand.NextFloat(0.7f, 0.9f)
            );

            smoke.noGravity = true;
            smoke.fadeIn = 0.3f;
            smoke.velocity *= 0.4f;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (target.hostile) // only if PvP enabled
                Explode();
        }

        private void Explode()
        {
            if (hit) return; // prevent double explosions if it already hit tiles

            hit = true;
            Projectile.velocity = Vector2.Zero;

            Projectile projectile = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<NukeBoom>(),
                        Projectile.damage, // explosion damage
                        1f  // explosion knockback
                    );

            float j = 5;
            projectile.ai[1] = 320f + j * 45f;
            projectile.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f);
            projectile.Opacity = MathHelper.Lerp(0.18f, 0.6f, j / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
            projectile.netUpdate = true;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            //Screenshake
            if (WeaponConfig.Instance.EnableScreenEffects)
            {
                float screenShakePower = Utils.GetLerpValue(
                    1300f,
                    0f,
                    Projectile.Distance(Main.LocalPlayer.Center),
                    true
                );

                Main.LocalPlayer.Calamity().GeneralScreenShakePower =
                    screenShakePower * 6f;
            }
        }

    }
}
