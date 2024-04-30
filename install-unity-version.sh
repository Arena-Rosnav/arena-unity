#!/bin/bash -i

unity_location="${HOME}/Unity/Hub/Editor/2022.3.11f1"

echo "*** Make sure you already installed the Unity Hub manually. Otherwise, follow these instructions: https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux"

mkdir -p "$unity_location"
cd "$unity_location"

url="https://download.unity3d.com/download_unity/d00248457e15/LinuxEditorInstaller/Unity.tar.xz"
download_file_name="Unity.tar.xz"
extracted_folder="Editor"

# Check if Unity Editor is already at location
if [ -d "$extracted_folder" ]; then
    echo "*** The Unity Editor already exists at correct location."
    echo "*** If you wish to extract it again, delete the \"Editor\" directory in \"$unity_location\"."
    exit 0
fi

# Check if download file already exists
if [ -f "$download_file_name" ]; then
    echo "*** The compressed Unity Editor file already exists. Skipping download."
    echo "*** If you wish to download the file again, delete \"$download_file_name\" in \"$unity_location\"."
else
    echo "*** Downloading Unity Editor."
    wget $url -O $download_file_name

    # Check if wget was successful
    if [ $? -ne 0 ]; then
        echo "*** Download of Unity Editor failed."
        exit 1
    fi

    echo "*** Download successful"
fi

echo "*** Extracting Unity Editor. May take a few minutes."
# Extract file
tar -xf $download_file_name
echo "*** Successfully extracted the Unity Editor."

# Check if tar was successful
if [ $? -ne 0 ]; then
    echo "*** Extracting of Unity Editor failed."
    exit 1
fi

echo "*** Deleting downloaded tar file"

rm -rf "$download_file_name"
