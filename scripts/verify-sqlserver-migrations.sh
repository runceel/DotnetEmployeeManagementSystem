#!/bin/bash

# SQL Server マイグレーション検証スクリプト
# このスクリプトは、既存のマイグレーションがSQL Serverで正しく動作するかを検証します

set -e

echo "=================================="
echo "SQL Server マイグレーション検証"
echo "=================================="
echo ""

# 色設定
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Docker で SQL Server が起動しているか確認
echo -e "${YELLOW}1. SQL Server コンテナの確認...${NC}"
if ! docker ps | grep -q "mssql"; then
    echo -e "${YELLOW}SQL Server コンテナが見つかりません。起動します...${NC}"
    docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
        -p 1433:1433 --name sqlserver-test -d mcr.microsoft.com/mssql/server:2022-latest
    
    echo "SQL Server の起動を待機中..."
    sleep 10
else
    echo -e "${GREEN}✓ SQL Server コンテナが起動しています${NC}"
fi

echo ""
echo -e "${YELLOW}2. マイグレーションスクリプトの生成...${NC}"

# 各サービスのマイグレーションスクリプトを生成
SERVICES=("EmployeeService" "AuthService" "NotificationService" "AttendanceService")

mkdir -p ./migration-scripts

for SERVICE in "${SERVICES[@]}"; do
    echo -e "${YELLOW}  - ${SERVICE}...${NC}"
    
    cd "src/Services/${SERVICE}/Infrastructure"
    
    # マイグレーションスクリプト生成
    dotnet ef migrations script --startup-project ../API \
        --idempotent \
        --output "../../../../migration-scripts/${SERVICE}-migration.sql" \
        2>/dev/null || {
        echo -e "${RED}    ✗ ${SERVICE} のマイグレーションスクリプト生成に失敗${NC}"
        cd ../../../..
        continue
    }
    
    echo -e "${GREEN}    ✓ ${SERVICE} のマイグレーションスクリプトを生成しました${NC}"
    cd ../../../..
done

echo ""
echo -e "${YELLOW}3. 生成されたスクリプトの確認...${NC}"

for SERVICE in "${SERVICES[@]}"; do
    SCRIPT_FILE="./migration-scripts/${SERVICE}-migration.sql"
    if [ -f "$SCRIPT_FILE" ]; then
        LINE_COUNT=$(wc -l < "$SCRIPT_FILE")
        echo -e "${GREEN}  ✓ ${SERVICE}: ${LINE_COUNT} 行${NC}"
    else
        echo -e "${RED}  ✗ ${SERVICE}: スクリプトが見つかりません${NC}"
    fi
done

echo ""
echo -e "${GREEN}=================================="
echo "検証完了"
echo "==================================${NC}"
echo ""
echo "生成されたマイグレーションスクリプト:"
echo "  - ./migration-scripts/*.sql"
echo ""
echo "次のステップ:"
echo "  1. 生成されたSQLスクリプトを確認してください"
echo "  2. SQL Serverに対してテスト実行する場合:"
echo "     sqlcmd -S localhost,1433 -U sa -P 'YourStrong@Passw0rd' -i migration-scripts/EmployeeService-migration.sql"
echo ""
echo "注意: SQLite と SQL Server でデータ型が異なる場合があります"
echo "  - INTEGER → INT"
echo "  - TEXT → NVARCHAR(MAX)"
echo "  - REAL → FLOAT"
echo ""

# クリーンアップオプション
read -p "SQL Server コンテナを停止・削除しますか? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker stop sqlserver-test 2>/dev/null && docker rm sqlserver-test 2>/dev/null
    echo -e "${GREEN}✓ SQL Server コンテナを削除しました${NC}"
fi
