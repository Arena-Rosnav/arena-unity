using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;

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
    AsyncOperationHandle<GameObject> opHandle;

    // Loading the asset
    IEnumerator Start()
    {
        // string path = UnityEngine.Application.dataPath + "/../" + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        // string path = UnityEngine.AddressableAssets.Addressables.RuntimePath + " and " + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        //Debug.Log(path);

        // asynchronous loading of assetbundle 
        AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(Path.Combine(Addressables.RuntimePath, "StandaloneLinux64", "catalog_0.1.json"), true);
        yield return handle;

        var i = 0; //enumerate for spawning
        foreach (var assetName in assetNames)
        {
            // asynchronous loading of asset
            opHandle = Addressables.LoadAssetAsync<GameObject>(assetName);
            yield return opHandle;

            // if asset loading worked
            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject asset = opHandle.Result;
                GameObject instance = Instantiate(asset, new Vector3(0, 0, -20 + (3 * i)), Quaternion.identity);
                i++;

                // add more actions
            }
            else
            {
                Debug.LogError("Asset " + assetName + " could not be found in bundle!");
            }
        }
    }
    void OnDestroy()
    {
        Addressables.Release(opHandle);
    }
}
