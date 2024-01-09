#!/bin/bash -i

unity_location="${HOME}/Unity/Arena-Unity/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

build_path="${project_path}/Build/arena-unity"

build_log="${project_path}/Build/build_log.txt"

if [ -f "$unity_location" ]; then
    echo "Found Unity Editor at correct location"
else 
    echo "Couldn't find Unity Editor at \"$unity_location\"."
    echo "Installing correct Unity Editor"
    ./install-unity.sh

    if [ $? -ne 0 ]; then
        echo "Failed to install Unity Editor"
        exit 1
    fi
fi

echo "Building ..."

echo "Please wait for build to finish"

# Create build dir
mkdir -p "$project_path/Build"

# Build Unity
"$unity_location" -quit -batchmode -projectpath "$project_path" -buildLinux64Player "$build_path" -logFile "$build_log"

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "Build executed successfully."
else
    echo "Build failed."
fi
