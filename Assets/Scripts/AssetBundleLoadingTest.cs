using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using System.Collections.Generic;

/**
* Dev Script for testing the spawning of assets from asset bundles.
* The assets should be built into asset bundles and be put into a folder under the StreamingAssets folder of this project.
* Define the assets and bundle by assigning the relative asset paths to assetPaths and the asset bundle name to assetBundleName.
*
* assetPaths: String list of RELATIVE paths (to the assets)
* assetBundleFolder: Folder in which all asset bundles are stored under StreamingAssets (default: StandaloneLinux64)
* assetBundleCatalog: Name of the catalog of the related asset bundles. Every release contains a catalog file which is needed to load the asset bundles of this release. Extra catalog files (from other addressable builds) can be added and loaded with Addressables.LoadContentCatalogAsync() in a similar way.
**/
public class AddressableSpawn : MonoBehaviour
{
    private string[] assetPaths = {
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
        "Assets/Resources_moved/Objects/Prefabs/Office/Sofa_2/Sofa_2.prefab",
        "Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_03a_snaps011/Box_03a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Bollard_01a_snaps011/Bollard_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Drawer/Drawer.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/TrashCan_01a_snaps011/TrashCan_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair 3/Chair 3.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Chair Office/Chair Office.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Kitchen Table B Prefab/Kitchen Table B Prefab.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/Fern_01/Fern_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_07_01/FFK_Table_07_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/OfficeCabinet_02a_snaps011/OfficeCabinet_02a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/General/WallShelf_Apt_02/WallShelf_Apt_02.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/DebrisCan_02a_snaps011/DebrisCan_02a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/Monitor_Apt_01/Monitor_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Box/Box.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/Plant_2/Plant_2.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Base_Sink_01/Cabinet_Base_Sink_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_06_01/FFK_Table_06_01.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/Computer_apt_01/Computer_apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Base_DD_01/Cabinet_Base_DD_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_MirrorFrame_01/RR_MirrorFrame_01.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/LoungeChair_01/LoungeChair_01.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Nighttable2/Nighttable2.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_Sink_01/RR_Sink_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair_Apt_01/Chair_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Kegs_01a_snaps011/Kegs_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_Pallet_01a_snaps011/Box_Pallet_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/General/Rack/Rack.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_Urinal_01/RR_Urinal_01.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Table_Side_Apt_01/Table_Side_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Wall_SD_01/Cabinet_Wall_SD_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Table_2/Table_2.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_03_01/FFK_Stool_03_01.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Dresser_Apt_01/Dresser_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/WallShelf_Apt_01/WallShelf_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/OfficeTable_01a_snaps011/OfficeTable_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Conference_table/Conference_table.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Bench_02_01/FFK_Bench_02_01.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Stove_01/Stove_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_02_01/FFK_Stool_02_01.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/Speaker Sub/Speaker Sub.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_01_01/FFK_Stool_01_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Office chair.001 Prefab/Office chair.001 Prefab.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_03_02/FFK_Stool_03_02.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Bedside Table/Bedside Table.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/Pot_01/Pot_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Bench_Apt_01/Bench_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Table_Media_01/Table_Media_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/OfficeChair_01a_snaps011/OfficeChair_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_02a_snaps011/Box_02a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Bench_03_01/FFK_Bench_03_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_04a_snaps011/Box_04a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_01_01/FFK_Table_01_01.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/PotFern_01/PotFern_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Shelf_Apt_01/Shelf_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Chair_01_02/FFK_Chair_01_02.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Tea Table/Tea Table.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair 2/Chair 2.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/phone2k 1/phone2k 1.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Ottoman/Ottoman.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_10_01/FFK_Table_10_01.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/Mouse_apt_01/Mouse_apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Bed_Apt_02_01/Bed_Apt_02_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Round Sofa/Round Sofa.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_Toilet_01/RR_Toilet_01.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Table_Computer_01/Table_Computer_01.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Table_Dining_Apt_01/Table_Dining_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Book Shelf One Side/Book Shelf One Side.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Sofa_2/Sofa_2.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Table_Coffee_01/Table_Coffee_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Water cooler/Water cooler.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Table_Computer_01_Setup/Table_Computer_01_Setup.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_SoapDispenser_01/RR_SoapDispenser_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Couch office 1 Prefab/Couch office 1 Prefab.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Coffee_table_2/Coffee_table_2.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Chair_02_01/FFK_Chair_02_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_01_02/FFK_Stool_01_02.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Cabinet with drawings/Cabinet with drawings.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Base_Corner_01/Cabinet_Base_Corner_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_HandDryer_01/RR_HandDryer_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_04_01/FFK_Table_04_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_01a_snaps011/Box_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Sofa_1/Sofa_1.prefab","Assets/Resources_moved/Objects/Prefabs/Office/water dispenser prefab/water dispenser prefab.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/Plant_3/Plant_3.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Wall_DD_02/Cabinet_Wall_DD_02.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Chair_1/Chair_1.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Kitchen Table A Prefab/Kitchen Table A Prefab.prefab","Assets/Resources_moved/Objects/Prefabs/General/GlassCase_01/GlassCase_01.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/Box_Pallet_02a_snaps011/Box_Pallet_02a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Fridge_01/Fridge_01.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Chair_3/Chair_3.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Stool_02_02/FFK_Stool_02_02.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Nighttable1/Nighttable1.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Closet/Closet.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_ToiletPaperDispenser/RR_ToiletPaperDispenser.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Couch office 2 Prefab/Couch office 2 Prefab.prefab","Assets/Resources_moved/Objects/Prefabs/General/RoundTable_01/RoundTable_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair Regular/Chair Regular.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_02_01/FFK_Table_02_01.prefab","Assets/Resources_moved/Objects/Prefabs/Electronics/KeyBoard_Apt_01/KeyBoard_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Office/Ottoman_v2/Ottoman_v2.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Wall_DD_01/Cabinet_Wall_DD_01.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Sofa_Apt_01/Sofa_Apt_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restroom/RR_PaperTowelDispenser_01/RR_PaperTowelDispenser_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair/Chair.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Base_SD_01/Cabinet_Base_SD_01.prefab","Assets/Resources_moved/Objects/Prefabs/Nature/Plant_1/Plant_1.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair_2/Chair_2.prefab","Assets/Resources_moved/Objects/Prefabs/General/BarStool_01/BarStool_01.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Bench_01_01/FFK_Bench_01_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Chair Relax/Chair Relax.prefab","Assets/Resources_moved/Objects/Prefabs/Living Room/Single Sofa/Single Sofa.prefab","Assets/Resources_moved/Objects/Prefabs/Restaurant/FFK_Table_03_02/FFK_Table_03_02.prefab","Assets/Resources_moved/Objects/Prefabs/Kitchen/Cabinet_Wall_Corner_01/Cabinet_Wall_Corner_01.prefab","Assets/Resources_moved/Objects/Prefabs/General/Wall Drawer/Wall Drawer.prefab","Assets/Resources_moved/Objects/Prefabs/Warehouse/DebrisBottle_01a_snaps011/DebrisBottle_01a_snaps011.prefab","Assets/Resources_moved/Objects/Prefabs/Bedroom/Bed_Apt_01_02/Bed_Apt_01_02.prefab"
    };
    private string assetBundleFolder = "StandaloneLinux64";
    private string assetBundleCatalog = "catalog_0.2.json";

    AsyncOperationHandle<IList<GameObject>> opHandle;

    // Loading the asset
    IEnumerator Start()
    {
        // string path = UnityEngine.Application.dataPath + "/../" + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        // string path = UnityEngine.AddressableAssets.Addressables.RuntimePath + " and " + UnityEditor.EditorUserBuildSettings.activeBuildTarget;
        //Debug.Log(path);

        // asynchronous loading of assetbundle catalog
        // Debug.Log(Path.Combine(Application.streamingAssetsPath, assetBundleFolder, assetBundleCatalog));
        AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(Path.Combine(Application.streamingAssetsPath, assetBundleFolder, assetBundleCatalog), true);
        yield return handle;


        int x = 0, z = 0;
        // asynchronous loading multiple assets
        opHandle = Addressables.LoadAssetsAsync<GameObject>(
            assetPaths,
            asset =>
            {
                GameObject instance = Instantiate(asset, new Vector3((3 * x++) - 20, 0, (3 * z) - 20), Quaternion.identity);
                if (x > 15)
                {
                    x = 0;
                    z++;
                }
            },
            Addressables.MergeMode.Union,
            false);
        yield return opHandle;

        // if asset loading worked
        if (opHandle.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("There was at least one loading error in the asset bundle!");
        }
    }
    void OnDestroy()
    {
        Addressables.Release(opHandle);
    }
}
