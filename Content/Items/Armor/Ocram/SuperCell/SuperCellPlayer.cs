using CalamityMod;
using InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.Eclipse;
using Terraria;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Items.Armor.Ocram.SuperCell
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
    public class SuperCellPlayer : ModPlayer
    {
        public bool hasSuperCellGuardEquipped;
        public int superCellExtraWingTime;
        public float previousStealth;

        public override void ResetEffects()
        {
            hasSuperCellGuardEquipped = false;
            superCellExtraWingTime = 0;
        }

        // Called after armor/accessories have been processed
        public override void UpdateEquips()
        {
            // Only apply if the player actually has some wing time to boost
            if (hasSuperCellGuardEquipped && Player.wingTimeMax > 0)
            {
                // Add 15% extra wing time (rounded)
                superCellExtraWingTime = (int)(Player.wingTimeMax * 0.15f);
                Player.wingTimeMax += superCellExtraWingTime;
            }
        }

        public override void PreUpdate()
        {
            previousStealth = Player.Calamity().rogueStealth;
        }

        public override void PostUpdate()
        {
            if (hasSuperCellGuardEquipped && Player.HeldItem.CountsAsClass<RogueDamageClass>())
            {
                var calPlayer = Player.Calamity();

                float current = calPlayer.rogueStealth;

                // Detect stealth consumption
                if (current < previousStealth)
                {
                    float consumed = (previousStealth - current) * 100;

                    // Convert 15% of consumed stealth into Void
                    float voidGain = consumed * 0.15f;

                    SOTSBonuses.RegainVoid(Player, voidGain);
                }
            }
        }
    }

}
