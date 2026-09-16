using UnityEngine;
using UnityEditor;
using ChefMachine.Stations;
using ChefMachine.Player;

public static class SceneDumper
{
    [MenuItem("Tools/Dump Scene")]
    public static void DumpScene()
    {
        var chopTables = Object.FindObjectsByType<ChopTable>(FindObjectsSortMode.None);
        foreach (var ct in chopTables)
        {
            Debug.Log($"ChopTable on {ct.name}: prepDuration={ct.PrepDuration}, acceptedType={ct.GetType().GetField("acceptedType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(ct)}");
            var colliders = ct.GetComponents<Collider>();
            foreach (var col in colliders) Debug.Log($"  Collider: {col.GetType().Name}, isTrigger={col.isTrigger}");
            var zones = ct.GetComponentsInChildren<ChefMachine.Core.InteractionZone>();
            foreach (var zone in zones) Debug.Log($"  InteractionZone: on {zone.gameObject.name}");
            var allComps = ct.GetComponents<Component>();
            foreach (var c in allComps) Debug.Log($"  Component: {c.GetType().Name}");
        }

        var stoves = Object.FindObjectsByType<Stove>(FindObjectsSortMode.None);
        foreach (var s in stoves)
        {
            Debug.Log($"Stove on {s.name}");
            var allComps = s.GetComponents<Component>();
            foreach (var c in allComps) Debug.Log($"  Component: {c.GetType().Name}");
        }
    }
}
