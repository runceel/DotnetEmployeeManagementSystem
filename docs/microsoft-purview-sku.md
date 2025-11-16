# Microsoft Purview SKU 情報

## 概要

Microsoft Purview は、Microsoft が提供するデータガバナンス、コンプライアンス、リスク管理のための統合ソリューションです。このドキュメントでは、Microsoft Purview の利用可能な SKU（Stock Keeping Unit）と価格モデルについて説明します。

## 価格モデル

Microsoft Purview は2つの主要な価格モデルを提供しています：

### 1. ユーザーごとのライセンスモデル

Microsoft 365 ワークロード（Exchange、SharePoint、Teams、エンドポイント）向けのコンプライアンス、リスク管理、ガバナンス機能を対象としています。

#### Microsoft Purview Suite
- **価格**: $12.00 USD/ユーザー/月（年間契約）
- **旧名称**: Microsoft 365 E5 Compliance
- **対象機能**:
  - データライフサイクル管理
  - インサイダーリスク管理
  - eDiscovery
  - 監査（Audit）
  - 情報保護
  - データ損失防止（DLP）

#### 利用可能なプラン
以下の Microsoft 365 プランのアドオンとして利用可能：
- Microsoft 365 E3/E5
- Microsoft 365 A5
- Microsoft 365 F5
- Microsoft 365 G5

**重要**: サービスから利益を受ける各ユーザーにライセンスが必要です。

### 2. 従量課金制（Pay-As-You-Go / PAYG）モデル

Microsoft 365 以外のワークロード（Azure、AWS、オンプレミスデータ、生成 AI アプリなど）向けに設計されています。

#### データガバナンス

**課金メーター**:
1. **日次ユニーク管理資産**: 統合カタログでアクティブに管理されている資産
2. **データガバナンス処理ユニット**: データ品質チェックなどのデータヘルス管理実行ごと

**課金対象**:
- 管理されている資産のみ（単にスキャンされただけで、ガバナンス概念にアタッチされていない資産は課金されません）

#### データセキュリティとコンプライアンス（2025年5月開始）

AI アプリ、エージェント、転送中のデータ保護に焦点を当てた新しい PAYG メーター：

| 機能 | 価格 |
|------|------|
| **転送中の保護（Information Protection/DLP）** | $0.50 USD/10,000 リクエスト |
| **インサイダーリスク管理** | $25.00 USD/10,000 監視イベント |
| **監査（Audit Standard）** | $15.00 USD/100万監査レコード |

**特徴**:
- 変動するワークロードに最適
- 予測不可能なワークロードに対応
- ボリュームベースの柔軟な価格設定

### 3. クラシック Data Map と従来の価格設定

従来の Azure Purview（現在の Microsoft Purview Data Map Classic）:
- サブスクリプションベースの PAYG モデル
- よりシンプルな SKU 構成
- 追加コスト:
  - プライベートエンドポイント
  - セルフホスト統合ランタイム
  - Event Hubs

## SKU の選択ガイド

### ユーザーごとのライセンスが適している場合
- 小規模組織
- Microsoft 365 に大きく投資している組織
- 予測可能なコスト管理を望む場合

### 従量課金制が適している場合
- 大企業
- データ駆動型組織
- ハイブリッド/マルチクラウド環境
- AI ワークロードを持つ組織
- データと複雑性に応じたスケーリングが必要な場合

## 重要な考慮事項

1. **両方のモデルを併用可能**: データ環境とニーズに応じて、ユーザーごとのライセンスと従量課金制を組み合わせて使用できます。

2. **Azure サブスクリプションとの関連付け**: PAYG サービスを利用するには、M365 テナントを Azure サブスクリプションに関連付ける必要がある場合があります。

3. **ライセンスガイダンス**: コンプライアンスと最適なコスト管理を確保するために、ライセンスガイダンスの確認が重要です。

4. **地域別価格**: 価格は地域によって異なる場合があります。

## 価格計算ツール

最新の価格情報と地域別の詳細については、以下をご利用ください：
- [Azure Purview Pricing Calculator](https://azure.microsoft.com/en-us/pricing/details/purview/)

## 参考資料

- [Microsoft Purview Suite 価格](https://www.microsoft.com/en-us/security/business/purview-suite-pricing)
- [Microsoft Purview サービス説明](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-purview-service-description)
- [Microsoft Purview ライセンスガイダンス](https://www.microsoft.com/licensing/guidance/Microsoft-Purview)
- [Microsoft Purview Data Governance 課金](https://learn.microsoft.com/en-us/purview/data-governance-billing)
- [Microsoft Purview 課金モデル](https://learn.microsoft.com/en-us/purview/purview-billing-models)

## 将来の統合について

このドキュメントは参考情報として提供されています。現時点では、この従業員管理システムは Microsoft Purview と統合されていませんが、将来的にデータガバナンスやコンプライアンス機能の追加を検討する際の参考資料としてご活用ください。

---

**最終更新日**: 2025年11月16日  
**情報源**: Microsoft 公式ドキュメント（2025年11月時点）
