#!/bin/bash
echo "📱 Installing optimized APK..."
echo "⚠️  Please allow installation on device if prompted"
echo ""
adb install -r "./Build/game-o-quan.apk"
if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Installation successful!"
    echo ""
    echo "🎮 Test checklist:"
    echo "  - Check FPS (should be stable 60)"
    echo "  - Check memory usage"
    echo "  - Play several games"
    echo "  - Compare with old version"
else
    echo ""
    echo "❌ Installation failed"
    echo "Please enable 'Install via USB' in Developer Options"
fi
