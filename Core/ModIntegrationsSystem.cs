using System;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core
{
    public class ModIntegrationsSystem : ModSystem
    {
        public static class Calamity
        {
            public static string Name = "CalamityMod";
            public static Mod Mod => ModLoader.GetMod(Name);
            public static bool Loaded => ModLoader.HasMod(Name);
        }

        public override void PostSetupContent()
        {
            SetupWeaponOutLite();
        }

        //See: https://github.com/Flashkirby/WeaponOutLite/blob/main/WeaponOutLite.SetupContent.cs for reference
        public static void SetupWeaponOutLite()
        {
            if (ModLoader.TryGetMod("WeaponOutLite", out Mod weaponOutLite))
            {
                if (!(bool)weaponOutLite.Call("RegisterLargeMelee", new int[] {
                    ModContent.ItemType<TwoPaths>(),
                })) { throw new ArgumentException("RegisterLargeMelee ModCall Failed"); }

                if (!(bool)weaponOutLite.Call("RegisterItemHoldPose", ModContent.ItemType<TwoPaths>(), "StanceBowInHand")
                ) { throw new ArgumentException("RegisterItemHoldPose ModCall Failed"); }
            }
        }
    }
}
