#!/bin/bash -i

unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1/Editor/Unity"

project_path="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

build_path="${project_path}/build/arena-unity.x86_64"

build_log="${project_path}/build/build_log.txt"

ARENA_SIM_SETUP="$(cd "$project_path"/../arena-simulation-setup && pwd)"

# Export arena_simulation_setup location for arena-unity to find files
export ARENA_SIM_SETUP

echo "Building"

echo "Please wait for build to finish"

# Build Unity
"$unity_location" -quit -batchmode -projectpath "$project_path" -buildLinux64Player "$build_path" -logFile "$build_log"
