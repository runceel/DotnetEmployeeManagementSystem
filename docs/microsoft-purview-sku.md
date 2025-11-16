# Microsoft Purview SKU 情報

> **情報源**: このドキュメントは Microsoft Learn の公式ドキュメントのみに基づいて作成されています。

## 概要

Microsoft Purview は、データのガバナンス、保護、管理を支援する包括的なソリューションセットです。Microsoft 365 とWindows/macOS のエンドポイント向けのユーザーごとのライセンスモデルと、Microsoft 365 以外のデータソース向けの従量課金モデルの2つの補完的な課金モデルをサポートしています。

## 利用可能なライセンスプラン（SKU）

### ユーザーごとのライセンスモデル

Microsoft 365 および Windows/macOS エンドポイントベースのアセットに Microsoft Purview の制御と保護を適用するための標準的なライセンスモデルです。

#### 主要な SKU と機能

| SKU | 含まれる機能 |
|-----|-------------|
| **Microsoft 365 E5/A5/G5** | Audit (Premium), eDiscovery (Premium), Communication Compliance, Insider Risk Management, Information Protection, DLP など全機能 |
| **Microsoft 365 E5/A5/G5/F5 Compliance** | コンプライアンス機能の完全セット（旧称: Microsoft 365 E5 Compliance） |
| **Microsoft 365 F5 Security & Compliance** | セキュリティとコンプライアンス機能のセット |
| **Microsoft 365 E5/A5/F5/G5 Information Protection and Governance** | 情報保護とガバナンス機能 |
| **Microsoft 365 E5/A5/G5/F5 eDiscovery and Audit** | eDiscovery と監査機能 |
| **Microsoft 365 E5/A5/G5/F5 Insider Risk Management** | インサイダーリスク管理機能 |
| **Microsoft 365 E3/A3/G3** | 基本的なコンプライアンス機能（eDiscovery Standard, Audit Standard など） |
| **Office 365 E5/A5/G5** | Office 365 のコンプライアンス機能 |
| **Microsoft 365 Purview Suite** | 包括的な Purview ソリューション（旧称: Microsoft 365 E5 Compliance） |

### ライセンスが必要なユーザー

以下のユーザーにはライセンスが必要です：

1. Microsoft Purview ポータルで使用するための Purview ロールが割り当てられたユーザー
2. Exchange メールボックス、OneDrive アカウント、Teams チャット、デバイスに関連付けられたユーザー（Purview ポリシーや機能が使用される場合）
3. SharePoint サイト、Microsoft 365 グループ、Teams チャネルメッセージの所有者またはメンバーロールを持つユーザー（訪問者または閲覧専用ロールのユーザーはライセンス不要）
4. 共有メールボックスまたはリソースメールボックス（特定の機能に必要）
5. eDiscovery のケースにおける保管者（custodian）

**注**: 非アクティブなメールボックスにはライセンスは不要です。

## 従量課金制（Pay-As-You-Go）モデル

Microsoft 365 以外のワークロード（Azure、AWS、オンプレミスデータ、生成 AI アプリケーションなど）に Microsoft Purview のデータセキュリティ、データガバナンス、データリスク、コンプライアンス保護機能を拡張するための消費ベースの課金モデルです。

### 前提条件

- Microsoft 365 テナントをアクティブな Azure サブスクリプションに関連付ける必要があります
- 一部の機能では、従量課金モデルを使用する前にユーザーごとのライセンスモデルの有効化が必要です

### データガバナンス機能

| ソリューション | 適用範囲 | 測定単位 |
|-------------|---------|---------|
| **Unified Catalog データキュレーション** | Microsoft Purview Unified Catalog で技術的資産をアクティブに管理する場合 | 日次ユニーク管理資産数 |
| **Unified Catalog データヘルス管理** | データ品質を管理し、ヘルス管理アクションを実行する場合 | データガバナンス処理ユニット（DGPU）消費数 |

#### データガバナンス処理ユニット（DGPU）

- 1 DGPU = 60分間のコンピュート時間
- Basic、Standard、Advanced の3つのパフォーマンスオプション（Basic がデフォルト）
- 消費量は以下に依存：
  - データ品質またはヘルス（メタデータ品質）ルールタイプ（標準またはカスタム）
  - データ量
  - ソースタイプ

**DGPU 生成例（Azure SQL DB, 100万行, Basic SKU）**:
- 空白チェック: 0.02 DGPU/ルール/実行
- 正規表現チェック: 0.02 DGPU/ルール/実行
- テーブルルックアップ: 0.03 DGPU/ルール/実行
- 一意性チェック: 0.02 DGPU/ルール/実行
- 重複チェック: 0.02 DGPU/ルール/実行

### データセキュリティ機能

| ソリューション | 適用範囲 | 測定単位 |
|-------------|---------|---------|
| **Data Security Investigations（プレビュー）** | 各調査に関連付けられたストレージ | 全調査のストレージGB/月、セキュリティコンピュートユニット消費数 |
| **Information Protection** | Microsoft 365 以外のデータソースに適用する機密ラベル | 保護ポリシー対象の資産数/日 |
| **Insider Risk Management** | Microsoft 365 以外の場所でのリスクの高い行動を検出（Cloud および生成 AI ポリシーインジケーター使用時） | データセキュリティ処理ユニット（日次ベース） |

### データリスクとコンプライアンス機能（生成 AI アプリとエージェント向け）

**注**: Microsoft 365 Copilot エクスペリエンスは課金されません。

| ソリューション | 適用範囲 | 測定単位 |
|-------------|---------|---------|
| **Audit ソリューション** | Microsoft 以外の生成 AI アプリケーションとのユーザーインタラクションの監査ログ | 処理された監査レコード数 |
| **Communication Compliance** | Microsoft 365 以外の AI インタラクションでの不適切またはリスクのあるインタラクションを検出（AI ポリシーインジケーター使用時） | スキャンされたテキストレコード数 |
| **Data Lifecycle Management** | AI インタラクションの保持ポリシー | 保持ポリシー対象の Microsoft 365 以外の Copilot または AI アプリのインタラクション（プロンプトとレスポンス）数 |
| **eDiscovery** | Microsoft 365 以外の AI アプリケーションデータのストレージと、標準ライセンステナント向け Microsoft Graph API の使用 | ストレージGB/日、エクスポートAPI使用GB/エクスポート |

### その他の従量課金ソリューション

| ソリューション | 測定単位 |
|-------------|---------|
| **On-demand Classification（プレビュー）** | スキャンごとに分類された資産数 |
| **Microsoft Security Copilot** | セキュリティコンピュートユニット（SCU） |
| **Network Data Security（プレビュー）** | エンドポイントデバイスからウェブサイト、クラウドアプリ、生成 AI アプリへのリクエスト数 |
| **Data Security for Gen AI Applications** | Microsoft 365 以外の AI インタラクション（プロンプト/レスポンス）のリクエストまたはメッセージ数 |
| **DLP for Cloud Apps in Edge for Business browser** | Edge for Business ブラウザからウェブサイト、クラウドアプリ、生成 AI アプリへのリクエスト数 |

## 主要機能の SKU 対応表

### Audit

| 機能 | E5 | E3 | E5/A5/G5/F5 Compliance | eDiscovery & Audit |
|------|----|----|------------------------|-------------------|
| Audit (Standard) | ✓ | ✓ | ✓ | ✓ |
| Audit (Premium) | ✓ | ✗ | ✓ | ✓ |

### eDiscovery

| 機能 | E5 | E3 | E5/A5/G5/F5 Compliance | eDiscovery & Audit |
|------|----|----|------------------------|-------------------|
| eDiscovery (Standard) | ✓ | ✓ | ✓ | ✓ |
| eDiscovery (Premium) | ✓ | ✗ | ✓ | ✓ |

### Data Loss Prevention (DLP)

| 機能 | E5/A5/G5 | E3/A3/G3 | E5/F5 Compliance | Info Protection & Governance |
|------|----------|----------|------------------|------------------------------|
| DLP (Exchange, SharePoint, OneDrive) | ✓ | ✓ | ✓ | ✓ |
| DLP for Teams | ✓ | ✗ | ✓ | ✓ |
| Endpoint DLP | ✓ | ✗ | ✓ | ✓ |

### Information Protection

| 機能 | E5/A5/G5 | E3/A3/G3 | E5/F5 Compliance | Info Protection & Governance |
|------|----------|----------|------------------|------------------------------|
| 手動機密ラベル | ✓ | ✓ | ✓ | ✓ |
| 自動機密ラベル（クライアントおよびサービス側） | ✓ | ✗ | ✓ | ✓ |
| Advanced Message Encryption | ✓ | ✗ | ✓ | ✓ |
| Customer Key | ✓ | ✗ | ✓ | ✓ |

### Insider Risk Management & Communication Compliance

| 機能 | E5/A5/G5 | E5/F5 Compliance | Insider Risk Management SKU |
|------|----------|------------------|----------------------------|
| Insider Risk Management | ✓ | ✓ | ✓ |
| Communication Compliance | ✓ | ✓ | ✓ |

## 課金設定

データガバナンス機能を使用するには、Microsoft Purview の従量課金課金の設定が必要です（2025年1月6日より有効）。

## コスト見積もりツール

従量課金機能の価格を理解し、予想される月額コストを見積もる：

- [Microsoft 従量課金価格](https://azure.microsoft.com/pricing/details/purview/)
- [Microsoft Purview 従量課金機能使用のコスト見積もり](https://azure.microsoft.com/pricing/calculator/)

## 参考資料（Microsoft Learn）

- [Microsoft Purview service description](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-purview-service-description)
- [Learn about Microsoft Purview billing models](https://learn.microsoft.com/en-us/purview/purview-billing-models)
- [Learn about data governance billing](https://learn.microsoft.com/en-us/purview/data-governance-billing)
- [Microsoft 365 guidance for security & compliance](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-365-security-compliance-licensing-guidance)

## 将来の統合について

このドキュメントは参考情報として提供されています。現時点では、この従業員管理システムは Microsoft Purview と統合されていませんが、将来的にデータガバナンスやコンプライアンス機能の追加を検討する際の参考資料としてご活用ください。

---

**最終更新日**: 2025年11月16日  
**情報源**: Microsoft Learn 公式ドキュメント
