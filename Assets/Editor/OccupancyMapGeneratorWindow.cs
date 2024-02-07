using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using PNM;
using System.Collections.Generic;
using System.IO;

[Serializable]
struct MapYaml
{
    public string name;
    public string image;
    public string resolution;
    public string origin;
    public string occupied_thresh;
    public string free_thresh;
}

public class OccupancyMapGeneratorWindow : EditorWindow
{
    Vector2 start = new Vector2(-17, -17);
    Vector2 end = new Vector2(17, 17);
    Vector2 origin = new Vector2(0, 0);
    float resolution = 0.1f;
    float testHeight = 0.30f;
    float testOffset = 0.25f;
    float toleranceMargin = 0.01f;

    [MenuItem("Assets/Generate Occupancy Map", true)]
    private static bool GenerateOccupancyMapValidation()
    {
        var selected = Selection.activeObject;
        Debug.Log(selected.GetType());
        Debug.Log(typeof(Scene).Name);
        return selected.GetType() == typeof(UnityEditor.SceneAsset);
    }

    [MenuItem("Assets/Generate Occupancy Map")]
    private static void ShowOccupancyMapWindow()
    {
        var win = GetWindow<OccupancyMapGeneratorWindow>("Occupancy Map Generator", true);
        win.minSize = new Vector2(300, 200);
        win.maxSize = new Vector2(900, 600);
    }

    public void GenerateOccupancyMap()
    {
        var selected = Selection.assetGUIDs[0];
        var activeScenePath = EditorSceneManager.GetActiveScene().path;
        var desiredScenePath = AssetDatabase.GUIDToAssetPath(selected);
        Scene scene;
        if (activeScenePath != desiredScenePath)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            scene = EditorSceneManager.OpenScene(desiredScenePath);
        }
        else
        {
            scene = SceneManager.GetActiveScene();
        }

        float width = end.x - start.x;
        float height = end.y - start.y;
        int pixelWidth = (int)(width / resolution);
        int pixelHeight = (int)(height / resolution);
        byte[] occupancyMap = new byte[pixelWidth * pixelHeight];

        for (int i = 0; i < occupancyMap.Length; i++)
        {
            //Set all to 0xCD which stands for unknown
            occupancyMap[i] = 0xCD;
        }

        //BFS
        (int x, int y) originPixel = ((int)((origin.x - start.x) / resolution), (int)((origin.y - start.y) / resolution));
        Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
        queue.Enqueue(originPixel);

        //we use free with 0xFE and occupied with 0x00 as that is the way in which it was saved in the existing pgm
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            //check if we are out of bounds
            if (current.x < 0 || current.x >= pixelWidth || current.y < 0 || current.y >= pixelHeight)
            {
                continue;
            }
            //check if we have already visited this pixel
            if (occupancyMap[current.x + current.y * pixelWidth] != 0xCD)
            {
                continue;
            }

            //check if this pixel is occupied
            Vector3 center = new Vector3(current.x * resolution + start.x, testHeight, current.y * resolution + start.y);
            Collider[] colliders = Physics.OverlapBox(center, new Vector3(resolution / 2 - toleranceMargin, testOffset, resolution / 2 - toleranceMargin));
            if (colliders.Length > 0)
            {
                occupancyMap[current.x + current.y * pixelWidth] = 0x00;
                continue;
            }

            //if we are here, then the pixel is free
            occupancyMap[current.x + current.y * pixelWidth] = 0xFE;

            //enqueue neighbours
            queue.Enqueue((current.x + 1, current.y));
            queue.Enqueue((current.x - 1, current.y));
            queue.Enqueue((current.x, current.y + 1));
            queue.Enqueue((current.x, current.y - 1));
        }

        string path = EditorUtility.SaveFolderPanel("Choose map folder", "", $"{scene.name}_unity");

        var pnm = new PortableAnyMap(MagicNumber.P5, pixelWidth, pixelHeight, 255);
        pnm.Bytes = occupancyMap;
        pnm.ToFile($"{path}/map.pgm", $"generated in Unity on {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} for scene {scene.name}");

        var yaml = new MapYaml
        {
            name = scene.name,
            image = "map.pgm",
            resolution = resolution.ToString(),
            origin = $"{origin.x} {origin.y} 0",
            occupied_thresh = "0.65",
            free_thresh = "0.196"
        };
        YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();
        File.WriteAllText($"{path}/map.yaml", serializer.Serialize(yaml));

        string mapWorld =
@"properties:
  velocity_iterations: 10
  position_iterations: 10
layers:
- name: static
  map: map.yaml
  color: [0, 1, 0, 1]";

        File.WriteAllText($"{path}/map.world.yaml", mapWorld);

        Debug.Log($"Map generated to {path}");
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Occupancy Map Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Map Size");
        start = EditorGUILayout.Vector2Field("Start", start);
        end = EditorGUILayout.Vector2Field("End", end);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Robo Origin");
        origin = EditorGUILayout.Vector2Field("Origin", origin);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Map Resolution");
        resolution = EditorGUILayout.FloatField("Resolution", resolution);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Test Box");
        testHeight = EditorGUILayout.FloatField("Height", testHeight);
        testOffset = EditorGUILayout.FloatField("Offset", testOffset);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Tolerance Margin");
        toleranceMargin = EditorGUILayout.FloatField("Margin", toleranceMargin);
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Occupancy Map"))
        {
            GenerateOccupancyMap();
        }

        EditorGUILayout.EndVertical();
    }


}
