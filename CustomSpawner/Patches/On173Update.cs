using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using NorthwoodLib.Pools;

namespace CustomSpawner.Patches
{
    using Mirror;
    using PlayerRoles;
    using PlayerRoles.PlayableScps.Scp173;

    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObserver))]
    internal static class Scp173Update
    {
        public static bool Prefix(Scp173ObserversTracker __instance, ref int __result, ReferenceHub targetHub)
        {
            if (targetHub.IsDummy())
            {
                __result = !__instance.Observers.Remove(targetHub) ? 0 : -1;
                return false;
            }
            return true;
        }
    }
}