using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static HarmonyLib.AccessTools;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using UnityEngine;

namespace CustomSpawner.Patches
{
    using System;
    using Mirror;
    using Utils.Networking;

    [HarmonyPatch(typeof(SyncedStatBase), nameof(SyncedStatBase.Update))]
    internal static class StatBaseUpdate
    {
        public static bool Prefix(SyncedStatBase __instance)
        {
            __instance.Update();
            if (!NetworkServer.active || !__instance._valueDirty)
                return false;
            new SyncedStatMessages.StatMessage()
            {
                Stat = __instance,
                SyncedValue = __instance.CurValue
            }.SendToHubsConditionally<SyncedStatMessages.StatMessage>((hub) => (__instance.CanReceive(hub) && !__instance.Hub.IsDummy()));
            __instance._lastSent = __instance.CurValue;
            __instance._valueDirty = false;
            return false;
        }
    }
}