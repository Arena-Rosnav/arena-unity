#!/bin/bash -i

unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

build_path="${project_path}/Build/arena-unity"

build_log="${project_path}/Build/build_log.txt"

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
