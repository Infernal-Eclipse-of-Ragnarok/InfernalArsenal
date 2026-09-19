using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;
using ThoriumMod.Sounds;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class SandSlasher : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<DamageReduction>(1);
            Empowerments.AddInfo<Defense>(1);
        }

        public override void SetBardDefaults()
        {
            Item.Size = new Vector2(62, 62);

            Item.useTime = 40;
            Item.useAnimation = 40;
            ((ModItem)this).Item.holdStyle = 5;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Guitar;

            Item.DamageType = ModContent.GetInstance<BardDamage>();
            Item.damage = 10;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.UseSound = ThoriumSounds.String_Sound;

            Item.value = Item.buyPrice(0, 2);
            Item.rare = ItemRarityID.Green;

            Item.shoot = ModContent.ProjectileType<SandSlasher_Projectile>();
            Item.shootSpeed = 20f;

            InspirationCost = 2;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 10);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(-10, 10) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = new Vector2(-10, 10f) * player.Directions;

            player.itemLocation += offset;
        }
    }

    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class SandSlasher_Projectile : BardProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Content/Projectiles/BardPro/SandSlasher_Projectile";
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public int TileBounces = 2;

        public override void SetBardDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.Size = new Vector2(32, 32);
            Projectile.timeLeft = 300;
            Projectile.penetrate = 5;
            Projectile.friendly = true;

            // Let each NPC be hit immediately again if needed
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.usesIDStaticNPCImmunity = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Sand trail effect
            if (Main.rand.NextBool(2)) // spawn every other frame for performance
            {
                Vector2 dustPos = Projectile.Center - Projectile.velocity * 0.25f; // slightly behind
                Dust dust = Dust.NewDustPerfect(
                    dustPos,
                    DustID.Sand,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f), // trailing motion
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.3f)
                );
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Ensure full damage every hit
            Projectile.damage = Projectile.damage + (Projectile.damage / 2);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TileBounces--;
            SoundEngine.PlaySound(SoundID.Item60, Projectile.position);

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 spawnPos = Projectile.Center + new Vector2(0f, -Projectile.height / 2f);
                var tornado = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<SandSlasherSandnado>(),
                    Projectile.damage / 2,
                    Projectile.knockBack
                );
                tornado.timeLeft = 120;
                tornado.netUpdate = true;
            }

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            return TileBounces < 0;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null, // draw full texture, not sliced
                lightColor,
                Projectile.rotation,
                origin,
                0.5f,
                SpriteEffects.None,
                0f
            );

            return false;
        }
    }


    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class SandSlasherSandnado : BardProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SandnadoFriendly;

        private const int MaxSegments = 6;
        private const float MaxRotationSpeed = 0.5f;
        private const float BaseScale = 0.4f;
        private const float TopScale = 1f;
        private const float BaseOpacity = 0.3f;
        private const float TopOpacity = 0.8f;
        private const float VerticalMargin = 30f;

        private bool initialized = false;
        private float[] segmentOffsets = new float[MaxSegments];
        private Vector2 spawnPosition;

        private int originalDamage;

        public override void SetBardDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 60;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.usesIDStaticNPCImmunity = false;
            Projectile.localNPCHitCooldown = 10; // hit NPCs every tick
            Projectile.timeLeft = 120;

            originalDamage = Projectile.damage;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.damage = Math.Max(Projectile.damage, 5);

            // Play grenade bounce sound
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); // Grenade bounce sound

            // Make a sand dust ring burst
            int dustCount = 12;
            float dustSpeed = 6f;

            Vector2 bottomCenter = new Vector2(Projectile.Center.X, (Projectile.Bottom.Y - 24f));

            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 velocity = angle.ToRotationVector2() * dustSpeed;
                Dust dust = Dust.NewDustPerfect(bottomCenter, DustID.Sand, velocity, 100, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
            }
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation += MaxRotationSpeed * Projectile.direction;

            if (!initialized)
            {
                spawnPosition = Projectile.Center;
                for (int i = 0; i < MaxSegments; i++)
                    segmentOffsets[i] = Main.rand.NextFloat();
                initialized = true;
            }

            // Dust spawning
            for (int i = 0; i < 2; i++)
            {
                float verticalFactor = Main.rand.NextFloat();
                float yOffset = VerticalMargin + verticalFactor * (Projectile.height - 2 * VerticalMargin);
                float oscillation = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5 + verticalFactor * MathHelper.TwoPi) * (Projectile.width / 2f);

                Vector2 dustPos = spawnPosition + new Vector2(0, -Projectile.height / 2 + yOffset) + new Vector2(oscillation, 0);

                Dust d = Dust.NewDustPerfect(
                    dustPos,
                    DustID.Sand,
                    new Vector2(0, -Main.rand.NextFloat(0.5f, 1.2f)),
                    100,
                    default,
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
                d.noGravity = true;
            }
        }

        public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Ensure full damage every hit
            // Projectile.damage = Projectile.damage + (Projectile.damage / 2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = tex.Frame();
            Vector2 origin = frame.Size() / 2f;

            float fadeMultiplier = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;

            for (int i = 0; i < MaxSegments; i++)
            {
                float factor = i / (float)(MaxSegments - 1);
                float yOffset = Projectile.height - VerticalMargin - factor * (Projectile.height - 2 * VerticalMargin);
                float scale = MathHelper.Lerp(BaseScale, TopScale, factor);
                float opacity = MathHelper.Lerp(BaseOpacity, TopOpacity, factor) * fadeMultiplier;
                Color drawColor = new Color(230, 200, 140) * opacity;

                Vector2 drawPos = spawnPosition + new Vector2(0, -Projectile.height / 2 + yOffset) - Main.screenPosition;

                Main.spriteBatch.Draw(tex, drawPos, frame, drawColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}