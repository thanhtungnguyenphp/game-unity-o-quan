#!/bin/bash

echo "=================================="
echo "🎮 BUILD APK - Ô Quan Game"
echo "=================================="
echo ""

PROJECT_PATH="/Users/Shared/jerry/game_unity/game-unity-o-quan"
BUILD_PATH="$PROJECT_PATH/Build"
APK_PATH="$BUILD_PATH/game-o-quan.apk"
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f2/Unity.app/Contents/MacOS/Unity"

# Check Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity not found at: $UNITY_PATH"
    echo "Please update UNITY_PATH in script"
    exit 1
fi

echo "✅ Unity found"
echo ""

# Create build directory
mkdir -p "$BUILD_PATH"

# Build APK using Unity command line
echo "🔨 Building APK..."
echo "This may take 5-10 minutes..."
echo ""

"$UNITY_PATH" \
    -quit \
    -batchmode \
    -projectPath "$PROJECT_PATH" \
    -executeMethod BuildAPK.BuildAndroid \
    -logFile "$BUILD_PATH/build.log"

BUILD_EXIT_CODE=$?

echo ""
if [ $BUILD_EXIT_CODE -eq 0 ] && [ -f "$APK_PATH" ]; then
    APK_SIZE=$(ls -lh "$APK_PATH" | awk '{print $5}')
    echo "✅ BUILD SUCCESS!"
    echo "📦 APK: $APK_PATH"
    echo "📊 Size: $APK_SIZE"
    echo ""
    
    # Ask to install
    echo "Install on connected device? (y/n)"
    read -r INSTALL
    
    if [[ $INSTALL == "y" || $INSTALL == "Y" ]]; then
        echo ""
        echo "📱 Checking devices..."
        adb devices
        
        DEVICE_COUNT=$(adb devices | grep -w device | wc -l)
        if [ $DEVICE_COUNT -eq 0 ]; then
            echo "❌ No Android device connected"
            echo "Connect device and run: adb install \"$APK_PATH\""
        else
            echo "✅ Found $DEVICE_COUNT device(s)"
            echo ""
            echo "🗑️  Uninstalling old version..."
            adb uninstall com.defaultcompany.oquan 2>/dev/null || true
            
            echo "⬇️  Installing APK..."
            if adb install "$APK_PATH"; then
                echo ""
                echo "✅ INSTALL SUCCESS!"
                echo "🎮 Game installed on device"
                echo ""
                echo "Launch game? (y/n)"
                read -r LAUNCH
                
                if [[ $LAUNCH == "y" || $LAUNCH == "Y" ]]; then
                    echo "🚀 Launching game..."
                    adb shell am start -n com.defaultcompany.oquan/com.unity3d.player.UnityPlayerActivity
                fi
            else
                echo "❌ Install failed"
            fi
        fi
    fi
else
    echo "❌ BUILD FAILED"
    echo "Check log: $BUILD_PATH/build.log"
    tail -n 50 "$BUILD_PATH/build.log"
fi

echo ""
echo "=================================="
