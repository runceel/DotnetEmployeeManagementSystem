# Microsoft Purview SKU Information

## Overview

Microsoft Purview is Microsoft's unified solution for data governance, compliance, and risk management. This document explains the available SKUs (Stock Keeping Units) and pricing models for Microsoft Purview.

## Pricing Models

Microsoft Purview offers two main pricing models:

### 1. Per-User Licensing Model

Designed for compliance, risk management, and governance features for Microsoft 365 workloads (Exchange, SharePoint, Teams, and endpoints).

#### Microsoft Purview Suite
- **Price**: $12.00 USD per user/month (annual commitment)
- **Previously known as**: Microsoft 365 E5 Compliance
- **Features included**:
  - Data Lifecycle Management
  - Insider Risk Management
  - eDiscovery
  - Audit
  - Information Protection
  - Data Loss Prevention (DLP)

#### Available Plans
Available as an add-on to the following Microsoft 365 plans:
- Microsoft 365 E3/E5
- Microsoft 365 A5
- Microsoft 365 F5
- Microsoft 365 G5

**Important**: Each user who benefits from the service requires a license.

### 2. Pay-As-You-Go (PAYG) Consumption Model

Designed for non-Microsoft 365 workloads such as Azure, AWS, on-premises data, and generative AI applications.

#### Data Governance

**Billing Meters**:
1. **Unique Governed Assets per Day**: Assets actively curated in the Unified Catalog
2. **Data Governance Processing Units per Run**: For data health management such as data quality checks

**What's Charged**:
- Only governed assets (assets that are scanned but not attached to governance concepts are not charged)

#### Data Security and Compliance (Starting May 2025)

New PAYG meters focused on protecting AI apps, agents, and data in transit:

| Feature | Pricing |
|---------|---------|
| **In Transit Protection (Information Protection/DLP)** | $0.50 USD per 10,000 requests |
| **Insider Risk Management** | $25.00 USD per 10,000 monitored events |
| **Audit Standard** | $15.00 USD per 1 million audit records ingested |

**Characteristics**:
- Ideal for fluctuating workloads
- Supports unpredictable workloads
- Flexible volume-based pricing

### 3. Classic Data Map and Legacy Pricing

The legacy Azure Purview (now Microsoft Purview Data Map Classic):
- Subscription-based PAYG model
- Simpler SKU structure
- Additional costs:
  - Private endpoints
  - Self-hosted integration runtime
  - Event Hubs

## Choosing the Right SKU

### When Per-User Licensing is Suitable
- Small organizations
- Organizations heavily invested in Microsoft 365
- When predictable cost management is desired

### When Pay-As-You-Go is Suitable
- Large enterprises
- Data-driven organizations
- Hybrid/multi-cloud environments
- Organizations with AI workloads
- When scaling with data and complexity is required

## Important Considerations

1. **Both Models Can Be Combined**: Depending on your data environment and needs, you can use both per-user licensing and pay-as-you-go together.

2. **Azure Subscription Association**: To use PAYG services, you may need to associate your M365 tenant with an Azure subscription.

3. **Licensing Guidance**: Reviewing licensing guidance is crucial to ensure compliance and optimal cost management.

4. **Regional Pricing**: Prices may vary by region.

## Pricing Calculator

For the most current and regional pricing information, use:
- [Azure Purview Pricing Calculator](https://azure.microsoft.com/en-us/pricing/details/purview/)

## References

- [Microsoft Purview Suite Pricing](https://www.microsoft.com/en-us/security/business/purview-suite-pricing)
- [Microsoft Purview Service Description](https://learn.microsoft.com/en-us/office365/servicedescriptions/microsoft-365-service-descriptions/microsoft-365-tenantlevel-services-licensing-guidance/microsoft-purview-service-description)
- [Microsoft Purview Licensing Guidance](https://www.microsoft.com/licensing/guidance/Microsoft-Purview)
- [Microsoft Purview Data Governance Billing](https://learn.microsoft.com/en-us/purview/data-governance-billing)
- [Microsoft Purview Billing Models](https://learn.microsoft.com/en-us/purview/purview-billing-models)

## Future Integration

This document is provided as reference information. Currently, this Employee Management System is not integrated with Microsoft Purview, but this documentation can serve as a reference if data governance and compliance features are considered for future integration.

---

**Last Updated**: November 16, 2025  
**Source**: Microsoft Official Documentation (as of November 2025)
