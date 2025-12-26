#!/bin/bash

APK="Build/game-o-quan.apk"

echo "⏳ Waiting for APK to be built..."
echo "Press Ctrl+C to cancel"
echo ""

while [ ! -f "$APK" ]; do
    echo -ne "\r⏳ Waiting... $(date +%H:%M:%S)"
    sleep 3
done

echo ""
echo ""
echo "✅ APK detected!"
echo ""

# Run install script
./install_apk.sh
