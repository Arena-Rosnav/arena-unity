using UnityEngine;
using System.IO;
using System.Collections;

// Dev Class for testing the spawning of a single asset from a single asset bundle.
// The asset should be built into an asset bundle which is located in Assets/AssetBundles/
// Define the asset and bundle by assigning the relative asset path to assetName and the asset bundle name to assetBundleName.
public class AddressableSpawn : MonoBehaviour
{

    private string assetName = "Assets/UserAssets/chair 1/chair.prefab";
    private string assetBundleName = "user assets";

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
            // asynchronous loading of asset
            var assetLoadRequest = assetBundle.LoadAssetAsync<GameObject>(assetName);
            yield return assetLoadRequest;  //return request object while the asset is loading

            // after loading assign the asset
            GameObject asset = assetLoadRequest.asset as GameObject;

            // if asset loading worked
            if (asset != null)
            {
                GameObject instance = Instantiate(asset, transform);

                // add more actions

            } else{
                Debug.LogError("Asset "+assetName+" could not be found in bundle "+assetBundleName+"!");
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
