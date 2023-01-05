using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using NorthwoodLib.Pools;

namespace CustomSpawner.Patches
{
    using Mirror;
    using PlayerRoles.PlayableScps.Scp173;

    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.UpdateObservers))]
    internal static class Scp173Update
    {
        public static bool Prefix(Scp173ObserversTracker __instance)
        {
            if (!NetworkServer.active)
            {
                return false;
            }
            int num = __instance.CurrentObservers;
            int num2 = (__instance.SimulatedStare > 0f) ? 1 : 0;
            if (__instance._simulatedTargets != num2)
            {
                num += num2 - __instance._simulatedTargets;
                __instance._simulatedTargets = num2;
            }
            foreach (ReferenceHub targetHub in ReferenceHub.AllHubs)
            {
                if(DummiesManager.IsDummy(targetHub))
                    continue;
                num += __instance.UpdateObserver(targetHub);
            }
            __instance.CurrentObservers = num;
            if (__instance.Owner.isLocalPlayer)
            {
                return false;
            }
            __instance.ServerSendRpc(true);

            return false;
        }
    }
}