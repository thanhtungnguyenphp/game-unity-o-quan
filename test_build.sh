#!/bin/bash

echo "🎮 === TEST BUILD GAME Ổ QUAN ==="

PROJECT_PATH="/Users/Shared/jerry/game_unity/game-unity-o-quan"
UNITY_PATH="/Applications/Unity/Hub/Editor/6000.1.9f1/Unity/Unity.app/Contents/MacOS/Unity"
BUILD_PATH="$PROJECT_PATH/Build"
APK_PATH="$BUILD_PATH/game-o-quan.apk"

echo "📁 Project Path: $PROJECT_PATH"
echo "🔧 Unity Path: $UNITY_PATH"
echo "📦 APK Output: $APK_PATH"

# Tạo thư mục build nếu chưa có
mkdir -p "$BUILD_PATH"

echo ""
echo "🔍 Kiểm tra Unity Editor..."
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Không tìm thấy Unity Editor tại: $UNITY_PATH"
    echo "Vui lòng cài đặt Unity 2022.3.29f1 hoặc compatible version"
    exit 1
fi

echo "✅ Unity Editor found"

echo ""
echo "📋 Kiểm tra project files..."
if [ ! -f "$PROJECT_PATH/Assets/Scenes/SampleScene.unity" ]; then
    echo "❌ Không tìm thấy scene chính"
    exit 1
fi

echo "✅ Project files OK"

echo ""
echo "🔧 HƯỚNG DẪN BUILD MANUAL:"
echo "1. Mở Unity Hub"
echo "2. Add project từ: $PROJECT_PATH"
echo "3. Mở project với Unity 2022.3.29f1 (hoặc compatible)"
echo "4. Đi tới: File → Build Settings"
echo "5. Chọn platform: Android"
echo "6. Click 'Switch Platform'"
echo "7. Nhấn 'Build' và chọn save tại: $BUILD_PATH/game-o-quan.apk"
echo ""
echo "Hoặc sử dụng build script trong Editor:"
echo "- Đi tới menu: Build → Build Android APK"
echo ""

echo "📊 Project Status:"
echo "- Unity Version Required: 2022.3.29f1"
echo "- Unity Version Available: 6000.1.9f1"
echo "- Platform Target: Android"
echo "- Build Output: $APK_PATH"

echo ""
echo "⚠️  Lưu ý: Do version mismatch, khuyến nghị:"
echo "1. Cài đặt Unity 2022.3.29f1 qua Unity Hub"
echo "2. Hoặc upgrade project lên Unity 6000.1.9f1"
echo ""
echo "✨ Script hoàn thành!"
