using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs.Healer;
using ThoriumMod.Projectiles.Healer;
using ThoriumMod.Utilities;

namespace InfernalEclipseWeaponsDLC.Utilities
{
    public static class HealerHelper
    {
        public static bool HealPlayerLocal(Player healer, Player target, int healAmount = 1, int recoveryTime = 0, bool healEffects = true, Action<Player> extraEffects = null, Func<Player, bool> canHealTarget = null)
        {
            if (canHealTarget != null && !canHealTarget(target))
                return false;

            if (recoveryTime > 0)
            {
                target.GetThoriumPlayer().SetLifeRecoveryEffect(LifeRecoveryEffectType.Generic, (short)recoveryTime, request: true);
                target.AddBuff(ModContent.BuffType<QuickRecovery>(), recoveryTime, true, false);
            }

            if (healEffects)
                OnHealEffects(healer, target);

            extraEffects?.Invoke(target);
            healAmount += healer.GetThoriumPlayer().healBonus;

            target.statLife += healAmount;
            if (target.statLife > target.statLifeMax2)
                target.statLife = target.statLifeMax2;

            target.HealEffect(healAmount, true);
            target.GetThoriumPlayer().mostRecentHeal = healAmount;
            target.GetThoriumPlayer().mostRecentHealer = healer.whoAmI;

            NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, target.whoAmI);
            healer.ApplyInteractionNearbyNPCs();

            return true;
        }

        public static bool HealPlayer(Player healer, Player target, int healAmount = 1, int recoveryTime = 0, bool healEffects = true, Action<Player> extraEffects = null, Func<Player, bool> canHealTarget = null)
        {
            if (canHealTarget != null && !canHealTarget(target))
                return false;

            if (recoveryTime > 0)
            {
                target.GetThoriumPlayer().SetLifeRecoveryEffect(LifeRecoveryEffectType.Generic, (short)recoveryTime, request: true);
                target.AddBuff(ModContent.BuffType<QuickRecovery>(), recoveryTime, true, false);
            }

            if (healEffects)
                OnHealEffects(healer, target);

            extraEffects?.Invoke(target);

            healAmount += healer.GetThoriumPlayer().healBonus;

            target.HealLife(healAmount, healer);
            target.GetThoriumPlayer().mostRecentHeal = healAmount;
            target.GetThoriumPlayer().mostRecentHealer = healer.whoAmI;

            healer.ApplyInteractionNearbyNPCs();

            return true;
        }

        public static void OnHealEffects(Player healer, Player target)
        {
            ThoriumPlayer thoriumHealer = healer.GetThoriumPlayer();
            ThoriumPlayer thoriumTarget = target.GetThoriumPlayer();
            bool selfHeal = healer.whoAmI == target.whoAmI;

            // Accessories / Set effects
            if (thoriumHealer.accForgottenCrossNecklace)
                target.AddBuff(ModContent.BuffType<ForgottenCrossNecklaceBuff>(), 900);
            if (thoriumHealer.setBlooming)
                target.AddBuff(ModContent.BuffType<BloomingSetBuff>(), 600);
            if (thoriumHealer.setLifeBinder)
                target.AddBuff(ModContent.BuffType<LifeBinderSetBuff>(), 600);
            if (thoriumHealer.accVerdantOrnament)
                target.AddBuff(ModContent.BuffType<VerdantOrnamentBuff>(), 300);
            if (thoriumHealer.buffDreamWeaversHoodDream)
                target.AddBuff(ModContent.BuffType<DreamWeaversHoodDreamAllyBuff>(), 60);

            // PvP / special effects
            if (!selfHeal)
            {
                if (thoriumHealer.honeyHeart && target.statLife <= healer.statLife)
                    target.AddPVPBuff(48, 300);

                ref int coralShieldCounter = ref thoriumHealer.setCoralShieldCounter;
                if (thoriumHealer.setCoral && coralShieldCounter > 0)
                {
                    thoriumHealer.HandleCoralSetTransfer(thoriumTarget, coralShieldCounter, request: true);
                    coralShieldCounter = 0;
                }

                if (thoriumHealer.accBeltoftheQuickResponse)
                    healer.AddBuff(ModContent.BuffType<BeltoftheQuickResponseBuff>(), 180);

                if (thoriumHealer.innerFlame.Active && thoriumHealer.LowestPlayer != healer.whoAmI && healer.whoAmI == Main.myPlayer)
                {
                    int p = Projectile.NewProjectile(healer.GetSource_Accessory(thoriumHealer.innerFlame.Item, null),
                        healer.Center.X, healer.Center.Y - 50f, 0f, 0f,
                        ModContent.ProjectileType<InnerFlamePro>(), 0, 0f, healer.whoAmI);
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, p);
                }

                if (thoriumHealer.accDewCollector.Active && healer.whoAmI == Main.myPlayer)
                {
                    int p2 = Projectile.NewProjectile(healer.GetSource_Accessory(thoriumHealer.accDewCollector.Item, null),
                        target.Center.X, target.Center.Y,
                        Utils.NextFloat(Main.rand, -1f, 1f), Utils.NextFloat(Main.rand, -3f, -1f),
                        ModContent.ProjectileType<DewCollectorPro>(), 0, 0f, healer.whoAmI);
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, p2);
                }
            }

            // Life recovery effects
            if (thoriumHealer.aloePlant)
                thoriumTarget.SetLifeRecoveryEffect(LifeRecoveryEffectType.AloeLeaf, 600, request: true);
            if (thoriumHealer.medicalAcc && !thoriumTarget.OutOfCombat)
                thoriumTarget.SetLifeRecoveryEffect(LifeRecoveryEffectType.MedicalBag, 300, request: true);

            if (!selfHeal && thoriumHealer.equilibrium)
            {
                if (healer.statLife > target.statLife)
                    thoriumTarget.SetLifeRecoveryEffect(LifeRecoveryEffectType.Equalizer, 300, request: true);
                else
                    thoriumHealer.SetLifeRecoveryEffect(LifeRecoveryEffectType.Equalizer, 300, request: true);
            }
        }
    }
}
