#!/bin/bash -i

unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

build_path="${project_path}/Build/arena-unity"

build_log="${project_path}/Build/build_log.txt"

if [ -f "$unity_location" ]; then
    echo "*** Found Unity Editor at correct location"
else 
    echo "*** Couldn't find Unity Editor at \"$unity_location\"."
    echo "*** Install the correct version of the Unity Editor by running the script install-unity-version.sh"
    
    exit 1
fi

echo "*** Building ..."

# Create build dir
mkdir -p "$project_path/Build"

# Build Unity
"$unity_location" -quit -batchmode -projectpath "$project_path" -buildLinux64Player "$build_path" -logFile "$build_log"

# Check if build was successful
if [ $? -ne 0 ]; then
    echo "*** Build failed. Look into Build/build_log.txt for more information."
    exit 1
fi
echo "*** Build executed successfully."

echo "*** Compressing build into Arena-Unity-Build.tar.gz for a new release."

tar -czf Arena-Unity-Build.tar.gz Build/

if [ $? -eq 0 ]; then
    echo "*** Successfully compressed into Arena-Unity-Build.tar.gz"
    exit 0
else 
    echo "*** Compression failed"
    exit 1
fi