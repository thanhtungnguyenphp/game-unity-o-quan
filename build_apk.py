#!/usr/bin/env python3
import os
import subprocess
import sys

def build_apk():
    project_path = "/Users/Shared/jerry/game_unity/game-o-quan"
    unity_path = "/Applications/Unity/Hub/Editor/6000.1.9f1/Unity/Unity.app/Contents/MacOS/Unity"
    build_path = os.path.join(project_path, "Build")
    apk_path = os.path.join(build_path, "game-o-quan.apk")
    
    # Tạo thư mục Build nếu chưa có
    os.makedirs(build_path, exist_ok=True)
    
    print("🚀 Bắt đầu build APK...")
    print(f"📁 Project path: {project_path}")
    print(f"📱 APK output: {apk_path}")
    
    # Unity command line arguments
    unity_args = [
        unity_path,
        "-batchmode",
        "-quit", 
        "-projectPath", project_path,
        "-executeMethod", "BuildScript.CommandLineBuild",
        "-buildTarget", "Android"
    ]
    
    try:
        # Chạy Unity build command
        result = subprocess.run(unity_args, capture_output=True, text=True)
        
        if os.path.exists(apk_path):
            print("✅ Build thành công!")
            print(f"📦 APK đã được tạo tại: {apk_path}")
            
            # Hiển thị kích thước file
            size = os.path.getsize(apk_path)
            print(f"📊 Kích thước APK: {size / (1024*1024):.2f} MB")
            
            return apk_path
        else:
            print("❌ Build thất bại!")
            print(f"Unity stdout: {result.stdout}")
            print(f"Unity stderr: {result.stderr}")
            return None
            
    except Exception as e:
        print(f"❌ Lỗi khi build: {e}")
        return None

def install_apk(apk_path):
    if not apk_path or not os.path.exists(apk_path):
        print("❌ Không tìm thấy file APK để cài đặt")
        return False
        
    print("📱 Cài đặt APK lên điện thoại...")
    
    try:
        # Kiểm tra thiết bị kết nối
        result = subprocess.run(["adb", "devices"], capture_output=True, text=True)
        if "device" not in result.stdout:
            print("❌ Không tìm thấy thiết bị Android nào được kết nối")
            return False
            
        # Gỡ cài đặt phiên bản cũ (nếu có)
        print("🗑️  Gỡ cài đặt phiên bản cũ...")
        subprocess.run(["adb", "uninstall", "com.lamgame.oquan"], capture_output=True)
        
        # Cài đặt APK mới
        print("⬇️  Đang cài đặt APK...")
        install_result = subprocess.run(["adb", "install", apk_path], capture_output=True, text=True)
        
        if install_result.returncode == 0:
            print("✅ Cài đặt thành công!")
            print("🎮 Game đã sẵn sàng trên điện thoại của bạn!")
            return True
        else:
            print(f"❌ Cài đặt thất bại: {install_result.stderr}")
            return False
            
    except Exception as e:
        print(f"❌ Lỗi khi cài đặt: {e}")
        return False

if __name__ == "__main__":
    print("🎮 === BUILD GAME Ổ QUAN ===")
    
    # Build APK
    apk_path = build_apk()
    
    if apk_path:
        # Cài đặt APK
        install_apk(apk_path)
    else:
        print("❌ Không thể build APK")
        sys.exit(1)