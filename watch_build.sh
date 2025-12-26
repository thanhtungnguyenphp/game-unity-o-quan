#!/bin/bash

LOG="Build/build.log"
APK="Build/game-o-quan.apk"

echo "🔍 Watching build progress..."
echo "Press Ctrl+C to stop"
echo ""

LAST_SIZE=0

while true; do
    clear
    echo "=================================="
    echo "🎮 BUILD PROGRESS - $(date +%H:%M:%S)"
    echo "=================================="
    echo ""
    
    # Check Unity process
    if ps aux | grep -q "[U]nity.*BuildAPK"; then
        echo "✅ Unity is building..."
    else
        echo "⚠️  Unity process not found"
    fi
    
    echo ""
    
    # Check log size
    if [ -f "$LOG" ]; then
        SIZE=$(wc -l < "$LOG")
        if [ $SIZE -gt $LAST_SIZE ]; then
            echo "📝 Build log: $SIZE lines (+$((SIZE - LAST_SIZE)))"
            LAST_SIZE=$SIZE
        else
            echo "📝 Build log: $SIZE lines"
        fi
        
        echo ""
        echo "📋 Last 5 lines:"
        echo "---"
        tail -5 "$LOG" | sed 's/^/  /'
        echo "---"
    else
        echo "⏳ Waiting for build log..."
    fi
    
    echo ""
    
    # Check APK
    if [ -f "$APK" ]; then
        SIZE=$(ls -lh "$APK" | awk '{print $5}')
        echo "✅ APK READY!"
        echo "📦 Size: $SIZE"
        echo "📍 Location: $APK"
        echo ""
        echo "🎉 Build complete! Check install.log for install status"
        break
    else
        echo "⏳ APK not ready yet..."
    fi
    
    echo ""
    echo "=================================="
    
    sleep 5
done
