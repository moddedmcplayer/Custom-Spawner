namespace CustomSpawner.Patches
{
    using HarmonyLib;
    using PlayerRoles.FirstPersonControl;

    [HarmonyPatch(typeof(FpcMouseLook), nameof(FpcMouseLook.UpdateRotation))]
    public class FpcRotationPatch
    {
        public static bool Prefix(FpcMouseLook __instance)
            => !__instance._hub.IsDummy();
    }
}