using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Sounds;
using InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro;

namespace InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class GlowstringBiwa : BardItem
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.String;

        public override void SetStaticDefaults()
        {
            Empowerments.AddInfo<InvincibilityFrames>(1);
            Empowerments.AddInfo<LifeRegeneration>(1);
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
            Item.damage = 24;
            Item.knockBack = 4f;
            Item.noMelee = true;

            Item.UseSound = ThoriumSounds.String_Sound;

            Item.value = Item.buyPrice(0, 1);
            Item.rare = ItemRarityID.Blue;

            Item.shoot = ModContent.ProjectileType<GlowstringBiwaPro>();
            Item.shootSpeed = 5f;

            InspirationCost = 2;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-12, 12);
        }

        public override void HoldItemFrame(Player player)
        {
            player.itemLocation += new Vector2(-12, 12f) * player.Directions;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Vector2 offset = new Vector2(-12, 12f) * player.Directions;

            player.itemLocation += offset;
        }

        public override bool BardShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int numberProjectiles = 2;
            float spreadAngle = MathHelper.ToRadians(20f);
            float baseSpeed = velocity.Length();
            float baseAngle = velocity.ToRotation();

            // Generate a unique pair ID for these two projectiles
            int pairID = Main.rand.Next(100000);

            int firstProj = 0;
            int secondProj = 0;

            for (int i = 0; i < numberProjectiles; i++)
            {
                float offset = (i - (numberProjectiles - 1) / 2f) * spreadAngle;
                Vector2 newVelocity = baseSpeed * baseAngle.ToRotationVector2().RotatedBy(offset);

                int projIndex = Projectile.NewProjectile(
                    source,
                    position,
                    newVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );

                Projectile proj = Main.projectile[projIndex];
                proj.ai[0] = i;        // 0 = first, 1 = second
                proj.ai[1] = pairID;   // pair ID

                if (i == 0) firstProj = projIndex;
                if (i == 1) secondProj = projIndex;
            }

            // Spawn the beam
            int beamIndex = Projectile.NewProjectile(
                source,
                position,
                Vector2.Zero,
                ModContent.ProjectileType<GlowstringBeam>(),
                damage,
                knockback,
                player.whoAmI
            );

            Projectile beam = Main.projectile[beamIndex];
            beam.ai[0] = firstProj;
            beam.ai[1] = secondProj;

            return false;
        }
    }
}