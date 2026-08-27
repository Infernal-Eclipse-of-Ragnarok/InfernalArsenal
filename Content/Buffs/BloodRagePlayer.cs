using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Buffs
{
    public class BloodRagePlayer : ModPlayer
    {
        // Duration of Blood Rage in ticks
        public int BloodRageTimer;

        // Is the effect currently active?
        public bool BloodRageActive => BloodRageTimer > 0;

        // Call this to activate the effect
        public void ActivateBloodRage(int duration)
        {
            BloodRageTimer = duration; // e.g., 720 ticks = 12 seconds
        }

        public override void ResetEffects()
        {
            if (BloodRageTimer > 0)
                BloodRageTimer--; // Countdown each tick
        }

        public override void PostUpdate()
        {
            if (!BloodRageActive) return;
            // Only active if Butcherers is being held
            if (Player.HeldItem.type == ModContent.ItemType<Items.Weapons.Ranged.ButchersBloodmaker>())
            {
                // Particle effect (same as before)
                if (Main.rand.NextBool(4))
                {
                    Vector2 offset = new Vector2(Main.rand.NextFloat(-15f, 15f), Main.rand.NextFloat(-20f, 20f));
                    Vector2 pos = Player.Center + offset;

                    int dustIndex = Dust.NewDust(pos, 0, 0, DustID.Blood, 0f, 0f, 0, default, Main.rand.NextFloat(1.5f, 2f));
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].velocity = offset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.2f, 0.5f);
                    Main.dust[dustIndex].scale *= Main.rand.NextFloat(1.1f, 1.5f);
                    Main.dust[dustIndex].alpha = 0;
                }
            }
        }
    }

}
