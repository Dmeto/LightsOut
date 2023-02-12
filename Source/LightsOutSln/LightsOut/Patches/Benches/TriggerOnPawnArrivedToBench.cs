﻿using HarmonyLib;
using LightsOut.Common;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace LightsOut.Patches.Benches
{
    /// <summary>
    /// Detects when a Pawn gets to a bench to do work
    /// </summary>
    [HarmonyPatch(typeof(JobDriver))]
    [HarmonyPatch(nameof(JobDriver.Notify_PatherArrived))]
    public class TriggerOnPawnArrivedToBench
    {
        /// <summary>
        /// Check if a Pawn is arriving at an applicable bench or building
        /// </summary>
        /// <param name="__instance">The <see cref="JobDriver"/> that is arriving at its destination</param>
        public static void Prefix(JobDriver __instance)
        {
            if (__instance is null) return;

            Pawn pawn = __instance.GetActor();

            if (__instance is JobDriver_DoBill billDriver)
                PawnIsAtTable(billDriver, pawn);
            else if (__instance is JobDriver_Research researchDriver)
                PawnIsAtResearchBench(researchDriver, pawn);
            else if (__instance is JobDriver_OperateDeepDrill drillDriver)
                PawnIsAtDeepDrill(drillDriver, pawn);
            else if (__instance.job?.GetTarget(TargetIndex.A).Thing is ThingWithComps tv && Tables.IsTelevision(tv))
                PawnIsAtTelevision(__instance, tv, pawn);
            else if (__instance is JobDriver_EnterBuilding enterDriver)
                PawnIsAtEnterable(enterDriver, pawn);
        }

        private static void PawnIsAtEnterable(JobDriver_EnterBuilding driver, Pawn pawn)
        {
            var target = driver.job.GetTarget(TargetIndex.A).Thing as Building_Enterable;
            if (pawn.Position == target.InteractionCell && Enterables.IsEnterable(target))
            {
                Tables.EnableTable(target);
                driver.AddFinishAction(() =>
                {
                    if (!Enterables.Occupied(target))
                    {
                        Tables.DisableTable(target);
                    }
                });
            }
        }

        /// <summary>
        /// Check if a Pawn is at an applicable table
        /// </summary>
        /// <param name="jobDriver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the table</param>
        private static void PawnIsAtTable(JobDriver_DoBill jobDriver, Pawn pawn)
        {
            IBillGiver giver = jobDriver.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
            if(giver is Building_WorkTable table)
            {
                if (!Tables.IsTable(table)) return;

                if (pawn.Position == table.InteractionCell)
                    ActivateBench(jobDriver, table);
            }
        }

        /// <summary>
        /// Check if a Pawn is at an applicable research bench
        /// </summary>
        /// <param name="researchDriver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the research bench</param>
        private static void PawnIsAtResearchBench(JobDriver_Research researchDriver, Pawn pawn)
        {
            if (researchDriver.job.targetA.Thing is Building_ResearchBench bench)
            {
                if(pawn.Position == bench.InteractionCell)
                    ActivateBench(researchDriver, bench);
            }
        }

        /// <summary>
        /// Check if a pawn is at an applicable deep drill
        /// </summary>
        /// <param name="drillDriver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the deep drill</param>
        private static void PawnIsAtDeepDrill(JobDriver_OperateDeepDrill drillDriver, Pawn pawn)
        {
            if (drillDriver.job.targetA.Thing is Building building)
            {
                if (pawn.Position == building.InteractionCell)
                    ActivateBench(drillDriver, building);
            }
        }

        /// <summary>
        /// Check if a Pawn is at a television
        /// </summary>
        /// <param name="driver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="pawn">The pawn arriving to the television</param>
        private static void PawnIsAtTelevision(JobDriver driver, ThingWithComps tv, Pawn pawn)
        {
            if (tv is null || pawn is null)
                return;

            IEnumerable<IntVec3> watchArea = WatchBuildingUtility.CalculateWatchCells(tv.def, tv.Position, tv.Rotation, tv.Map);

            Tables.EnableTable(tv);

            driver.AddFinishAction(() =>
            {
                if (!Tables.IsAnyoneElseWatching(tv, pawn))
                    Tables.DisableTable(tv);
            });
        }

        /// <summary>
        /// Activate a bench and set it to deactivate when the Pawn finishes
        /// </summary>
        /// <param name="driver">The <see cref="JobDriver"/> that set the target</param>
        /// <param name="table">The building that is being turned on or off</param>
        private static void ActivateBench(JobDriver driver, ThingWithComps table)
        {
            // activate the bench
            Tables.EnableTable(table);
            //ModResources.SetConsumesPower(powerTrader, true);
            // set the bench to turn off after the job is complete
            driver.AddFinishAction(() =>
            {
                Tables.DisableTable(table);
            });
        }
    }
}