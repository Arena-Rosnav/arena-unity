#!/bin/bash -i
unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

build_method="CreateAssetBundles.BuildAllAssetBundles"

build_log="${project_path}/Assets/StreamingAssets/AssetBundles/build_log.txt"

if [ -f "$unity_location" ]; then
    echo "*** Found Unity Editor at correct location"
else 
    echo "*** Couldn't find Unity Editor at \"$unity_location\"."
    echo "*** Install the correct version of the Unity Editor by running the script install-unity-version.sh"
    
    exit 1
fi

echo "*** Creating folder..."
# Create build dir
mkdir -p "$project_path/Assets/StreamingAssets/AssetBundles"

echo "*** Building custom asset bundle..."
# Build Unity
"$unity_location" -quit -batchmode -projectpath "$project_path" -executeMethod "$build_method" -logFile "$build_log"

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "*** Asset bundle build failed. Look into Assets/StreamingAssets/AssetBundles/build_log.txt for more information."
    exit 1
fi
echo "*** Asset bundle built successfully."
