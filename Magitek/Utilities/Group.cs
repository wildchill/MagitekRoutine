﻿using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using Magitek.Extensions;
using Magitek.Models.Debugging;
using Magitek.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using BaseSettings = Magitek.Models.Account.BaseSettings;

namespace Magitek.Utilities
{
    internal static class Group
    {
        public static IEnumerable<Character> AllianceMembers
        {
            get
            {
                return GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(r => r.Type == GameObjectType.Pc && r.IsTargetable && r.InLineOfSight());
            }
        }

        public static IEnumerable<Character> Pets
        {
            get
            {
                return GameObjectManager.GetObjectsByNPCIds<GameObject>(PetIds).Where(r => r.IsTargetable && r.InLineOfSight() && r.Distance(Core.Me) <= 30).Select(r => r as Character);
            }
        }

        private static readonly uint[] PetIds = { 1398, 1399, 1400, 1401, 1402, 1403, 1404, 5478 };

        public static void UpdateAllies(Action extensions = null)
        {
            var CastableAllies = new List<Character>();
            DeadAllies.Clear();
            CastableTanks.Clear();
            CastableHealers.Clear();
            CastableDps.Clear();
            CastableAlliesWithin30.Clear();
            CastableAlliesWithin25.Clear();
            CastableAlliesWithin20.Clear();
            CastableAlliesWithin15.Clear();
            CastableAlliesWithin12.Clear();
            CastableAlliesWithin10.Clear();

            if (!Globals.InParty)
            {
                if (Globals.InGcInstance)
                {
                    CastableAllies.Add(Core.Me);

                    foreach (var ally in GameObjectManager.GetObjectsOfType<BattleCharacter>().Where(r => !r.CanAttack))
                    {
                        //if (!ally.IsTargetable || !ally.InLineOfSight() || ally.Icon == PlayerIcon.Viewing_Cutscene)
                        //TODO: This is a temporary fix for wrong PlayerIcon Enum: 15 = Viewing_Cutscene
                        if (!ally.IsTargetable || !ally.InLineOfSight() || ally.Icon == (PlayerIcon)15)
                            continue;

                        if (BaseSettings.Instance.PartyMemberAuraHistory)
                        {
                            UpdatePartyMemberHistory(ally);
                        }

                        if (ally.CurrentHealth <= 0 || ally.IsDead)
                        {
                            DeadAllies.Add(ally);
                            continue;
                        }

                        CastableAllies.Add(ally);
                    }
                }
            }

            foreach (var ally in PartyManager.AllMembers.Select(r => r.BattleCharacter))
            {
                if (ally == null)
                    continue;

                if (BaseSettings.Instance.DebugHealingLists == true)
                {
                    Logger.WriteInfo($@"[Debug] PartyManager {ally.Name} is a valid Party Member in PartyManager.");
                }

                //if (!ally.IsTargetable || !ally.InLineOfSight() || ally.Icon == PlayerIcon.Viewing_Cutscene)
                //TODO: This is a temporary fix for wrong PlayerIcon Enum: 15 = Viewing_Cutscene
                if (!ally.IsTargetable || !ally.InLineOfSight() || ally.Icon == (PlayerIcon)15)
                    continue;

                if (BaseSettings.Instance.PartyMemberAuraHistory)
                {
                    UpdatePartyMemberHistory(ally);
                }

                if (ally.CurrentHealth <= 0 || ally.IsDead)
                {
                    DeadAllies.Add(ally);
                    continue;
                }

                if (WorldManager.InPvP)
                {
                    if (ally.HasAura(Auras.MountedPvp))
                        continue;
                }

                CastableAllies.Add(ally);
            }

            foreach (var ally in CastableAllies.OrderBy(a => a.GetHealingWeight()))
            {
                var distance = ally.Distance(Core.Me);

                if (ally.IsTank())
                    CastableTanks.Add(ally);
                if (ally.IsHealer())
                    CastableHealers.Add(ally);
                if (ally.IsDps())
                    CastableDps.Add(ally);

                if (distance <= 30) { CastableAlliesWithin30.Add(ally); }
                if (distance <= 25) { CastableAlliesWithin25.Add(ally); }
                if (distance <= 20) { CastableAlliesWithin20.Add(ally); }
                if (distance <= 15) { CastableAlliesWithin15.Add(ally); }
                if (distance <= 12) { CastableAlliesWithin12.Add(ally); }
                if (distance <= 10) { CastableAlliesWithin10.Add(ally); }
            }

            extensions?.Invoke();
        }

        private static void UpdatePartyMemberHistory(Character unit)
        {
            foreach (var aura in unit.CharacterAuras)
            {
                if (Debug.Instance.PartyMemberAuras.ContainsKey(aura.Id))
                    continue;

                var newAura = new TargetAuraInfo(aura.Name, aura.Id, unit.Name);
                Logger.WriteInfo($@"[Debug] Adding {aura.Name} To Party Member Aura History");
                Debug.Instance.PartyMemberAuras.Add(aura.Id, newAura);
            }
        }

        public static readonly List<Character> DeadAllies = new List<Character>();
        public static readonly List<Character> CastableTanks = new List<Character>();
        public static readonly List<Character> CastableHealers = new List<Character>();
        public static readonly List<Character> CastableDps = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin30 = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin25 = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin20 = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin15 = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin12 = new List<Character>();
        public static readonly List<Character> CastableAlliesWithin10 = new List<Character>();
    }
}
