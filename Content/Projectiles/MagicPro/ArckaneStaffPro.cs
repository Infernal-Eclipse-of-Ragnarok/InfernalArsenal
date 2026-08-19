using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Dusts.WaterSplash;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static CatalystMod.Effects.Begin;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.MagicPro
{
    public class ArckaneGemPro : ModProjectile
    {
        public static readonly int[] dusts = new int[5]
        {
            59,
            60,
            61,
            62,
            6
        };

        private const float RotationOffset = 0f;

        private bool storedVelocity = false;
        private float originalSpeed;
        private float speedTimer = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 13;
            Projectile.height = 23;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = Projectile.width / 2;
            height = Projectile.height / 2;
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Magick Staff Debuffs
            target.AddBuff(BuffID.Ichor, 120, false);
            target.AddBuff(BuffID.Confused, 90, false);
            target.AddBuff(BuffID.OnFire, 300, false);
            target.AddBuff(BuffID.Frostburn, 300, false);
            target.AddBuff(BuffID.Poisoned, 300, false);
            
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thor))
            {
                if (!target.boss) target.AddBuff(thor.Find<ModBuff>("Stunned").Type, 30, false);
                target.AddBuff(thor.Find<ModBuff>("Charmed").Type, 180, false);
                //target.AddBuff(thor.Find<ModBuff>("MagickStaffDebuff").Type, 300, false);
            }

            if (target.IsHostile())
            {
                Player player = Main.player[Projectile.owner];
                player.statLife += 5;
                if (player.statLife > player.statLifeMax2)
                    player.statLife = player.statLifeMax2;
                player.HealEffect(5, true);
            }

            //Arckane Staff Debuffs
            target.AddBuff(BuffID.CursedInferno, 300, false);
            target.AddBuff(ModContent.BuffType<Crumbling>(), 120, false);
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300, false);
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 300, false);
            target.AddBuff(ModContent.BuffType<Plague>(), 300, false);
            target.AddBuff(BuffID.ShadowFlame, 300, false);
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300, false);
        }

        public override void AI()
        {
            if (!storedVelocity)
            {
                originalSpeed = Projectile.velocity.Length();
                storedVelocity = true;
            }
            // Face the direction you're aiming (the velocity).
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // multiply speed but keep direction
            if (speedTimer < 3)
                speedTimer += 0.04f;
            if (speedTimer == 3)
                speedTimer = 3.1f;

            float speedFactor = 0.1f * (float)(Math.Exp(speedTimer));
            if (Projectile.velocity != Vector2.Zero)
            {
                Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                Projectile.velocity = currentDir * originalSpeed * speedFactor;
            }

            // Avoid horizontal mirroring when rotating.
            Projectile.spriteDirection = 1;
            Projectile.direction = 1;

            if (!Utils.NextBool(Main.rand, 2)) return;
            for (int i = 0; i < dusts.Length; i++)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dusts[i],
                    Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 50, default, 1.35f).noGravity = true;

            Vector3 blue = new Vector3(0.51f, 0.957f, 1f);
            Vector3 red = new Vector3(1f, 0.255f, 0.392f);
            Vector3 color = Vector3.Lerp(blue,red, Main.rand.NextFloat(0.3f, 0.7f));
            Lighting.AddLight(Projectile.Center, color);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center, null);
            for (int index = 0; index < dusts.Length; ++index)
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dusts[index], oldVelocity.X * 0.2f, oldVelocity.Y * 0.2f, 100, Color.White, 1.25f);
            return true;
        }
    }
    public class ArckaneScythe1Pro : ModProjectile
    {
        public ref float GemLocation => ref Projectile.ai[2];
        private float oscillationTimer = 0f;
        private float rotationTimer = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() > 0.0001f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            oscillationTimer += (float)(Math.PI / 12);
            if (oscillationTimer >= MathHelper.TwoPi)
                oscillationTimer = 0;

            rotationTimer += 0.5f;
            Projectile.rotation = rotationTimer;
            Projectile.velocity = 1.25f * Main.projectile[(int)GemLocation].velocity.RotatedBy(-Math.Sin(oscillationTimer));

            if (!Main.projectile[(int)GemLocation].active)
                Projectile.Kill();

            Projectile.spriteDirection = 1;
            Projectile.direction = 1;

            Lighting.AddLight(Projectile.Center, 0.671f, 0.98f, 1f);

            Dust splash = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<SulphuricSplash>(), Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 25, default, 1.35f);
            splash.noGravity = true;
            if (Main.rand.NextBool(3))
            {
                splash.scale *= 2f;
                splash.velocity *= 2f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CrushDepth>(), 300, false);
        }

    }
    public class ArckaneScythe2Pro : ModProjectile
    {
        public ref float GemLocation => ref Projectile.ai[2];
        private float oscillationTimer = 0f;
        private float rotationTimer = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            oscillationTimer += (float)(Math.PI / 12);
            if (oscillationTimer >= MathHelper.TwoPi)
                oscillationTimer = 0;

            rotationTimer += 0.5f;
            Projectile.rotation = rotationTimer;
            Projectile.velocity = 1.25f * Main.projectile[(int)GemLocation].velocity.RotatedBy(Math.Sin(oscillationTimer));

            if (!Main.projectile[(int)GemLocation].active)
                Projectile.Kill();

            Projectile.spriteDirection = 1;
            Projectile.direction = 1;

            Lighting.AddLight(Projectile.Center, 1f, 0.553f, 0.51f);

            Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<BrimstoneFlame>(),Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 50, default, 1.35f);
            fire.noGravity = true;
            if (Main.rand.NextBool(3))
            {
                fire.scale *= 2f;
                fire.velocity *= 2f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 300, false);
        }
    }
}
