using InfernalEclipseWeaponsDLC.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod;
using Terraria.ID;
using CalamityMod.Buffs.DamageOverTime;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.PocketConcert
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public class MusicalNoteProjectile : BardProjectile
    {
        public static readonly Color[] Colors =
    {
        new Color(70, 229, 255),   // Blue
        new Color(238, 57, 252),   // Purple
        new Color(66, 255, 78),    // Green
        new Color(255, 81, 72),    // Red
        new Color(255, 233, 52),   // Yellow
    };

        private Color Color => Colors[Projectile.frame];

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            Main.projFrames[Type] = 5;
        }

        public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;

        public override void SetBardDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.friendly = true;

            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.scale = 1f;

            Projectile.timeLeft = 60;
            Projectile.penetrate = 2;

            Projectile.localNPCHitCooldown = 40;
            Projectile.idStaticNPCHitCooldown = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesIDStaticNPCImmunity = false;
        }

        private int DebuffID;

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            Projectile.frame = Main.rand.Next(Main.projFrames[Type]);

            // Assign debuff based on frame/color
            switch (Projectile.frame)
            {
                case 0: // Blue
                    DebuffID = BuffID.Electrified;
                    break;
                case 1: // Purple
                    DebuffID = BuffID.ShadowFlame;
                    break;
                case 2: // Green
                    DebuffID = ModContent.BuffType<Plague>();
                    break;
                case 3: // Red
                    DebuffID = ModContent.BuffType<BrimstoneFlames>();
                    break;
                case 4: // Yellow
                    DebuffID = BuffID.Ichor;
                    break;
                default:
                    DebuffID = 0;
                    break;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (DebuffID > 0)
            {
                int duration = 180;
                target.AddBuff(DebuffID, duration);
            }
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);

            var position = Projectile.Center;
            var velocity = Main.rand.NextVector2Circular(4f, 4f) * 2f;

            SpawnDustEffects(in position, in velocity);
        }

        private void SpawnDustEffects(in Vector2 position, in Vector2 velocity)
        {
            for (var i = 0; i < 5; i++)
            {
                var offset = Main.rand.NextVector2Circular(8f, 8f) * 2f;
                var dust = Dust.NewDustDirect(position + offset, 0, 0, ModContent.DustType<NoteDust>(), velocity.X, velocity.Y);

                dust.noGravity = true;

                dust.scale = Main.rand.NextFloat(1f, 3f) * Projectile.scale;

                dust.color = Color;
            }
        }

        public override void AI()
        {
            base.AI();

            UpdateEffects();
            UpdateScale();

            Projectile.velocity.Y += 0.1f;
            Projectile.velocity.X *= 1.01f;
        }

        private void UpdateEffects()
        {
            var position = Projectile.Center;
            var velocity = Projectile.velocity / 2f;

            SpawnDustEffects(in position, in velocity);
        }

        private void UpdateScale()
        {
            Projectile.scale = MathHelper.Clamp(Projectile.scale, 0f, 1f);

            if (Projectile.scale >= 1f)
            {
                return;
            }

            Projectile.scale += 0.05f + Projectile.scale / 10f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawProjectile();

            return false;
        }

        private void DrawProjectile()
        {
            var texture = TextureAssets.Projectile[Type].Value;

            var frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            var color = Projectile.GetAlpha(Color.White);
            var origin = frame.Size() / 2f;

            var position = Projectile.Center - Main.screenPosition + new Vector2(DrawOffsetX, Projectile.gfxOffY);

            var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, position, frame, color, Projectile.rotation, origin, Projectile.scale, effects);
        }
    }
}