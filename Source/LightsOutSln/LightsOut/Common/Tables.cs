﻿//************************************************
// Holds all of the common table operations
//************************************************

using LightsOut.Defs;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace LightsOut.Common
{
    [StaticConstructorOnStartup]
    public static class Tables
    {
        //****************************************
        // Does the hard work of disabling
        // a worktable
        //****************************************
        public static void DisableTable(ThingWithComps table)
        {
            DebugLogger.AssertFalse(table is null, "DisableTable called on a null table");
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null))
                Glowers.DisableGlower(glower);
            else
                Resources.SetConsumesResources(table, false);
        }

        //****************************************
        // Does the hard work of enabling
        // a worktable
        //****************************************
        public static void EnableTable(ThingWithComps table)
        {
            DebugLogger.AssertFalse(table is null, "EnableTable called on a null table");
            if (table is null) return;
            ThingComp glower = Glowers.GetGlower(table);
            if (!(glower is null))
                Glowers.EnableGlower(glower);
            else
                Resources.SetConsumesResources(table, true);
        }

        //****************************************
        // Check if a table has a comp on the
        // disallowed list
        //****************************************
        public static bool IsTable(ThingWithComps thing)
        {
            DebugLogger.AssertFalse(thing is null, "IsTable called on a null thing");
            if (MemoizedIsTable.ContainsKey(thing))
                return MemoizedIsTable[thing];

            if (thing is null)
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            if (HasBlacklistedTableComp(thing))
            {
                MemoizedIsTable.Add(thing, false);
                return false;
            }

            bool isTable = ((thing is Building_WorkTable || thing is Building_ResearchBench));
            MemoizedIsTable.Add(thing, isTable);
            return isTable;
        }

        //****************************************
        // Detects blacklisted comps on tables
        //
        // Used to be a list, but that has much
        // worse performance implications
        //****************************************
        public static bool HasBlacklistedTableComp(ThingWithComps thing)
        {
            if (thing.TryGetComp<CompFireOverlay>() is null)
            {
                return false;
            }
            return true;
        }

        //****************************************
        // Detects if a building is a television
        //****************************************
        public static bool IsTelevision(ThingWithComps thing)
        {
            return thing.def == CustomThingDefs.TubeTelevision
                || thing.def == CustomThingDefs.FlatscreenTelevision
                || thing.def == CustomThingDefs.MegascreenTelevision;
        }

        private static Dictionary<ThingWithComps, bool> MemoizedIsTable { get; } = new Dictionary<ThingWithComps, bool>();
    }
}