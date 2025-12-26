#!/bin/bash

APK="Build/game-o-quan.apk"
PACKAGE="com.defaultcompany.oquan"

echo "🤖 Auto-install script"
echo "Waiting for APK to be ready..."
echo ""

# Wait for APK
while [ ! -f "$APK" ]; do
    echo -ne "\r⏳ Waiting for build... $(date +%H:%M:%S)"
    sleep 5
done

echo ""
echo "✅ APK ready!"

# Get size
SIZE=$(ls -lh "$APK" | awk '{print $5}')
echo "📦 Size: $SIZE"
echo ""

# Check device
echo "📱 Checking device..."
if ! adb devices | grep -q "device$"; then
    echo "❌ No device connected!"
    exit 1
fi

echo "✅ Device found"
echo ""

# Uninstall old
echo "🗑️  Uninstalling old version..."
adb uninstall "$PACKAGE" 2>/dev/null || echo "No old version"

# Install
echo "⬇️  Installing APK..."
if adb install "$APK"; then
    echo ""
    echo "✅ INSTALL SUCCESS!"
    echo ""
    
    # Launch
    echo "🚀 Launching game..."
    sleep 2
    adb shell am start -n "$PACKAGE/com.unity3d.player.UnityPlayerActivity"
    
    echo ""
    echo "🎮 Game launched on device!"
    echo ""
    echo "📊 Monitor logs:"
    echo "   adb logcat | grep Unity"
else
    echo ""
    echo "❌ Install failed!"
fi
