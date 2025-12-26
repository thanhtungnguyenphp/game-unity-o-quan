#!/bin/bash

echo "🔨 Building APK with Tutorial System..."

/Applications/Unity/Hub/Editor/2022.3.11f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode \
  -projectPath "$(pwd)" \
  -executeMethod BuildScript.BuildAndroid \
  -logFile "$(pwd)/build_tutorial.log"

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    echo "📦 APK location: ./Build/game-o-quan.apk"
    
    # Auto install if device connected
    if adb devices | grep -q "device$"; then
        echo "📱 Installing to device..."
        adb install -r "./Build/game-o-quan.apk"
        echo "✅ Installation complete!"
    else
        echo "⚠️  No device connected. Connect device and run:"
        echo "   adb install -r ./Build/game-o-quan.apk"
    fi
else
    echo "❌ Build failed. Check build_tutorial.log"
    tail -50 build_tutorial.log
fi
