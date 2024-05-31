using UnityEngine;
using System.IO;
using System.Collections;

// Dev Class for testing the spawning of assets from a single asset bundle.
// The assets should be built into an asset bundle which is located in Assets/StreamingAssets/AssetBundles/
// Define the assets and bundle by assigning the relative asset paths to assetNames and the asset bundle name to assetBundleName.
public class AddressableSpawn : MonoBehaviour
{
    private string[] assetNames = {
        "Assets/Resources_moved/Objects/Prefabs/Office/Box/Box.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Cabinet with drawings/Cabinet with drawings.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Chair Office/Chair Office.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Chair_1/Chair_1.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Coffee_table_2/Coffee_table_2.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Conference_table/Conference_table.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Couch office 1 Prefab/Couch office 1 Prefab.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Couch office 2 Prefab/Couch office 2 Prefab.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Drawer/Drawer.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Office chair.001 Prefab/Office chair.001 Prefab.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/OfficeCabinet_02a_snaps011/OfficeCabinet_02a_snaps011.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Ottoman/Ottoman.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Ottoman_v2/Ottoman_v2.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Sofa_1/Sofa_1.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Office/Sofa_2/Sofa_2.prefab"
    };

    private string assetBundleName = "generatedobjects_assets_all_5fcb792c29aa23a8981898b35fb9ea7b.bundle";

    // Loading the asset
    IEnumerator Start()
    {
        // string path = UnityEngine.Application.dataPath + "/../" + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        // string path = UnityEngine.AddressableAssets.Addressables.RuntimePath + " and " + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        //Debug.Log(path);

        // asynchronous loading of assetbundle 
        var assetBundleLoadRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "AssetBundles", assetBundleName));
        yield return assetBundleLoadRequest; // return request object while the bundle is loading

        // after loading assign the bundle
        var assetBundle = assetBundleLoadRequest.assetBundle;

        // if asset bundle loading worked
        if (assetBundle != null)
        {
            var i = 0; //enumerate for spawning
            foreach (var assetName in assetNames)
            {
                // asynchronous loading of asset
                var assetLoadRequest = assetBundle.LoadAssetAsync<GameObject>(assetName);
                yield return assetLoadRequest;  //return request object while the asset is loading

                // after loading assign the asset
                GameObject asset = assetLoadRequest.asset as GameObject;

                // if asset loading worked
                if (asset != null)
                {
                    GameObject instance = Instantiate(asset, new Vector3(0, 0, -20 + (3 * i)), Quaternion.identity);
                    i++;

                    // add more actions
                }
                else
                {
                    Debug.LogError("Asset " + assetName + " could not be found in bundle " + assetBundleName + "!");
                }
            }
            // unload bundle
            assetBundle.Unload(false);

        }
        else
        {
            Debug.LogError("Failed to load asset bundle: " + assetBundleName);
        }
    }
}
