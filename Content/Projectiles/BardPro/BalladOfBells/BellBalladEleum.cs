using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Projectiles.Bard;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.BalladOfBells
{
    [JITWhenModsEnabled("ThoriumMod")]
    [ExtendsFromMod("ThoriumMod")]
    public class BellBalladEleum : BardProjectile
    {
        public override BardInstrumentType InstrumentType => BardInstrumentType.Percussion;
        public int BellIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public ref float AI_Timer => ref Projectile.ai[1];

        public bool Despawn
        {
            get => Projectile.ai[2] > 0f;
            set => Projectile.ai[2] = value ? 1 : -1;
        }

        public Player Player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
        }

        public override void SetBardDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 26;
            Projectile.tileCollide = false;
            Projectile.timeLeft = int.MaxValue;
            Projectile.Opacity = 0f;

            Despawn = false;
        }

        public override bool? CanHitNPC(NPC target) => false;
        public override bool CanHitPvp(Player target) => false;

        public override void AI()
        {
            AI_Timer++;
            Projectile.rotation = 0f;

            float movementProgress;
            if (Despawn)
            {
                Projectile.Opacity -= 0.05f;
                movementProgress = MathHelper.Clamp(Projectile.timeLeft / 20f, 0, 1);
            }
            else
            {
                Projectile.timeLeft = int.MaxValue;
                movementProgress = MathHelper.Clamp(AI_Timer / 20f, 0, 1);

                if (Projectile.Opacity < 1f)
                    Projectile.Opacity += 0.1f;

                if (Projectile.owner == Main.myPlayer)
                {
                    bool bellBalladEquipped = Player.HeldItem.type == ModContent.ItemType<BellBallad>() || Main.mouseItem.type == ModContent.ItemType<BellBallad>();
                    if (!bellBalladEquipped)
                    {
                        Despawn = true;
                        Projectile.timeLeft = 20;
                        Projectile.netUpdate = true;
                    }
                }

                if (Player.dead || !Player.active)
                {
                    Despawn = true;
                    Projectile.timeLeft = 20;
                    Projectile.netUpdate = true;
                }
            }

            float radians = AI_Timer * 0.01f % MathHelper.TwoPi + MathHelper.TwoPi / 3f * BellIndex;
            Projectile.Center = Player.Center + new Vector2(0, Player.gfxOffY) + new Vector2(75 * movementProgress, 0).RotatedBy(radians);
        }

        public virtual void Shoot(int damage, float knockBack)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(default) * 10f;
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BellBalladEleumBolt>(), damage, knockBack);
            }
        }
    }
}
