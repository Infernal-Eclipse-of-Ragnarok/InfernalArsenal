using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod;
using CalamityMod;
using CalamityMod.Sounds;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.NukePros
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class NukePro : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Wind;

        public override string Texture => "CalamityMod/Projectiles/Boss/AresGaussNukeProjectile";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 12;
        }

        public sealed override void SetBardDefaults()
        {
            Projectile.width = 102;
            Projectile.height = 106;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
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

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

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
                return;
            }

            if (Projectile.frameCounter % 2 == 0)
                Projectile.frame = (Projectile.frame + 1) % 12;

            if (Main.player[Projectile.owner].GetModPlayer<ThoriumPlayer>().accWindHoming)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy(Projectile.owner) && Vector2.DistanceSquared(npc.Center, Projectile.Center) < 500 * 500)
                    {
                        Vector2 vector = npc.Center - Projectile.Center;
                        float num4 = Projectile.velocity.Length();
                        vector.Normalize();
                        vector *= num4;
                        Projectile.velocity = (Projectile.velocity * 10f + vector) / 20f;
                        Projectile.velocity.Normalize();
                        Projectile.velocity *= num4;
                        break;
                    }
                }
            }

            if (hit)
            {
                Projectile.Kill();
                return;
            }

            Lighting.AddLight(Projectile.Center, Color.Lime.ToVector3() * 0.8f);

            if (Projectile.Center.Y > mouseY || ignoreMousePos)
            {
                Tile tile = Main.tile[new Point(
                    (int)(Projectile.Center.X +
                    Projectile.rotation.ToRotationVector2().X * Projectile.height / 2) / 16,
                    (int)(Projectile.Center.Y +
                    Projectile.rotation.ToRotationVector2().Y * Projectile.height / 2) / 16)];

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    groundY = Projectile.Center.Y;
                    Explode();
                    return;
                }
            }

            //Projectile.rotation += 0.2f;
            Projectile.velocity.Y += 0.275f;
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hitInfo, int damageDone)
        {
            Explode();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (target.hostile) // PvP safety
                Explode();
        }

        private void Explode()
        {
            if (hit) return; // already exploded

            hit = true;
            Projectile.velocity = Vector2.Zero;

            float j = 30;

            Projectile projectile = Projectile.NewProjectileDirect( Projectile.GetSource_FromAI(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<NukeBoom>(),
                Projectile.damage,
                1f,
                Projectile.owner,
                0f,    // ai[0]: current radius
                320f + j * 45f,  // ai[1]: maximum radius
                90f    // ai[2]: lifetime
                );

            projectile.localAI[1] = Main.rand.NextFloat(0.08f, 0.25f);
            projectile.Opacity = MathHelper.Lerp(0.18f, 0.6f, j / 7f) + Main.rand.NextFloat(-0.08f, 0.08f);
            projectile.netUpdate = true;

            //SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/ExoMechs/AresGaussNukeExplosion", (SoundType)0) with { Volume = 0.5f }, Projectile.position);
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
                    screenShakePower * 20f;
            }

            Projectile.Kill();
        }
    }
}
