using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Content.Projectiles.BardPro.PocketConcert
{
    [JITWhenModsEnabled("ThoriumMod", "CalamityMod")]
    [ExtendsFromMod("ThoriumMod", "CalamityMod")]
    public sealed class PocketConcertAudioSystem : ModSystem
    {
        public static readonly SoundStyle Loop = new SoundStyle($"{nameof(InfernalEclipseWeaponsDLC)}/Assets/Effects/Sounds/PocketConcertLoop") with
        {
            Volume = 0.5f,
            IsLooped = true,
            MaxInstances = 1
        };

        private static SlotId Slot { get; set; } = SlotId.Invalid;

        private static float volume;

        private static float Volume
        {
            get => volume;
            set => volume = MathHelper.Clamp(value, 0f, 0.5f);
        }

        private static bool Active { get; set; }

        public static void Play(Vector2 position)
        {
            var success = SoundEngine.TryGetActiveSound(Slot, out var sound);

            if (success)
            {
                return;
            }

            Slot = SoundEngine.PlaySound(in Loop, position);

            Volume = 0f;
            Active = true;

            if (!success)
            {
                return;
            }

            sound.Volume = Volume;
        }

        public static void Stop()
        {
            if (!SoundEngine.TryGetActiveSound(Slot, out var sound))
            {
                return;
            }

            Active = false;
        }

        public override void PreUpdateWorld()
        {
            if (!SoundEngine.TryGetActiveSound(Slot, out var sound))
            {
                return;
            }

            if (Active)
            {
                Volume += 0.01f;
            }
            else
            {
                Volume -= 0.01f;

                if (Volume <= 0f)
                {
                    sound.Stop();

                    Slot = SlotId.Invalid;
                }
            }

            if (Main.hasFocus)
            {
                sound.Volume = Volume;

            }
            else
            {
                sound.Volume = 0f;
            }

        }
    }
}