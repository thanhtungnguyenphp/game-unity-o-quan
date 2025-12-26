#!/bin/bash
echo "📱 Installing APK with Tutorial System..."
adb install -r "./Build/game-o-quan.apk"
if [ $? -eq 0 ]; then
    echo "✅ Installation successful!"
    echo "🎮 Open app and test tutorial"
else
    echo "❌ Installation failed"
fi
