using CalamityMod.DataStructures;
using CalamityMod.Systems.Collections;
using InfernalEclipseWeaponsDLC.Content.Buffs.Summoner.Tag;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Whips;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core
{
    [JITWhenModsEnabled("CalamityMod")]
    [ExtendsFromMod("CalamityMod")]
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
            SummonTagEntry[] array =
            {
                new SummonTagEntry
                {
                    ItemType = () => ModContent.ItemType<GrandThunderWhip>(),
                    BuffType = () => ModContent.BuffType<GrandThunderWhipTag>(),
                    Setup = delegate (SummonTag summonTag)
                    {
                        summonTag.FlatTagDamage = 3;
                        summonTag.AutoDrawTooltip = false;
                        summonTag.TagTexture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/Whips/GrandThunderWhip", (AssetRequestMode)1);
                    }
                },

                new SummonTagEntry
                {
                    ItemType = () => ModContent.ItemType<Deoxyribonunoboko>(),
                    BuffType = () => ModContent.BuffType<DeoxyribonunobokoWhipTagDefault>(),
                    Setup = delegate (SummonTag summonTag)
                    {
                        summonTag.FlatTagDamage = 12;
                        summonTag.TagCritChance = 15;
                        summonTag.AutoDrawTooltip = false;
                        summonTag.TagTexture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/Whips/Deoxyribonunoboko", (AssetRequestMode)1);
                    }
                },

                new SummonTagEntry
                {
                    ItemType = () => ModContent.ItemType<Deoxyribonunoboko>(),
                    BuffType = () => ModContent.BuffType<DeoxyribonunobokoWhipTagStrong>(),
                    Setup = delegate (SummonTag summonTag)
                    {
                        summonTag.FlatTagDamage = 20;
                        summonTag.TagCritChance = 15;
                        summonTag.AutoDrawTooltip = false;
                        summonTag.TagTexture = ModContent.Request<Texture2D>("InfernalEclipseWeaponsDLC/Content/Items/Weapons/Summoner/Whips/Deoxyribonunoboko", (AssetRequestMode)1);
                    }
                },
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
