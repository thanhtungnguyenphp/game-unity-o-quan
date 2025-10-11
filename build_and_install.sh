#!/bin/bash

echo "🎮 === BUILD VÀ CÀI ĐẶT GAME Ổ QUAN ==="

PROJECT_PATH="/Users/Shared/jerry/game_unity/game-o-quan"
BUILD_PATH="$PROJECT_PATH/Build"
APK_PATH="$BUILD_PATH/game-o-quan.apk"

echo "📁 Project Path: $PROJECT_PATH"
echo "📦 APK Output: $APK_PATH"

# Tạo thư mục build nếu chưa có
mkdir -p "$BUILD_PATH"

echo ""
echo "📱 Kiểm tra thiết bị Android được kết nối..."
adb devices

# Kiểm tra xem có thiết bị nào không
DEVICE_COUNT=$(adb devices | grep -w device | wc -l)
if [ $DEVICE_COUNT -eq 0 ]; then
    echo "❌ Không tìm thấy thiết bị Android nào được kết nối!"
    echo "Vui lòng:"
    echo "1. Kết nối điện thoại qua USB"
    echo "2. Bật USB Debugging"
    echo "3. Cho phép kết nối từ máy tính này"
    exit 1
fi

echo "✅ Tìm thấy $DEVICE_COUNT thiết bị Android"

# Vì Unity build từ command line có vấn đề, tôi sẽ hướng dẫn user build manual
echo ""
echo "⚠️  Do vấn đề với Unity command line build, vui lòng làm theo các bước sau:"
echo ""
echo "🔧 HƯỚNG DẪN BUILD MANUAL:"
echo "1. Unity Editor đã được mở"
echo "2. Trong Unity Editor, đi tới: File → Build Settings"
echo "3. Chọn platform: Android"
echo "4. Click 'Switch Platform' (nếu cần)"
echo "5. Nhấn 'Build' và chọn save tại: $BUILD_PATH/game-o-quan.apk"
echo ""
echo "Hoặc sử dụng build script trong Editor:"
echo "- Đi tới menu: Build → Build Android APK"
echo ""

# Function để chờ user build xong
wait_for_apk() {
    echo "⏳ Đang chờ APK được tạo..."
    while [ ! -f "$APK_PATH" ]; do
        echo "   Chờ file APK tại: $APK_PATH"
        sleep 5
    done
    echo "✅ Tìm thấy APK!"
}

# Hỏi user có muốn chờ không
echo "Bạn có muốn script chờ cho đến khi APK được tạo? (y/n)"
read -r WAIT_RESPONSE

if [[ $WAIT_RESPONSE == "y" || $WAIT_RESPONSE == "Y" ]]; then
    wait_for_apk
    
    # Kiểm tra kích thước APK
    APK_SIZE=$(ls -lh "$APK_PATH" | awk '{print $5}')
    echo "📊 Kích thước APK: $APK_SIZE"
    
    # Cài đặt APK
    echo ""
    echo "📱 Bắt đầu cài đặt APK lên điện thoại..."
    
    # Gỡ cài đặt phiên bản cũ (nếu có)
    echo "🗑️  Gỡ cài đặt phiên bản cũ (nếu có)..."
    adb uninstall com.lamgame.oquan 2>/dev/null || true
    
    # Cài đặt APK mới
    echo "⬇️  Đang cài đặt APK..."
    if adb install "$APK_PATH"; then
        echo "✅ CÀI ĐẶT THÀNH CÔNG!"
        echo "🎮 Game 'Ổ Quan' đã được cài đặt trên điện thoại!"
        echo ""
        echo "🚀 Bạn có thể mở game từ app drawer hoặc home screen"
        
        # Launch game
        echo ""
        echo "Bạn có muốn mở game ngay không? (y/n)"
        read -r LAUNCH_RESPONSE
        if [[ $LAUNCH_RESPONSE == "y" || $LAUNCH_RESPONSE == "Y" ]]; then
            echo "🎯 Đang mở game..."
            adb shell am start -n com.lamgame.oquan/com.unity3d.player.UnityPlayerActivity
        fi
    else
        echo "❌ CÀI ĐẶT THẤT BẠI!"
        echo "Vui lòng kiểm tra:"
        echo "- Điện thoại có cho phép cài đặt từ unknown sources không"
        echo "- USB Debugging có bật không"
        echo "- Có đủ dung lượng trống không"
    fi
else
    echo ""
    echo "🔧 Sau khi build xong APK, bạn có thể cài đặt thủ công bằng lệnh:"
    echo "   adb install \"$APK_PATH\""
fi

echo ""
echo "✨ Hoàn thành!"