using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC
{
    public class InfernalEclipseWeaponsDLC : Mod
    {
        internal static InfernalEclipseWeaponsDLC Instance;

        internal Mod calamity = null;
        internal Mod calamitybardhealer = null;
        internal Mod ragnarok = null;
        internal Mod thorium = null;

        public static ModKeybind AbsoluteTVRemotePause;

        public override void Load()
        {
            Instance = this;

            ModLoader.TryGetMod("CalamityMod", out calamity);
            ModLoader.TryGetMod("CalamityBardHealer", out calamitybardhealer);
            ModLoader.TryGetMod("RagnarokMod", out ragnarok);
            ModLoader.TryGetMod("ThoriumMod", out thorium);

            AbsoluteTVRemotePause = KeybindLoader.RegisterKeybind(
                this,
                "Absolute TV Remote Pause",
                "Mouse3"
            );
        }

        public override void Unload()
        {
            AbsoluteTVRemotePause = null;
        }
    }
}