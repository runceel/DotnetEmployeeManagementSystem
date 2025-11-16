# Microsoft Purview SKU Information

> **Source**: This document is based exclusively on official Microsoft Learn documentation.

## Overview

Microsoft Purview is a comprehensive set of solutions that helps organizations govern, protect, and manage data, wherever it lives. It supports two complementary billing models: per-user licensing for Microsoft 365 and Windows/macOS endpoints, and pay-as-you-go for non-Microsoft 365 data sources and certain other capabilities.

## Available License Plans (SKUs)

### Per-User Licensing Model

The standard licensing model that enables Microsoft Purview controls and protections for Microsoft 365 and Windows/macOS endpoint-based assets.

#### Key SKUs and Features

| SKU | Included Features |
|-----|-------------------|
| **Microsoft 365 E5/A5/G5** | Audit (Premium), eDiscovery (Premium), Communication Compliance, Insider Risk Management, Information Protection, DLP, and all features |
| **Microsoft 365 E5/A5/G5/F5 Compliance** | Complete compliance feature set (formerly Microsoft 365 E5 Compliance) |
| **Microsoft 365 F5 Security & Compliance** | Security and compliance feature set |
| **Microsoft 365 E5/A5/F5/G5 Information Protection and Governance** | Information protection and governance features |
| **Microsoft 365 E5/A5/G5/F5 eDiscovery and Audit** | eDiscovery and audit features |
| **Microsoft 365 E5/A5/G5/F5 Insider Risk Management** | Insider risk management features |
| **Microsoft 365 E3/A3/G3** | Basic compliance features (eDiscovery Standard, Audit Standard, etc.) |
| **Office 365 E5/A5/G5** | Office 365 compliance features |
| **Microsoft 365 Purview Suite** | Comprehensive Purview solutions (formerly Microsoft 365 E5 Compliance) |

### Users Who Need a License

Licenses are required for:

1. Users with a Purview role assigned for use in the Microsoft Purview portal
2. Users associated with Exchange mailboxes, OneDrive accounts, Teams chats, and devices (when Purview policies or features are used)
3. Users with owner or member roles in SharePoint sites, Microsoft 365 Groups, and Teams channel messages (visitors or view-only roles don't need a license)
4. Shared or resource mailboxes (for specific features)
5. Custodians in eDiscovery cases

**Note**: Inactive mailboxes don't require a license.

## Pay-As-You-Go (PAYG) Model

A consumption-based billing model that extends Microsoft Purview data security, data governance, and data risk and compliance protection capabilities beyond Microsoft 365 and Windows/macOS environments to non-Microsoft 365 locations (Azure, AWS, on-premises data, generative AI applications, etc.).

### Prerequisites

- Must associate Microsoft 365 tenant with an active Azure subscription
- Some features require per-user licensing model to be enabled before using pay-as-you-go

### Data Governance Capabilities

| Solution | Applies To | Unit of Measure |
|----------|-----------|-----------------|
| **Unified Catalog Data Curation** | When actively curating and managing technical assets in Microsoft Purview Unified Catalog | Number of unique assets governed/day |
| **Unified Catalog Data Health Management** | When managing data quality and taking health management actions | Number of data governance processing units (DGPUs) consumed |

#### Data Governance Processing Units (DGPU)

- 1 DGPU = 60 minutes of compute time
- Three performance options: Basic, Standard, Advanced (Basic is default)
- Consumption depends on:
  - Data quality or health (metadata quality) rule type (out of box or custom)
  - Volume of data
  - Source type

**DGPU Generation Examples (Azure SQL DB, 1 Million rows, Basic SKU)**:
- Empty/blank check: 0.02 DGPU/rule/run
- StringFormat Regex/Like check: 0.02 DGPU/rule/run
- Table lookup (1 million row reference table): 0.03 DGPU/rule/run
- Unique check: 0.02 DGPU/rule/run
- Duplicate check (3 column combo): 0.02 DGPU/rule/run

### Data Security Capabilities

| Solution | Applies To | Unit of Measure |
|----------|-----------|-----------------|
| **Data Security Investigations (preview)** | Storage associated with each investigation | Number of GB of stored data for all investigations/month, Security Compute units consumed |
| **Information Protection** | Sensitivity labels applied to non-Microsoft 365 data sources | Number of assets in scope of protection policy/day |
| **Insider Risk Management** | Detect risky behavior for non-Microsoft 365 locations when using Cloud and generative AI policy indicators | Data Security processing unit measured daily |

### Data Risk and Compliance Capabilities (for Generative AI Apps and Agents)

**Note**: Microsoft 365 Copilot experiences are not charged.

| Solution | Applies To | Unit of Measure |
|----------|-----------|-----------------|
| **Audit Solutions** | Audit logs for user interactions with non-Microsoft generative AI applications | Number of audit records processed |
| **Communication Compliance** | Detect inappropriate or risky interactions for non-Microsoft 365 AI interactions when using AI policy indicators | Number of text records scanned |
| **Data Lifecycle Management** | Retention policies for AI interactions | Number of non-Microsoft 365 Copilot or AI App interactions (prompts and responses) under retention policy |
| **eDiscovery** | Storage of non-Microsoft 365 AI application data and usage of Microsoft Graph APIs for standard licensed tenants | Number of GB stored/day, GB/export |

### Other Pay-As-You-Go Solutions

| Solution | Unit of Measure |
|----------|-----------------|
| **On-demand Classification (preview)** | Assets classified per scan |
| **Microsoft Security Copilot** | Security Compute Units (SCU) |
| **Network Data Security (preview)** | Number of requests from endpoint device to website, cloud app, or generative AI app |
| **Data Security for Gen AI Applications** | Number of requests or messages for non-Microsoft 365 AI interactions (prompts or responses) |
| **DLP for Cloud Apps in Edge for Business browser** | Number of requests sent from Edge for Business browser to website, cloud app, or generative AI app |

## Key Feature SKU Mapping

### Audit

| Feature | E5 | E3 | E5/A5/G5/F5 Compliance | eDiscovery & Audit |
|---------|----|----|------------------------|-------------------|
| Audit (Standard) | ✓ | ✓ | ✓ | ✓ |
| Audit (Premium) | ✓ | ✗ | ✓ | ✓ |

### eDiscovery

| Feature | E5 | E3 | E5/A5/G5/F5 Compliance | eDiscovery & Audit |
|---------|----|----|------------------------|-------------------|
| eDiscovery (Standard) | ✓ | ✓ | ✓ | ✓ |
| eDiscovery (Premium) | ✓ | ✗ | ✓ | ✓ |

### Data Loss Prevention (DLP)

| Feature | E5/A5/G5 | E3/A3/G3 | E5/F5 Compliance | Info Protection & Governance |
|---------|----------|----------|------------------|------------------------------|
| DLP (Exchange, SharePoint, OneDrive) | ✓ | ✓ | ✓ | ✓ |
| DLP for Teams | ✓ | ✗ | ✓ | ✓ |
| Endpoint DLP | ✓ | ✗ | ✓ | ✓ |

### Information Protection

| Feature | E5/A5/G5 | E3/A3/G3 | E5/F5 Compliance | Info Protection & Governance |
|---------|----------|----------|------------------|------------------------------|
| Manual Sensitivity Labeling | ✓ | ✓ | ✓ | ✓ |
| Automatic Sensitivity Labeling (Client and Service-side) | ✓ | ✗ | ✓ | ✓ |
| Advanced Message Encryption | ✓ | ✗ | ✓ | ✓ |
| Customer Key | ✓ | ✗ | ✓ | ✓ |

### Insider Risk Management & Communication Compliance

| Feature | E5/A5/G5 | E5/F5 Compliance | Insider Risk Management SKU |
|---------|----------|------------------|----------------------------|
| Insider Risk Management | ✓ | ✓ | ✓ |
| Communication Compliance | ✓ | ✓ | ✓ |

## Billing Setup

To use the data governance experience in the Microsoft Purview portal, pay-as-you-go billing setup is required (effective January 6, 2025).

## Cost Estimator Tools

Understand pricing and estimate expected monthly costs for pay-as-you-go capabilities:

- [Microsoft pay-as-you-go pricing](https://azure.microsoft.com/pricing/details/purview/)
- [Estimate your costs for Microsoft Purview pay-as-you-go feature use](https://azure.microsoft.com/pricing/calculator/)

## References (Microsoft Learn)

- [Microsoft Purview service description](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-purview-service-description)
- [Learn about Microsoft Purview billing models](https://learn.microsoft.com/en-us/purview/purview-billing-models)
- [Learn about data governance billing](https://learn.microsoft.com/en-us/purview/data-governance-billing)
- [Microsoft 365 guidance for security & compliance](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-365-security-compliance-licensing-guidance)

## Future Integration

This document is provided as reference information. Currently, this Employee Management System is not integrated with Microsoft Purview, but this documentation can serve as a reference if data governance and compliance features are considered for future integration.

---

**Last Updated**: November 16, 2025  
**Source**: Microsoft Learn Official Documentation
