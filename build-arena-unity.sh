#!/bin/bash -i

unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

ARENA_UNITY_BUILD="${project_path}/Build/arena-unity"

build_log="${project_path}/Build/build_log.txt"

ARENA_SIM_SETUP="$(cd "$project_path"/../arena-simulation-setup && pwd)"

echo "Building ..."

echo "Please wait for build to finish"

# Build Unity
"$unity_location" -quit -batchmode -projectpath "$project_path" -buildLinux64Player "$build_path" -logFile "$build_log"

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "Build executed successfully."

    # Export arena_simulation_setup location for arena-unity to find files
    export ARENA_SIM_SETUP
    # Export arena unity build location to verify build and find during launch
    export ARENA_UNITY_BUILD
else
    echo "Build failed."
fi
