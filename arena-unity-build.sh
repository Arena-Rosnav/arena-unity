#!/bin/bash

echo "*** This script installs the latest Arena-Unity release from Arena-Rosnav/arena-unity."

# Check if Arena-Unity-Build.tar.gz exists and delete if it does
if [ -f "Arena-Unity-Build.tar.gz" ]; then
    echo "*** Arena-Unity-Build.tar.gz file already exists. Deleting it..."
    # rm "Arena-Unity-Build.tar.gz"
fi

if [ -d "Build" ]; then
    echo "*** Removing old Build directory"
    rm -rf "Build"
fi

# Download Arena-Unity-Build.tar.gz
#url="
#echo "*** Downloading latest release"
#wget -O "Arena-Unity-Build.tar.gz" [URL] || { echo "Error: Download failed"; exit 1; }

# Extract the tar.gz file to a folder named Build
echo "*** Extracting Arena-Unity build"
mkdir -p Build
tar -xzf "Arena-Unity-Build.tar.gz" || { echo "Error: Extraction failed"; exit 1; }

echo "*** Extraction successful. You can now start arena-rosnav with simulator:=unity"

echo "*** Deleting downloaded tar.gz file"
rm "Arena-Unity-Build.tar.gz"
