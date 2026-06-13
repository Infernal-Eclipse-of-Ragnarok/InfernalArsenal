using CalamityMod.DataStructures;
using CalamityMod.Systems.Collections;
using InfernalEclipseWeaponsDLC.Content.Buffs;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core
{
    public class InfernalArsenalSummonTagSystem : ModSystem
    {
        private struct SummonTagEntry
        {
            public Func<int> ItemType;

            public Func<int> BuffType;

            public Action<SummonTag> Setup;
        }

        public override void PostSetupContent()
        {
            SummonTagEntry[] array = new SummonTagEntry[1]
            {
                new SummonTagEntry
                {
                    ItemType = () => ModContent.ItemType<GrandThunderWhip>(),
                    BuffType = () => ModContent.BuffType<GrandThunderWhipTag>(),
                    Setup = delegate (SummonTag summonTag)
                    {
                        summonTag.FlatTagDamage = 3;
                        summonTag.AutoDrawTooltip = false;
                        summonTag.TagTexture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/GrandThunderWhip", (AssetRequestMode)1);
                    }
                }
            };

            for (int num = 0; num < array.Length; num++)
            {
                SummonTagEntry summonTagEntry = array[num];
                SummonTag tag = new SummonTag(summonTagEntry.ItemType());
                summonTagEntry.Setup?.Invoke(tag);
                int debuff = summonTagEntry.BuffType();
                if (CalamityBuffSets.SummonTagDebuff[debuff] == null)
                {
                    CalamityBuffSets.SummonTagDebuff[debuff] = tag;
                }
            }
        }


    }
}
