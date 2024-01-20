#!/bin/bash

file_name="Arena-Unity-Build.tar.gz"

echo "*** This script installs the latest Arena-Unity release from Arena-Rosnav/arena-unity."

# Check if Arena-Unity-Build.tar.gz exists and delete if it does
if [ -f file_name ]; then
    echo "*** $file_name file already exists. Deleting it..."
    rm $file_name
fi

if [ -d "Build" ]; then
    echo "*** Removing old Build directory"
    rm -rf "Build"
fi

# Download Arena-Unity-Build.tar.gz
url="https://github.com/Arena-Rosnav/arena-unity/releases/latest/download/$file_name"
echo "*** Downloading latest release"
wget $url || { echo "Error: Download failed"; exit 1; }

# Extract the tar.gz file to a folder named Build
echo "*** Extracting Arena-Unity build"
mkdir -p Build
tar -xzf $file_name || { echo "Error: Extraction failed"; exit 1; }

echo "*** Extraction successful. You can now start arena-rosnav with simulator:=unity"

echo "*** Deleting downloaded tar.gz file"
rm $file_name
