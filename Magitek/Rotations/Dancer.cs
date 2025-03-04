﻿using ff14bot;
using ff14bot.Managers;
using Magitek.Extensions;
using Magitek.Logic;
using Magitek.Logic.Dancer;
using Magitek.Logic.Roles;
using Magitek.Models.Account;
using Magitek.Models.Dancer;
using Magitek.Utilities;
using DancerRoutine = Magitek.Utilities.Routines.Dancer;
using System.Linq;
using System.Threading.Tasks;

namespace Magitek.Rotations
{
    public static class Dancer
    {
        public static Task<bool> Rest()
        {
            var needRest = Core.Me.CurrentHealthPercent < 75 || Core.Me.CurrentManaPercent < 50;
            return Task.FromResult(needRest);
        }

        public static async Task<bool> PreCombatBuff()
        {
            if (Core.Me.IsCasting)
                return true;

            if (await Casting.TrackSpellCast())
                return true;

            await Casting.CheckForSuccessfulCast();

            //Openers.OpenerCheck();

            if (Core.Me.HasTarget && Core.Me.CurrentTarget.CanAttack)
            {
                return false;
            }

            if (BaseSettings.Instance.ActivePvpCombatRoutine)
                return await PvP();

            if (WorldManager.InSanctuary)
                return false;

            if (await Buff.DancePartner()) return true;

            return await PhysicalDps.Peloton(DancerSettings.Instance);
        }

        public static async Task<bool> Pull()
        {
            if (BotManager.Current.IsAutonomous)
            {
                if (Core.Me.HasTarget)
                {
                    Movement.NavigateToUnitLos(Core.Me.CurrentTarget, 20);
                }
            }

            return await Combat();
        }

        public static async Task<bool> Heal()
        {
            if (await Casting.TrackSpellCast()) return true;
            await Casting.CheckForSuccessfulCast();

            if (await GambitLogic.Gambit()) return true;
            return false;
        }

        public static Task<bool> CombatBuff()
        {
            return Task.FromResult(false);
        }

        public static async Task<bool> Combat()
        {
            if (BaseSettings.Instance.ActivePvpCombatRoutine)
                return await PvP();

            if (BotManager.Current.IsAutonomous)
            {
                if (Core.Me.HasTarget)
                    Movement.NavigateToUnitLos(Core.Me.CurrentTarget, 20);
            }

            if (!SpellQueueLogic.SpellQueue.Any())
                SpellQueueLogic.InSpellQueue = false;

            if (SpellQueueLogic.SpellQueue.Any())
            {
                if (await SpellQueueLogic.SpellQueueMethod())
                    return true;
            }

            if (!Core.Me.HasTarget || !Core.Me.CurrentTarget.ThoroughCanAttack())
                return false;

            if (await CustomOpenerLogic.Opener()) return true;
            if (await GambitLogic.Gambit()) return true;

            //LimitBreak
            if (Aoe.ForceLimitBreak()) return true;

            if (await Buff.DancePartner()) return true;

            if (await Aoe.StarfallDance()) return true;
            if (await Dances.Tillana()) return true;
            if (await Dances.StandardStep()) return true;
            if (await Dances.TechnicalStep()) return true;
            if (await Dances.DanceStep()) return true;

            if (Core.Me.HasAura(Auras.StandardStep) || Core.Me.HasAura(Auras.TechnicalStep))
                return false;

            if (DancerRoutine.GlobalCooldown.CanWeave())
            {
                if (await PhysicalDps.Interrupt(DancerSettings.Instance)) return true;
                if (await PhysicalDps.SecondWind(DancerSettings.Instance)) return true;
                if (await Buff.UsePotion()) return true;


                if (await Buff.CuringWaltz()) return true;
                if (await Buff.PreTechnicalDevilment()) return true;
                if (await Aoe.FanDance4()) return true;
                if (await Aoe.FanDance3()) return true;
                if (await Aoe.FanDance2()) return true;
                if (await SingleTarget.FanDance()) return true;
                if (await Buff.Devilment()) return true;
                if (await Buff.Flourish()) return true;
                if (await Buff.Improvisation()) return true;
            }

            if (await Aoe.SaberDance()) return true;
            if (await Aoe.Bloodshower()) return true;
            if (await Aoe.RisingWindmill()) return true;
            if (await SingleTarget.Fountainfall()) return true;
            if (await SingleTarget.ReverseCascade()) return true;
            if (await Aoe.Bladeshower()) return true;
            if (await Aoe.Windmill()) return true;
            if (await SingleTarget.Fountain()) return true;
            return await SingleTarget.Cascade();
        }

        public static async Task<bool> PvP()
        {
            if (!BaseSettings.Instance.ActivePvpCombatRoutine)
                return await Combat();

            //Partner
            if (await Pvp.ClosedPosition()) return true;

            if (!Core.Me.HasTarget || !Core.Me.CurrentTarget.ThoroughCanAttack())
                return false;

            // Utilities
            if (await PhysicalDps.Guard(DancerSettings.Instance)) return true;
            if (await PhysicalDps.Purify(DancerSettings.Instance)) return true;
            if (await PhysicalDps.Recuperate(DancerSettings.Instance)) return true;
            if (await Pvp.CuringWaltz()) return true;

            //LB
            if (await Pvp.Contradance()) return true;

            if (!PhysicalDps.GuardCheck())
            {
                //oGCD
                if (await Pvp.HoningDance()) return true;
                if (await Pvp.FanDance()) return true;
                if (await Pvp.StarfallDance()) return true;
            }

            //Combo
            if (await Pvp.SaberDance()) return true;
            if (await Pvp.FountainFall()) return true;
            if (await Pvp.ReverseCascade()) return true;
            if (await Pvp.Fountain()) return true;
            
            return await Pvp.Cascade();
        }
    }
}
