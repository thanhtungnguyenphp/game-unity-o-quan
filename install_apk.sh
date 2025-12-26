#!/bin/bash

APK="Build/game-o-quan.apk"
PACKAGE="com.defaultcompany.oquan"

echo "🎮 Installing Ô Quan Game"
echo ""

# Check APK exists
if [ ! -f "$APK" ]; then
    echo "❌ APK not found: $APK"
    echo "Please build APK first in Unity"
    exit 1
fi

# Get APK info
SIZE=$(ls -lh "$APK" | awk '{print $5}')
echo "📦 APK: $SIZE"
echo ""

# Check device
echo "📱 Checking device..."
if ! adb devices | grep -q "device$"; then
    echo "❌ No device connected!"
    exit 1
fi
echo "✅ Device connected"
echo ""

# Uninstall old
echo "🗑️  Uninstalling old version..."
adb uninstall "$PACKAGE" 2>/dev/null || echo "No old version"
echo ""

# Install
echo "⬇️  Installing APK..."
if adb install "$APK"; then
    echo ""
    echo "✅ INSTALL SUCCESS!"
    echo ""
    
    # Launch
    echo "🚀 Launching game..."
    sleep 1
    adb shell am start -n "$PACKAGE/com.unity3d.player.UnityPlayerActivity"
    
    echo ""
    echo "🎮 Game launched!"
    echo ""
    echo "📊 View logs:"
    echo "   adb logcat | grep Unity"
else
    echo ""
    echo "❌ Install failed!"
fi
