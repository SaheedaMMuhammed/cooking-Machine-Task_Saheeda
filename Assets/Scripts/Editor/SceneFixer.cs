using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ChefMachine.Core;
using ChefMachine.Stations;
using ChefMachine.Orders;

public static class SceneFixer
{
    public static void FixScene()
    {
        string scenePath = "Assets/Scenes/Kitchen.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Remove all invalid InteractionZones (the ones on UI elements)
        InteractionZone[] allZones = Object.FindObjectsByType<InteractionZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var zone in allZones)
        {
            if (zone.GetComponent<RectTransform>() != null)
            {
                Debug.Log($"Removing invalid InteractionZone on {zone.gameObject.name}");
                Object.DestroyImmediate(zone, true);
            }
        }

        // Fix ChopTable
        var chopTables = Object.FindObjectsByType<ChopTable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var ct in chopTables)
        {
            EnsureInteractionZone(ct, new Vector3(0, 0, -1.25f), new Vector3(2, 2, 2));
        }

        // Fix Stove
        var stoves = Object.FindObjectsByType<Stove>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in stoves)
        {
            EnsureInteractionZone(s, new Vector3(0, 0, -1.25f), new Vector3(3, 2, 2));
        }

        // Fix CustomerWindows
        var windows = Object.FindObjectsByType<CustomerWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var w in windows)
        {
            EnsureInteractionZone(w, new Vector3(0, 0, -1.5f), new Vector3(3, 2, 2));
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("SceneFixer: Saved fixed Kitchen.unity");
    }

    private static void EnsureInteractionZone(MonoBehaviour station, Vector3 localCenter, Vector3 size)
    {
        InteractionZone existing = station.GetComponentInChildren<InteractionZone>();
        if (existing != null)
        {
            Debug.Log($"Station {station.name} already has a valid InteractionZone on {existing.gameObject.name}");
            return;
        }

        GameObject zoneGo = new GameObject("InteractionZone");
        zoneGo.transform.SetParent(station.transform, false);
        zoneGo.transform.localPosition = Vector3.zero;
        zoneGo.transform.localRotation = Quaternion.identity;

        BoxCollider col = zoneGo.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = localCenter;
        col.size = size;
        
        // Put on Default layer or a specific Interactable layer if it exists
        zoneGo.layer = LayerMask.NameToLayer("Default"); // PlayerInteractor uses ~0 (All layers)

        InteractionZone iz = zoneGo.AddComponent<InteractionZone>();
        var so = new SerializedObject(iz);
        so.FindProperty("station").objectReferenceValue = station;
        so.ApplyModifiedProperties();

        Debug.Log($"Created new InteractionZone for {station.name}");
    }
}
