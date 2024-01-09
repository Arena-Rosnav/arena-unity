#!/bin/bash -i

unity_location="${HOME}/Unity/Arena-Unity"

mkdir -p "$unity_location"
cd "$unity_location"

url="https://download.unity3d.com/download_unity/d00248457e15/LinuxEditorInstaller/Unity.tar.xz"
download_file_name="Unity.tar.xz"
extracted_folder="Editor"

# Check if download file already exists
if [ -f "$download_file_name" ]; then
    echo "The compressed Unity Editor file already exists. Skipping download."
    echo "If you wish to download the file again, delete '$download_file_name' in $unity_location."
else
    echo "Downloading Unity Editor."
    wget $url -O $download_file_name

    # Check if wget was successful
    if [ $? -ne 0 ]; then
        echo "Download of Unity Editor failed."
        exit 1
    fi

    echo "Download successful"
fi

if [ -d "$extracted_folder" ]; then
    echo "The to be extracted Unity Editor directory already exists. Skipping extraction."
    echo "If you wish to extract it again. Delete the 'Editor' directory in $unity_location."
else
    # Extract file
    tar -xf $file_name
    echo "Successfully extracted the Unity Editor."

    # Check if tar was successful
    if [ $? -ne 0 ]; then
        echo "Extracting of Unity Editor failed."
        exit 1
    fi
fi

echo "You can now build arena-unity by running the 'build-arena-unity.sh' script."
