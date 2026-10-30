using Terraria;
using Terraria.ID;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    public static class PlayerExtensions
    {
        public static bool HasAltFunctionUse(this Player player)
        {
            return player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed;
        }

        public static bool HasMaxSentries(this Player player)
        {
            int sentryCount = 0;

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.owner == player.whoAmI && projectile.sentry)
                    sentryCount++;
            }

            return sentryCount >= player.maxTurrets;
        }
    }
}
