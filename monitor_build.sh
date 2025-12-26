#!/bin/bash

BUILD_LOG="Build/build.log"
APK_PATH="Build/game-o-quan.apk"

echo "🔍 Monitoring Unity build..."
echo "Press Ctrl+C to stop monitoring"
echo ""

while true; do
    if [ -f "$BUILD_LOG" ]; then
        # Check for completion
        if grep -q "Build completed" "$BUILD_LOG" 2>/dev/null; then
            echo ""
            echo "✅ BUILD COMPLETE!"
            
            if [ -f "$APK_PATH" ]; then
                SIZE=$(ls -lh "$APK_PATH" | awk '{print $5}')
                echo "📦 APK: $APK_PATH"
                echo "📊 Size: $SIZE"
                echo ""
                echo "Install now? (y/n)"
                read -r INSTALL
                
                if [[ $INSTALL == "y" ]]; then
                    echo "📱 Installing..."
                    adb uninstall com.defaultcompany.oquan 2>/dev/null
                    adb install "$APK_PATH"
                    
                    echo ""
                    echo "🚀 Launch game? (y/n)"
                    read -r LAUNCH
                    
                    if [[ $LAUNCH == "y" ]]; then
                        adb shell am start -n com.defaultcompany.oquan/com.unity3d.player.UnityPlayerActivity
                    fi
                fi
            fi
            break
        fi
        
        # Check for errors
        if grep -q "Build failed" "$BUILD_LOG" 2>/dev/null; then
            echo ""
            echo "❌ BUILD FAILED!"
            echo "Last 20 lines:"
            tail -20 "$BUILD_LOG"
            break
        fi
        
        # Show progress
        LAST_LINE=$(tail -1 "$BUILD_LOG" 2>/dev/null)
        echo -ne "\r⏳ Building... $LAST_LINE                    "
    else
        echo -ne "\r⏳ Waiting for build to start...                    "
    fi
    
    sleep 2
done

echo ""
