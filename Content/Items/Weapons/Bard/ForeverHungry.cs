using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Sounds;
using ThoriumMod.Projectiles.Bard;
using System;
using Terraria.Audio;
using Terraria.DataStructures;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class ForeverHungry : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<DamageReduction>(1);
            Empowerments.AddInfo<Defense>(1);
            Empowerments.AddInfo<InvincibilityFrames>(1);
        }

        public override void SetBardDefaults()
        {
            Item.Size = new Vector2(44, 48);
            Item.value = Item.buyPrice(0, 10);

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Guitar;

            Item.DamageType = ModContent.GetInstance<BardDamage>();
            Item.damage = 48;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.UseSound = ThoriumSounds.String_Sound;
            Item.rare = ItemRarityID.LightRed;

            Item.shoot = ModContent.ProjectileType<BouncingHungry>();
            Item.shootSpeed = 20f;

            InspirationCost = 1;

            ((ModItem)this).Item.holdStyle = 5;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6, 6);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(-6, 6f) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = new Vector2(-6, 6f) * player.Directions;

            player.itemLocation += offset;
        }
    }

    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BouncingHungry : BardProjectile
    {
        public override string Texture => $"Terraria/Images/NPC_{NPCID.TheHungry}";

        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public float BounceDampenX = .8f;
        public float BounceDampenY = .8f;
        public int TileBounces = 2;

        public override void SetBardDefaults()
        {
            Main.projFrames[Type] = 3;

            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.Size = new Vector2(32, 32);
            Projectile.timeLeft = 1000;
            Projectile.penetrate = 5;

            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathF.PI;
            Projectile.velocity.Y += .8f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            TileBounces--;

            // Original dust
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.Center, 10, 10, DustID.BloodWater, Scale: 1.5f);
                Dust.NewDust(Projectile.Center, 10, 10, DustID.Blood, Scale: 1.3f);
            }

            // Spawn blood projectiles in a burst
            int burstCount = 6;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f));
                Vector2 velocity = new Vector2(0, -7f).RotatedBy(angle);
                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<ForeverHungryBlood>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            SoundEngine.PlaySound(SoundID.NPCHit9, Projectile.position);

            // Bounce logic
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
            {
                Projectile.velocity.X = -oldVelocity.X * BounceDampenX;
            }
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
            {
                Projectile.velocity.Y = -oldVelocity.Y * BounceDampenY;
            }

            return TileBounces < 0;
        }

    }

    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class ForeverHungryBlood : BardProjectile
    {
        public override string Texture => "InfernalEclipseWeaponsDLC/Assets/Textures/Empty";

        public override void SetBardDefaults()
        {
            Projectile.DamageType = ModContent.GetInstance<BardDamage>();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.aiStyle = 0;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = true;

            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Reduce damage to 1/5 of initial damage on spawn
            Projectile.damage = Math.Max(Projectile.damage / 5, 10);
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.3f;

            // Optional: fade out
            Projectile.alpha += 2;
            if (Projectile.alpha > 255)
                Projectile.Kill();

            // Optional dust trail
            Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Projectile.velocity * 0.2f, 150, Color.Red, 1.2f).noGravity = true;
        }
    }
}
