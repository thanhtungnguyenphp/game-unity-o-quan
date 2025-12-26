#!/bin/bash
echo "📱 Installing Production APK v1.0 (Build 4)"
adb install -r "./Build/game-o-quan.apk"
if [ $? -eq 0 ]; then
    echo "✅ Success!"
else
    echo "❌ Failed - Enable 'Install via USB' in Developer Options"
fi
