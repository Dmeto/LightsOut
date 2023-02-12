﻿using System;
using System.Collections.Generic;
using LightsOut.Common;
using RimWorld;
using Verse;

namespace LightsOut.Patches.ModCompatibility.Biotech
{
    /// <summary>
    /// Support for the wastepack atomizer: will not draw power when not atomizing waste
    /// </summary>
    public class PatchWasteAtomiser : ICompatibilityPatchComponent<Building_WastepackAtomizer>
    {
        public override string ComponentName => "Patch waste atomiser";
        public override IEnumerable<PatchInfo> GetPatches(Type type)
        {
            var patches = new List<PatchInfo>
            {
                new PatchInfo
                {
                    method = GetMethod<CompAtomizer>(nameof(CompAtomizer.Notify_MaterialsAdded)),
                    patch = GetMethod<PatchWasteAtomiser>(nameof(UpdateWorking)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<CompAtomizer>("DoAtomize"),
                    patch = GetMethod<PatchWasteAtomiser>(nameof(UpdateWorking)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<CompAtomizer>("EjectContents"),
                    patch = GetMethod<PatchWasteAtomiser>(nameof(UpdateWorking)),
                    patchType = PatchType.Postfix,
                },
                new PatchInfo
                {
                    method = GetMethod<CompAtomizer>("PostSpawnSetup"),
                    patch = GetMethod<PatchWasteAtomiser>(nameof(UpdateWorking)),
                    patchType = PatchType.Postfix,
                }
            };
            return patches;
        }

        /// <summary>
        /// Update the working state of the atomizer
        /// </summary>
        /// <param name="__instance"></param>
        private static void UpdateWorking(CompAtomizer __instance)
        {
            var atomizer = __instance;
            if (!(atomizer.parent is Building_WastepackAtomizer wasteatomizer))
            {
                return;
            }

            var working = !atomizer.Empty;
            if (working)
            {
                Tables.EnableTable(wasteatomizer);
            }
            else
            {
                Tables.DisableTable(wasteatomizer);
            }
        }
    }
}