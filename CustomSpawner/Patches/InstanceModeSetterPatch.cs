namespace CustomSpawner.Patches
{
    using HarmonyLib;

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.InstanceMode), MethodType.Setter)]
    public class InstanceModeSetterPatch
    {
        public static bool Prefix(CharacterClassManager __instance)
            => !__instance.Hub.IsDummy();
    }
}