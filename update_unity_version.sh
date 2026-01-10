#!/bin/bash

# Unity Version Update Script
# Updates project from 2022.3.29f1 to 2022.3.62f3

echo "🔧 Unity Version Update Script"
echo "================================"
echo ""

PROJECT_PATH="/Users/Shared/jerry/game_unity/game-unity-o-quan"
OLD_VERSION="2022.3.29f1"
NEW_VERSION="2022.3.62f3"

echo "📋 Current Version: $OLD_VERSION"
echo "📋 Target Version:  $NEW_VERSION"
echo ""

# Check if Unity is running
if pgrep -x "Unity" > /dev/null; then
    echo "⚠️  Unity is currently running!"
    echo "   Please close Unity Editor first."
    echo ""
    exit 1
fi

echo "✅ Unity is not running"
echo ""

# Backup current state
echo "💾 Creating backup..."
cd "$PROJECT_PATH"
BACKUP_FILE="../game-unity-o-quan-backup-$(date +%Y%m%d-%H%M%S).tar.gz"
tar -czf "$BACKUP_FILE" \
    --exclude="Library" \
    --exclude="Temp" \
    --exclude="Build" \
    --exclude="Logs" \
    . 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✅ Backup created: $BACKUP_FILE"
else
    echo "⚠️  Backup failed, but continuing..."
fi
echo ""

# Update ProjectVersion.txt
echo "📝 Updating ProjectVersion.txt..."
cat > ProjectSettings/ProjectVersion.txt << EOF
m_EditorVersion: $NEW_VERSION
m_EditorVersionWithRevision: $NEW_VERSION (ef5f1e8c5219)
EOF

if [ $? -eq 0 ]; then
    echo "✅ ProjectVersion.txt updated"
else
    echo "❌ Failed to update ProjectVersion.txt"
    exit 1
fi
echo ""

# Delete Library folder for clean rebuild
echo "🗑️  Removing Library folder for clean rebuild..."
if [ -d "Library" ]; then
    rm -rf Library/
    echo "✅ Library folder removed"
else
    echo "ℹ️  Library folder doesn't exist"
fi
echo ""

# Commit changes
echo "💾 Committing version update to git..."
git add ProjectSettings/ProjectVersion.txt
git commit -m "Update Unity version to $NEW_VERSION (security fix)" 2>/dev/null

if [ $? -eq 0 ]; then
    echo "✅ Changes committed to git"
else
    echo "ℹ️  No changes to commit (or git error)"
fi
echo ""

echo "================================"
echo "✅ Update Complete!"
echo ""
echo "📌 Next steps:"
echo "   1. Open Unity Hub"
echo "   2. Select this project"
echo "   3. Choose Unity version: $NEW_VERSION"
echo "   4. Wait for Unity to reimport assets (5-10 minutes)"
echo "   5. Check Console for any errors"
echo ""
echo "🔗 Or open directly with:"
echo "   open -a /Applications/Unity/Hub/Editor/$NEW_VERSION/Unity.app $PROJECT_PATH"
echo ""
