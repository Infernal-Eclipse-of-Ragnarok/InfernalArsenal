using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Bard;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Hybrid;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Magic;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Healer.Melee.Scythes;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Magic;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Melee.Void;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Ranged;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Rogue;
using InfernalEclipseWeaponsDLC.Content.Items.Weapons.Summoner.Whips;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace InfernalEclipseWeaponsDLC.Core.GlobalNPCs
{
    public class NPCDropChanges : GlobalNPC
    {
        private bool droppedFromWaterContact = false;
        public override bool InstancePerEntity => true;

        public override void PostAI(NPC npc)
        {
            if (npc.type == NPCID.BlazingWheel && npc.active && !droppedFromWaterContact && WeaponConfig.Instance.GitGudWeapon)
            {
                if (Collision.WetCollision(npc.position, npc.width, npc.height))
                {
                    droppedFromWaterContact = true;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<ObsidianSickle>());
                }
            }
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.GoblinSummoner && WeaponConfig.Instance.AIGenedWeapons)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShadowflameAxe>(), 10));
            }

            if (npc.type == NPCID.WallofFlesh)
            {
                npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<ForeverHungry>(), 3));

                npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<BottleOfSouls>(), 3));
            }

            ModLoader.TryGetMod("CalamityMod", out Mod calamity);
            ModLoader.TryGetMod("Consolaria", out Mod console);

            if (calamity != null)
            {
                int FindNPC(string name)
                {
                    return calamity.Find<ModNPC>(name).Type;
                }

                if (npc.type == FindNPC("BrimstoneElemental"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<BrimstoneHarp>(), 3));

                    if (console == null && WeaponConfig.Instance.AIGenedWeapons)
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StormCrossbow>(), 10));
                }

                if (npc.type == FindNPC("DesertScourgeHead"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<SandSlasher>(), 3));
                }

                if (npc.type == FindNPC("AquaticScourgeHead"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<SulphuricShanty>(), 7));
                }

                if (npc.type == FindNPC("AstrumDeusHead"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<DeusFlute>(), 3));
                }

                if (npc.type == FindNPC("RavagerBody"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<NecroticChorus>(), 3));
                }

                if (npc.type == FindNPC("Signus"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<TheParallel>(), 3));
                }

                if (npc.type == FindNPC("Dragonfolly"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<BirbSaxophone>(), 3));
                }

                if (npc.type == FindNPC("DevourerofGodsHead"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<DeathsWhisper>(), 3));
                }

                if (npc.type == FindNPC("OldDuke"))
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<CorrodedCane>(), 4));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<DukeSynth>(), 4));
                }

                if (npc.type == FindNPC("SupremeCataclysm"))
                {
                    npcLoot.Add(ItemDropRule.Common(ModLoader.HasMod("SOTS") ? Mod.Find<ModItem>("CataclysmicGauntletVoid").Type : ModContent.ItemType<CataclysmicGauntlet>(), 10));
                }

                if (npc.type == FindNPC("SupremeCatastrophe"))
                {
                    npcLoot.Add(ItemDropRule.Common(ModLoader.HasMod("SOTS") ? Mod.Find<ModItem>("CatastrophicLongbladeVoid").Type : ModContent.ItemType<CatastrophicLongblade>(), 10));
                }

                if (npc.type == FindNPC("PrimordialWyrmHead"))
                {
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AbyssalAnchor>()));
                }
            }

            if (console != null)
            {
                if (npc.type == console.Find<ModNPC>("ArchWyvernHead").Type && WeaponConfig.Instance.AIGenedWeapons)
                {
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<StormCrossbow>(), 10));
                }

                if (npc.type == console.Find<ModNPC>("Ocram").Type)
                {
                    //npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<OcramKnife>(), 4));
                    //npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<TheBlight>(), 4));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<Legacy>(), 4));
                }
            }

            if (ModLoader.TryGetMod("SOTS", out Mod sots))
            {
                if (npc.type == sots.Find<ModNPC>("Glowmoth").Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<MothwingDagger>(), 5));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<IlluminantCross>(), 5));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<GlowstringBiwa>(), 5));
                }
            }

            if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
            {
                if (npc.type == thorium.Find<ModNPC>("TheGrandThunderBird").Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<GrandThunderWhip>(), 4));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<StormCarver>(), 4));
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<GrandAmplifier>(), 4));
                }

                if (npc.type == thorium.Find<ModNPC>("Lich").Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<ListoftheDamned>(), 5));
                }
            }

            if (ModLoader.TryGetMod("FargowiltasSouls", out Mod souls) && !ModLoader.HasMod("YharimEX"))
            {
                if (npc.type == souls.Find<ModNPC>("MutantBoss").Type)
                {
                    npcLoot.Add(ItemDropRule.ByCondition(new Conditions.NotExpert(), ModContent.ItemType<AuricBrimfireCrosier>(), 1));
                }
            }
        }
    }
}
