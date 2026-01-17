# .NET 10 Upgrade Plan

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
5. [Risk Management](#risk-management)
6. [Testing & Validation Strategy](#testing--validation-strategy)
7. [Complexity & Effort Assessment](#complexity--effort-assessment)
8. [Source Control Strategy](#source-control-strategy)
9. [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description
Upgrade all 25 projects in the DotnetEmployeeManagementSystem solution from .NET 9 to .NET 10 (Long Term Support).

### Current State
- **Total Projects**: 25 (4 API services, 1 Blazor web app, 1 AppHost, 1 ServiceDefaults library, 12 domain/application/infrastructure layers, 5 test projects)
- **Current Framework**: All projects target net9.0
- **Total Issues Identified**: 218 (69 mandatory, 144 potential, 5 optional)
- **Affected Files**: 42 code files requiring modifications
- **Lines of Code**: 13,593 total (estimated 149+ LOC to modify, ~1.1% of codebase)

### Target State
- **Target Framework**: net10.0 for all 25 projects
- **Architecture**: Microservices with Clean Architecture (Domain, Application, Infrastructure, API layers)
- **Primary Services**: EmployeeService, AuthService, AttendanceService, NotificationService

### Selected Migration Strategy
**Bottom-Up (Dependency-First) Strategy** with incremental tier-based migration.

**Rationale**:
- Clear 5-tier dependency hierarchy (Level 0-4)
- No circular dependencies detected
- Medium-sized solution (25 projects) benefits from staged approach
- Risk mitigation: upgrades stable foundation before dependent layers
- 3 high-risk projects (AuthService.Infrastructure with IdentityModel issues, BlazorWeb with 53 API compatibility issues, EmployeeService.Integration.Tests) require careful handling

### Complexity Assessment

**Classification**: **Medium Complexity**

**Key Metrics**:
- Projects: 25 (?15 threshold for simple exceeded)
- Dependency depth: 5 levels (?4 threshold exceeded)
- High-risk projects: 3 (AuthService.Infrastructure, BlazorWeb, EmployeeService.Integration.Tests)
- No security vulnerabilities detected
- No circular dependencies

**Primary Challenge**: IdentityModel & Claims-based Security APIs
- 44 binary incompatible issues across AuthService.Infrastructure, AuthService.Tests, and EmployeeService.Integration.Tests
- JWT token handling requires migration to modern identity stack
- WIF (Windows Identity Foundation) APIs replaced in .NET 10

### Critical Issues
1. **IdentityModel Breaking Changes** (44 issues, 29.5% of all API issues)
   - `System.IdentityModel.Tokens.Jwt` namespace APIs are binary incompatible
   - Affects: AuthService.Infrastructure (17 issues), AuthService.Tests (20 issues), EmployeeService.Integration.Tests (13 issues)
   - Migration path: Use Microsoft.IdentityModel.* packages (modern identity stack)

2. **HttpContent Behavioral Changes** (72 issues)
   - Behavioral changes in `System.Net.Http.HttpContent`
   - Affects multiple test projects and API clients
   - Low impact but requires runtime testing

3. **Deprecated NuGet Packages** (4 projects)
   - Some packages marked as deprecated in AppHost, AttendanceService.API, EmployeeService.API, NotificationService.API
   - Requires package replacement or version updates

### Recommended Approach
**Incremental Tier-Based Migration** following Bottom-Up strategy:
- Migrate 5 tiers sequentially (Level 0 Å® Level 4)
- Each tier fully upgraded, tested, and validated before proceeding
- High-risk projects get additional validation steps
- Estimated 8-10 iteration cycles for detailed planning

### Iteration Strategy
**Phase-based approach** (one iteration per tier):
- **Phase 1 (Foundation)**: Skeleton, discovery, strategy - 3 iterations
- **Phase 2 (Foundation Details)**: Dependency analysis, migration strategy, project stubs - 3 iterations
- **Phase 3 (Tier Details)**: 5 iterations (one per dependency tier Level 0-4)
- **Phase 4 (Finalization)**: Success criteria, source control - 1 iteration

**Total**: ~12 iterations expected

---

## Migration Strategy

### Approach Selection

**Selected Strategy**: **Incremental Tier-Based Migration** using Bottom-Up (Dependency-First) approach

**Justification**:

1. **Solution Characteristics**:
   - 25 projects with clear 5-tier hierarchy
   - Dependency depth of 5 levels
   - 3 high-risk projects requiring careful handling
   - No circular dependencies (clean architecture)
   - Mixed project types (APIs, libraries, web app, tests)

2. **Why Not All-At-Once**:
   - ? Medium-sized solution (25 projects) increases coordination complexity
   - ? High-risk projects (AuthService.Infrastructure, BlazorWeb, EmployeeService.Integration.Tests) need isolated validation
   - ? IdentityModel breaking changes require careful testing before downstream consumption
   - ? 42 affected files across multiple services - errors would be difficult to isolate

3. **Why Bottom-Up Strategy**:
   - ? Clear tier boundaries enable stable validation points
   - ? Dependencies always on same or newer framework (no multi-targeting)
   - ? Foundation projects (Domain, Shared) upgraded first - lowest risk
   - ? Issues isolated to current tier (easier debugging)
   - ? Each tier builds on proven, stable foundation
   - ? Learning from early tiers applies to later ones

4. **Risk Mitigation**:
   - High-risk projects handled individually with extra validation
   - Each tier fully tested before proceeding
   - Breaking changes (IdentityModel) addressed in isolation
   - Incremental approach allows pause/rollback if needed

---

### Bottom-Up Strategy Rationale

**Core Principle**: Upgrade projects sequentially from leaf nodes (no dependencies) upward through dependency chain to entry-point applications.

**Tier Progression**:
```
Tier 1 (Foundation)    Å® Tier 2 (Application Logic) Å® 
Tier 3 (Infrastructure) Å® Tier 4 (API Services)     Å® 
Tier 5 (App Hosts)
```

**Advantages for This Solution**:
1. **Stability**: Each tier builds on already-upgraded, tested foundation
2. **No Multi-Targeting**: Domain libraries on net10.0 before Application layers consume them
3. **Clear Validation**: Test each tier before proceeding (5 checkpoints)
4. **Easier Debugging**: Issues isolated to current tier (e.g., IdentityModel issues only in Tier 3-5)
5. **Risk Distribution**: High-risk projects spread across tiers (Tier 2: BlazorWeb, Tier 3: AuthService.Infrastructure, Tier 5: Integration.Tests)

**Challenges Accepted**:
- Longer total timeline (~5 phases vs 1)
- Benefits realized late (APIs upgraded in Phase 4)
- Requires strict tier ordering discipline

---

### Dependency-Based Ordering Rationale

**Bottom-Up Ordering Principles Applied**:

1. **Tier 0 (Implicit)**: External NuGet packages
   - Already compatible or have known upgrade paths
   - No action needed before project upgrades

2. **Tier 1 (Level 0)**: Projects with zero internal project references
   - 6 projects: 4 Domain layers + ServiceDefaults + Shared.Contracts
   - Verified: No project dependencies in .csproj files
   - Reasoning: Can be upgraded in parallel or as single batch

3. **Tier N+1 Rule**: Projects depending only on Tiers 0 through N
   - **Tier 2** depends only on Tier 1 ?
   - **Tier 3** depends only on Tiers 1-2 ?
   - **Tier 4** depends only on Tiers 1-3 ?
   - **Tier 5** depends only on Tiers 1-4 ?

4. **Verification**: No project in Tier N depends on Tier N+1
   - Confirmed via dependency graph analysis
   - No circular dependencies detected ?

**Critical Constraints**:
- ? Cannot upgrade Tier 2 until Tier 1 complete
- ? Cannot upgrade Tier 3 until Tier 2 complete
- ? Cannot upgrade Tier 4 until Tier 3 complete
- ? Cannot upgrade Tier 5 until Tier 4 complete

---

### Parallel vs Sequential Execution

**Within-Tier Parallelization**:

- **Tier 1**: ? All 6 projects can be upgraded in parallel (no inter-dependencies)
- **Tier 2**: ?? Partial parallelization
  - 4 Application layers can be parallel (each only depends on Tier 1)
  - BlazorWeb can be parallel (depends on Tier 1)
  - 2 Domain.Tests can be parallel (each depends on specific Domain project from Tier 1)
- **Tier 3**: ? All 5 projects can be upgraded in parallel (each depends only on Tier 1-2)
- **Tier 4**: ? All 5 projects can be upgraded in parallel (each depends only on Tier 1-3)
- **Tier 5**: ? Both projects can be upgraded in parallel (each has distinct dependency paths)

**Sequential Requirements Between Tiers**:
- ? Strict sequential: Tier N must be 100% complete before Tier N+1 begins
- ? Within-tier parallelization: Projects in same tier can proceed simultaneously
- ?? High-risk projects: May require sequential handling even within tier (e.g., AuthService.Infrastructure before AuthService.API testing)

**Recommendation**: 
- Batch projects within same tier into single operation
- Execute tiers strictly sequentially
- Treat high-risk projects as separate sub-phases with additional validation

---

### Phase Definitions

#### Phase 1: Foundation Layer (Tier 1)
**Projects**: 6 (AttendanceService.Domain, AuthService.Domain, EmployeeService.Domain, NotificationService.Domain, ServiceDefaults, Shared.Contracts)

**Scope**:
- Update TargetFramework to net10.0 (6 projects)
- Update NuGet packages (AuthService.Domain: 1 package, ServiceDefaults: 4 packages)
- Verify no breaking changes (pure domain models)
- Run unit tests for Domain.Tests (if any)

**Success Criteria**:
- All 6 projects build successfully on net10.0
- No compiler errors or warnings
- Domain.Tests pass (Tier 2, but test Tier 1 code)
- No regression in downstream projects still on net9.0

**Estimated Effort**: Low

---

#### Phase 2: Application & Frontend Layer (Tier 2)
**Projects**: 7 (4 Application layers, BlazorWeb, 2 Domain.Tests)

**Scope**:
- Update TargetFramework to net10.0 (7 projects)
- Update NuGet packages (NotificationService.Application: 1 package)
- Address BlazorWeb's 53 API behavioral changes (HttpContent, Uri)
- Run unit tests for Application layers
- Validate BlazorWeb UI functionality

**Success Criteria**:
- All 7 projects build successfully on net10.0
- Application unit tests pass
- Domain.Tests pass
- BlazorWeb runs without runtime errors
- HttpContent behavioral changes validated through manual testing
- No regression in downstream projects still on net9.0

**Estimated Effort**: Medium (BlazorWeb high-risk requires extra validation)

**Sub-Phases**:
- 2A: Application layers (4 projects) + Domain.Tests (2 projects) - batch
- 2B: BlazorWeb (1 project) - separate validation

---

#### Phase 3: Infrastructure Layer (Tier 3)
**Projects**: 5 (4 Infrastructure layers, EmployeeService.Application.Tests)

**Scope**:
- Update TargetFramework to net10.0 (5 projects)
- Update NuGet packages (AttendanceService: 2, AuthService: 3, EmployeeService: 2, NotificationService: 3)
- **Critical**: Migrate AuthService.Infrastructure IdentityModel APIs (15 binary incompatible + 2 source incompatible)
- Replace JWT token handling with Microsoft.IdentityModel.* packages
- Update Entity Framework Core usage
- Run integration tests for Infrastructure layers

**Success Criteria**:
- All 5 projects build successfully on net10.0
- No IdentityModel API errors in AuthService.Infrastructure
- JWT token generation and validation work correctly
- Infrastructure integration tests pass
- Database access functions correctly
- No regression in downstream projects still on net9.0

**Estimated Effort**: High (IdentityModel migration complex)

**Sub-Phases**:
- 3A: Non-Auth Infrastructure layers (3 projects) + EmployeeService.Application.Tests (1 project) - batch
- 3B: AuthService.Infrastructure (1 project) - separate, critical IdentityModel migration

---

#### Phase 4: API Services Layer (Tier 4)
**Projects**: 5 (4 API services, AuthService.Tests)

**Scope**:
- Update TargetFramework to net10.0 (5 projects)
- Update NuGet packages (all API projects: 4 packages each, AuthService.Tests: 5 packages)
- Replace deprecated packages in API projects (AppHost.Hosting packages)
- Address AuthService.Tests IdentityModel issues (16 binary incompatible + 4 source incompatible)
- Configure ASP.NET Core pipeline for net10.0
- Run service-level integration tests

**Success Criteria**:
- All 5 projects build successfully on net10.0
- No deprecated package warnings
- API endpoints respond correctly
- AuthService.Tests pass (JWT validation in tests)
- Service-to-service communication works
- No regression in downstream projects still on net9.0

**Estimated Effort**: Medium (package replacements + AuthService.Tests)

**Sub-Phases**:
- 4A: API services (4 projects) - batch, address deprecated packages
- 4B: AuthService.Tests (1 project) - separate, IdentityModel migration in tests

---

#### Phase 5: Application Hosts (Tier 5)
**Projects**: 2 (AppHost, EmployeeService.Integration.Tests)

**Scope**:
- Update TargetFramework to net10.0 (2 projects)
- Update NuGet packages (AppHost: 4 packages, EmployeeService.Integration.Tests: 3 packages)
- Replace deprecated packages in AppHost
- Address EmployeeService.Integration.Tests issues (13 IdentityModel + 35 HttpContent)
- Configure .NET Aspire orchestration
- Run end-to-end integration tests

**Success Criteria**:
- Both projects build successfully on net10.0
- AppHost orchestrates all services correctly
- EmployeeService.Integration.Tests pass (full stack validation)
- End-to-end user scenarios work
- No runtime errors or behavioral regressions

**Estimated Effort**: Medium (end-to-end validation + IdentityModel in tests)

**Sub-Phases**:
- 5A: AppHost (1 project) - orchestration host
- 5B: EmployeeService.Integration.Tests (1 project) - separate, full stack validation

---

### Tier Completion Criteria

**Before proceeding from Tier N to Tier N+1**:

1. ? All projects in Tier N build successfully without errors
2. ? All projects in Tier N build without warnings (or warnings documented and approved)
3. ? All unit tests for Tier N projects pass
4. ? All integration tests for Tier N projects pass (if applicable)
5. ? Manual validation completed for high-risk projects
6. ? No regressions detected in lower tiers (Tier 0 through N-1)
7. ? Higher tiers still on net9.0 continue to function correctly (if they depend on Tier N)
8. ? Code review completed for Tier N changes
9. ? Documentation updated (if breaking changes affected public APIs)

**Validation Checkpoints**:
- **Post-Tier 1**: Domain.Tests (Tier 2) should pass against upgraded Tier 1
- **Post-Tier 2**: Infrastructure projects (Tier 3) should compile against upgraded Tier 2
- **Post-Tier 3**: API projects (Tier 4) should compile against upgraded Tier 3
- **Post-Tier 4**: AppHost (Tier 5) should compile against upgraded Tier 4
- **Post-Tier 5**: Full end-to-end smoke tests

---

### Between-Tier Validation

**After Each Tier Completion**:

1. **Build Validation**:
   - All projects in completed tier: `dotnet build` succeeds
   - All projects in lower tiers: `dotnet build` still succeeds
   - Sample projects in higher tiers: `dotnet build` still succeeds (they reference upgraded tier)

2. **Test Validation**:
   - Unit tests in completed tier: `dotnet test` passes
   - Integration tests in completed tier: `dotnet test` passes
   - Unit tests in lower tiers: `dotnet test` still passes

3. **Runtime Validation** (for API/Web projects):
   - Tier 2 (BlazorWeb): Launch and navigate UI
   - Tier 4 (API services): Send HTTP requests to endpoints
   - Tier 5 (AppHost): Full orchestration smoke test

4. **Regression Detection**:
   - No new compiler warnings introduced
   - No test failures in lower tiers
   - No API contract changes (unless documented)

**Stop Conditions**:
- ? Any tier fails validation Å® Do not proceed to next tier
- ? High-risk project issues unresolved Å® Do not proceed
- ? Test failures not understood Å® Investigate before proceeding

---

### Strategy-Specific Considerations

**Multi-Project Tiers** (all tiers have multiple projects):

- **Can projects be upgraded in parallel?** 
  - Yes, within same tier (no inter-dependencies within tier)
  - Exception: High-risk projects may need sequential handling for validation

- **Order within tier?**
  - Generally no strict order required within tier
  - Recommendation: Batch simple projects, handle high-risk separately

- **Testing together or separately?**
  - Together for tier validation (all projects in tier must pass before proceeding)
  - High-risk projects may need separate validation pass before batch validation

**Task Granularity for Execution**:
- Each tier = 1 preparation task + 1 update task + 1 testing task + 1 stabilization task
- High-risk projects = separate tasks within tier
- Total expected tasks: ~20-25 tasks across 5 tiers

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution exhibits a clear 5-tier hierarchy with no circular dependencies, making it ideal for Bottom-Up migration strategy.

```
Level 4: [AppHost] [EmployeeService.Integration.Tests]
           Å´           Å´
Level 3: [4 API Services] [AuthService.Tests] [EmployeeService.API]
           Å´
Level 2: [4 Infrastructure Layers] [EmployeeService.Application.Tests]
           Å´
Level 1: [4 Application Layers] [BlazorWeb] [2 Domain.Tests]
           Å´
Level 0: [4 Domain Layers] [ServiceDefaults] [Shared.Contracts]
```

### Tier-by-Tier Breakdown

#### Tier 1 (Level 0): Foundation Libraries - No Internal Dependencies

**Projects (6)**:
- `AttendanceService.Domain.csproj` (1 mandatory issue)
- `AuthService.Domain.csproj` (1 mandatory issue, 1 NuGet update)
- `EmployeeService.Domain.csproj` (1 mandatory issue)
- `NotificationService.Domain.csproj` (1 mandatory issue)
- `ServiceDefaults.csproj` (1 mandatory issue, 4 NuGet updates)
- `Shared.Contracts.csproj` (1 mandatory issue)

**Characteristics**:
- Pure domain models and shared utilities
- Zero project dependencies
- Minimal external package dependencies
- Lowest risk tier

**Why This Tier**:
All projects have no internal project references. They are leaf nodes in the dependency graph.

**Dependants**: Used by 17 downstream projects across all upper tiers.

---

#### Tier 2 (Level 1): Application Logic & Web Frontend

**Projects (7)**:
- `AttendanceService.Application.csproj` (1 mandatory)
- `AuthService.Application.csproj` (1 mandatory)
- `EmployeeService.Application.csproj` (1 mandatory)
- `NotificationService.Application.csproj` (1 mandatory, 1 NuGet update)
- `BlazorWeb.csproj` (1 mandatory, 53 API behavioral changes) ?? **HIGH RISK**
- `AttendanceService.Domain.Tests.csproj` (1 mandatory)
- `EmployeeService.Domain.Tests.csproj` (1 mandatory)

**Dependencies**: Only on Tier 1 projects (Domain layers, ServiceDefaults, Shared.Contracts)

**Characteristics**:
- Application business logic and use cases
- Frontend web application (BlazorWeb)
- Unit tests for domain layers
- BlazorWeb has significant API behavioral changes (53 issues)

**Why This Tier**:
All projects depend exclusively on Tier 1 projects. No dependencies on Tier 2+ projects.

**High-Risk Project**: `BlazorWeb.csproj`
- 53 API issues (52 behavioral changes, 1 source incompatible)
- HttpContent behavioral changes across multiple components
- Requires extensive runtime testing

**Dependants**: Used by 11 downstream projects in Tier 3-4.

---

#### Tier 3 (Level 2): Infrastructure & Data Access

**Projects (5)**:
- `AttendanceService.Infrastructure.csproj` (1 mandatory, 2 NuGet updates)
- `AuthService.Infrastructure.csproj` (16 mandatory, 3 NuGet updates) ?? **HIGH RISK**
- `EmployeeService.Infrastructure.csproj` (1 mandatory, 2 NuGet updates)
- `NotificationService.Infrastructure.csproj` (1 mandatory, 3 NuGet updates)
- `EmployeeService.Application.Tests.csproj` (1 mandatory)

**Dependencies**: Tier 1 (Domain, Shared.Contracts) + Tier 2 (Application layers)

**Characteristics**:
- Database access and external integrations
- Entity Framework Core implementations
- Repository pattern implementations
- AuthService.Infrastructure has critical IdentityModel breaking changes

**Why This Tier**:
Projects depend on both Tier 1 (Domain) and Tier 2 (Application). Cannot migrate until Tier 2 complete.

**High-Risk Project**: `AuthService.Infrastructure.csproj`
- 15 binary incompatible issues (IdentityModel APIs)
- 2 source incompatible issues
- JWT token generation and validation affected
- Migration path: Microsoft.IdentityModel.* packages

**Dependants**: Used by 7 downstream projects in Tier 4-5.

---

#### Tier 4 (Level 3): API Services & Integration Tests

**Projects (5)**:
- `AttendanceService.API.csproj` (1 mandatory, 4 NuGet updates including deprecated packages)
- `AuthService.API.csproj` (1 mandatory, 4 NuGet updates)
- `EmployeeService.API.csproj` (1 mandatory, 4 NuGet updates including deprecated packages)
- `NotificationService.API.csproj` (1 mandatory, 4 NuGet updates including deprecated packages)
- `AuthService.Tests.csproj` (17 mandatory, 5 NuGet updates) ?? **HIGH RISK**

**Dependencies**: All lower tiers (Domain, Application, Infrastructure, ServiceDefaults, Shared.Contracts)

**Characteristics**:
- ASP.NET Core Web APIs with endpoints
- Service-level integration tests
- HTTP pipeline configuration
- Some deprecated packages requiring replacement

**Why This Tier**:
API projects depend on Infrastructure (Tier 3), Application (Tier 2), and Domain (Tier 1) layers. Tests depend on Infrastructure and Application.

**High-Risk Project**: `AuthService.Tests.csproj`
- 16 binary incompatible issues (IdentityModel APIs)
- 4 source incompatible issues
- JWT token validation in tests
- Depends on AuthService.Infrastructure (already high-risk)

**Dependants**: Used by 2 top-level projects (AppHost, EmployeeService.Integration.Tests).

---

#### Tier 5 (Level 4): Application Hosts & End-to-End Tests

**Projects (2)**:
- `AppHost.csproj` (1 mandatory, 4 NuGet updates including deprecated packages)
- `EmployeeService.Integration.Tests.csproj` (14 mandatory, 3 NuGet updates) ?? **HIGH RISK**

**Dependencies**: All projects in solution (transitively through API services)

**Characteristics**:
- .NET Aspire AppHost for orchestration
- End-to-end integration tests
- Top-level entry points

**Why This Tier**:
These projects depend on API services (Tier 4) which transitively depend on all lower tiers.

**High-Risk Project**: `EmployeeService.Integration.Tests.csproj`
- 13 binary incompatible issues (IdentityModel APIs)
- 35 behavioral changes (HttpContent)
- Tests entire EmployeeService stack
- Requires full stack stability before migration

**Dependants**: None (top-level applications).

---

### Migration Phase Groupings (Bottom-Up Strategy)

**Phase 1: Foundation Layer**
- Tier 1 (6 projects)
- Batch all Domain projects + ServiceDefaults + Shared.Contracts together
- Single operation for target framework update
- Single operation for package updates
- Expected duration: Low complexity

**Phase 2: Application & Frontend Layer**
- Tier 2 (7 projects)
- Batch Application layers together (4 projects)
- Handle BlazorWeb separately (high-risk, 53 API issues)
- Handle Domain.Tests separately (2 projects)
- Expected duration: Medium complexity (BlazorWeb requires extra validation)

**Phase 3: Infrastructure Layer**
- Tier 3 (5 projects)
- Handle AuthService.Infrastructure separately (high-risk, 17 IdentityModel issues)
- Batch remaining Infrastructure projects (3 projects)
- Handle EmployeeService.Application.Tests separately (1 project)
- Expected duration: High complexity (IdentityModel migration)

**Phase 4: API Services Layer**
- Tier 4 (5 projects)
- Handle AuthService.Tests separately (high-risk, 20 IdentityModel issues)
- Batch remaining API projects (4 projects) - address deprecated packages
- Expected duration: Medium complexity (package replacements)

**Phase 5: Application Hosts**
- Tier 5 (2 projects)
- Handle EmployeeService.Integration.Tests separately (high-risk, 13 IdentityModel + 35 HttpContent issues)
- Handle AppHost separately (orchestration host)
- Expected duration: Medium complexity (end-to-end validation)

---

### Critical Path Identification

**Critical Path Projects** (must succeed for downstream to proceed):

1. **Tier 1**: `Shared.Contracts` Å® Used by 10 projects
2. **Tier 1**: `ServiceDefaults` Å® Used by 5 API projects + BlazorWeb
3. **Tier 2**: Application layers Å® Used by all Infrastructure projects
4. **Tier 3**: `AuthService.Infrastructure` Å® Highest risk, blocks AuthService.API
5. **Tier 4**: API Services Å® Required by AppHost

**Blocking Issues**:
- IdentityModel migration in AuthService.Infrastructure must complete before AuthService.Tests can proceed
- BlazorWeb HttpContent behavioral changes must be validated before AppHost migration
- All API services must be stable before AppHost orchestration works

---

### Circular Dependencies

**Status**: ? No circular dependencies detected

All projects follow a clean top-down dependency flow. The Bottom-Up strategy can proceed without any refactoring.

---

## Project-by-Project Plans

### Tier 1 (Level 0): Foundation Libraries

#### AttendanceService.Domain.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: AttendanceService.Application, AttendanceService.Infrastructure, AttendanceService.Domain.Tests
- LOC: 625
- Files: 9

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### AuthService.Domain.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: AuthService.Application, AuthService.Infrastructure
- LOC: 19
- Files: 1

**Target State**:
- Target Framework: net10.0
- Package Updates: 1 package upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Domain.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: EmployeeService.Application, EmployeeService.Infrastructure, EmployeeService.Domain.Tests
- LOC: 282
- Files: 4

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### NotificationService.Domain.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: NotificationService.Application, NotificationService.Infrastructure
- LOC: 193
- Files: 2

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### ServiceDefaults.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: AttendanceService.API, AuthService.API, EmployeeService.API, NotificationService.API, BlazorWeb
- LOC: 127
- Files: 1

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### Shared.Contracts.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: 0 project references
- Dependants: 10 projects (Application layers, API projects, Web app, Tests)
- LOC: 732
- Files: 25

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

### Tier 2 (Level 1): Application Logic & Frontend

#### AttendanceService.Application.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: AttendanceService.Domain, Shared.Contracts
- Dependants: AttendanceService.API, AttendanceService.Infrastructure
- LOC: 454
- Files: 5

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### AuthService.Application.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: AuthService.Domain, Shared.Contracts
- Dependants: AuthService.API, AuthService.Infrastructure, AuthService.Tests
- LOC: 40
- Files: 2

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Application.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: Shared.Contracts, EmployeeService.Domain
- Dependants: EmployeeService.API, EmployeeService.Infrastructure, EmployeeService.Application.Tests, EmployeeService.Integration.Tests
- LOC: 460
- Files: 7

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### NotificationService.Application.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: Shared.Contracts, NotificationService.Domain
- Dependants: NotificationService.API, NotificationService.Infrastructure
- LOC: 140
- Files: 4

**Target State**:
- Target Framework: net10.0
- Package Updates: 1 package upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### BlazorWeb.csproj ?? HIGH RISK

**Current State**:
- Target Framework: net9.0
- Project Type: AspNetCore (Blazor Web App)
- Dependencies: ServiceDefaults, Shared.Contracts
- Dependants: AppHost
- LOC: 1,488
- Files: 35
- **API Issues**: 53 (52 behavioral changes, 1 source incompatible)

**Target State**:
- Target Framework: net10.0
- Package Updates: None required
- **Breaking Changes**: HttpContent behavioral changes, Uri behavioral changes, Task.WhenAll source incompatible

**Migration Steps**:
[Details to be filled]

---

#### AttendanceService.Domain.Tests.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test project)
- Dependencies: AttendanceService.Domain
- Dependants: None (top-level test project)
- LOC: 741
- Files: 5

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Domain.Tests.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test project)
- Dependencies: EmployeeService.Domain
- Dependants: None (top-level test project)
- LOC: 241
- Files: 4

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

### Tier 3 (Level 2): Infrastructure & Data Access

#### AttendanceService.Infrastructure.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: AttendanceService.Domain, AttendanceService.Application
- Dependants: AttendanceService.API
- LOC: 930
- Files: 10

**Target State**:
- Target Framework: net10.0
- Package Updates: 2 packages upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### AuthService.Infrastructure.csproj ?? HIGH RISK

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: AuthService.Application, AuthService.Domain
- Dependants: AuthService.API, AuthService.Tests
- LOC: 1,171
- Files: 8
- **API Issues**: 17 (15 binary incompatible, 2 source incompatible)
- **Technology**: IdentityModel & Claims-based Security (JWT tokens)

**Target State**:
- Target Framework: net10.0
- Package Updates: 3 packages upgrade recommended
- **Breaking Changes**: System.IdentityModel.Tokens.Jwt APIs, JwtSecurityTokenHandler, JwtRegisteredClaimNames, IdentityEntityFrameworkBuilderExtensions

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Infrastructure.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: EmployeeService.Application, EmployeeService.Domain
- Dependants: EmployeeService.API, EmployeeService.Integration.Tests
- LOC: 1,008
- Files: 14

**Target State**:
- Target Framework: net10.0
- Package Updates: 2 packages upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### NotificationService.Infrastructure.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: ClassLibrary
- Dependencies: Shared.Contracts, NotificationService.Domain, NotificationService.Application
- Dependants: NotificationService.API
- LOC: 751
- Files: 8

**Target State**:
- Target Framework: net10.0
- Package Updates: 3 packages upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Application.Tests.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test project)
- Dependencies: EmployeeService.Application, Shared.Contracts
- Dependants: None (top-level test project)
- LOC: 472
- Files: 4

**Target State**:
- Target Framework: net10.0
- Package Updates: None required

**Migration Steps**:
[Details to be filled]

---

### Tier 4 (Level 3): API Services & Integration Tests

#### AttendanceService.API.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: AspNetCore (Web API)
- Dependencies: AttendanceService.Infrastructure, AttendanceService.Application, ServiceDefaults
- Dependants: AppHost
- LOC: 1,119
- Files: 3
- **API Issues**: 6 behavioral changes

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages (some deprecated packages to replace)
- **Breaking Changes**: HttpContent behavioral changes, ExceptionHandler behavioral changes

**Migration Steps**:
[Details to be filled]

---

#### AuthService.API.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: AspNetCore (Web API)
- Dependencies: AuthService.Application, AuthService.Infrastructure, Shared.Contracts, ServiceDefaults
- Dependants: AppHost
- LOC: 70
- Files: 3

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages upgrade recommended

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.API.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: AspNetCore (Web API)
- Dependencies: EmployeeService.Application, EmployeeService.Infrastructure, ServiceDefaults
- Dependants: AppHost, EmployeeService.Integration.Tests
- LOC: 346
- Files: 4
- **API Issues**: 5 source incompatible

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages (some deprecated packages to replace)
- **Breaking Changes**: IdentityEntityFrameworkBuilderExtensions, JwtBearer APIs

**Migration Steps**:
[Details to be filled]

---

#### NotificationService.API.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: AspNetCore (Web API)
- Dependencies: Shared.Contracts, NotificationService.Infrastructure, ServiceDefaults, NotificationService.Application
- Dependants: AppHost
- LOC: 152
- Files: 4

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages (some deprecated packages to replace)

**Migration Steps**:
[Details to be filled]

---

#### AuthService.Tests.csproj ?? HIGH RISK

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Test project)
- Dependencies: AuthService.Application, AuthService.Infrastructure, Shared.Contracts
- Dependants: None (top-level test project)
- LOC: 320
- Files: 3
- **API Issues**: 20 (16 binary incompatible, 4 source incompatible)
- **Technology**: IdentityModel & Claims-based Security (JWT token validation in tests)

**Target State**:
- Target Framework: net10.0
- Package Updates: 5 packages upgrade recommended
- **Breaking Changes**: System.IdentityModel.Tokens.Jwt APIs, JwtSecurityTokenHandler, JwtRegisteredClaimNames

**Migration Steps**:
[Details to be filled]

---

### Tier 5 (Level 4): Application Hosts & End-to-End Tests

#### AppHost.csproj

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (.NET Aspire App Host)
- Dependencies: NotificationService.API, BlazorWeb, AuthService.API, EmployeeService.API, AttendanceService.API
- Dependants: None (top-level application)
- LOC: 42
- Files: 1

**Target State**:
- Target Framework: net10.0
- Package Updates: 4 packages (some deprecated packages to replace)

**Migration Steps**:
[Details to be filled]

---

#### EmployeeService.Integration.Tests.csproj ?? HIGH RISK

**Current State**:
- Target Framework: net9.0
- Project Type: DotNetCoreApp (Integration test project)
- Dependencies: EmployeeService.Application, EmployeeService.API, EmployeeService.Infrastructure
- Dependants: None (top-level test project)
- LOC: 1,670
- Files: 8
- **API Issues**: 48 (13 binary incompatible, 35 behavioral changes)
- **Technology**: IdentityModel (13 issues) + HttpContent behavioral changes (35 issues)

**Target State**:
- Target Framework: net10.0
- Package Updates: 3 packages upgrade recommended
- **Breaking Changes**: System.IdentityModel.Tokens.Jwt APIs, HttpContent behavioral changes

**Migration Steps**:
[Details to be filled]

---

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| AuthService.Infrastructure | ?? **High** | 15 binary incompatible + 2 source incompatible IdentityModel APIs. JWT token generation/validation core functionality. | Migrate to Microsoft.IdentityModel.* packages first. Create comprehensive unit tests for token operations. Validate against existing tokens. Manual testing of authentication flows. |
| BlazorWeb | ?? **Medium-High** | 52 behavioral changes in HttpContent, 1 source incompatible. Large UI codebase (1,488 LOC, 35 files). | Extensive manual UI testing. Focus on HTTP client calls. Validate all user scenarios. Browser compatibility testing. |
| EmployeeService.Integration.Tests | ?? **Medium-High** | 13 binary incompatible (IdentityModel) + 35 behavioral changes (HttpContent). Full-stack integration tests. | Update after all lower tiers stable. Validate JWT handling in tests. Re-record/update test expectations. |
| AuthService.Tests | ?? **Medium** | 16 binary incompatible + 4 source incompatible IdentityModel APIs in tests. | Migrate after AuthService.Infrastructure complete. Reuse migration patterns from Infrastructure. |
| AttendanceService.API | ?? **Low-Medium** | 6 behavioral changes (HttpContent, ExceptionHandler). Deprecated packages. | Standard testing. Replace deprecated packages with recommended alternatives. |
| EmployeeService.API | ?? **Low-Medium** | 5 source incompatible (IdentityModel in DI). Deprecated packages. | Update service registration. Replace deprecated packages. Test authentication. |

### Security Vulnerabilities

**Status**: ? No security vulnerabilities detected in current package versions.

**Assessment Notes**:
- Current packages already on secure versions
- .NET 10 upgrade does not introduce new vulnerabilities
- Continue monitoring for security advisories post-upgrade

### Contingency Plans

#### 1. IdentityModel Migration Fails (AuthService.Infrastructure)

**Symptoms**:
- JWT tokens cannot be generated
- Token validation fails
- Authentication breaks

**Alternatives**:
- **Option A**: Stay on compatible IdentityModel package versions (if available for net10.0)
- **Option B**: Rollback AuthService.Infrastructure to net9.0, defer migration to later phase
- **Option C**: Implement custom JWT handling using System.IdentityModel.Tokens.Jwt alternatives

**Decision Criteria**:
- If Option A available: Use it temporarily, plan full migration later
- If authentication completely broken: Rollback (Option B)
- If partial functionality: Continue with Option C (high effort)

**Recommended**: Create AuthService.Infrastructure branch before migration, enable easy rollback

---

#### 2. BlazorWeb Behavioral Changes Cause UI Failures

**Symptoms**:
- HTTP calls fail with unexpected errors
- UI components don't render correctly
- Navigation broken

**Alternatives**:
- **Option A**: Rollback BlazorWeb to net9.0, keep on older framework temporarily
- **Option B**: Implement workarounds for HttpContent behavioral changes
- **Option C**: Refactor affected components to avoid problematic APIs

**Decision Criteria**:
- If < 10 components affected: Option B (workarounds)
- If > 10 components affected: Option A (rollback) or Option C (refactor)
- If blocking AppHost: Rollback immediately

**Recommended**: Test BlazorWeb in isolation before AppHost integration

---

#### 3. Integration Tests Fail After Migration

**Symptoms**:
- EmployeeService.Integration.Tests failures
- Cross-service authentication broken
- End-to-end scenarios fail

**Alternatives**:
- **Option A**: Update test expectations to match new .NET 10 behavior
- **Option B**: Rollback failing services to net9.0
- **Option C**: Skip integration tests temporarily, rely on manual testing

**Decision Criteria**:
- If tests fail due to behavioral changes: Option A (update expectations)
- If services genuinely broken: Option B (rollback)
- If time-critical: Option C (temporary skip) - not recommended

**Recommended**: Run integration tests after each tier completion, isolate failures early

---

#### 4. Deprecated Packages Have No Direct Replacement

**Symptoms**:
- Package not available for net10.0
- No clear migration path
- Critical functionality lost

**Alternatives**:
- **Option A**: Find community alternative packages
- **Option B**: Implement functionality directly (remove package dependency)
- **Option C**: Stay on older framework version for affected project

**Decision Criteria**:
- If non-critical functionality: Option B (implement directly)
- If critical with alternative: Option A (switch packages)
- If critical without alternative: Option C (defer migration)

**Recommended**: Identify deprecated packages early (Phase 1), research alternatives before migration

---

#### 5. Performance Regression After Upgrade

**Symptoms**:
- API response times increased
- Memory usage higher
- Throughput decreased

**Alternatives**:
- **Option A**: Profile and optimize hot paths
- **Option B**: Adjust runtime configuration (GC settings, thread pool)
- **Option C**: Rollback if regression unacceptable

**Decision Criteria**:
- If < 10% regression: Option A or B (optimize)
- If 10-20% regression: Investigate root cause, Option A
- If > 20% regression: Option C (rollback), report issue to .NET team

**Recommended**: Baseline performance metrics before upgrade, compare after each tier

---

### Rollback Strategy

#### Per-Tier Rollback

**When to Rollback**:
- ? Tier validation fails (build errors, test failures)
- ? Critical functionality broken
- ? High-risk project issues unresolved after 2 attempts
- ? Performance regression > 20%
- ? Security vulnerabilities introduced

**Rollback Process**:
1. Identify last successful tier (N-1)
2. Revert project files (.csproj) for Tier N to net9.0
3. Revert package versions for Tier N
4. Restore code changes for Tier N from Git
5. Rebuild solution
6. Run tests for Tiers 0 through N-1 (ensure stability)
7. Document rollback reason and failed tier

**Git Strategy**:
- Each tier completion = separate commit
- Tag commits: `tier-1-complete`, `tier-2-complete`, etc.
- Easy rollback: `git reset --hard tier-N-1-complete`

**Communication**:
- Notify team of rollback immediately
- Document blockers in issue tracker
- Plan remediation approach before retry

---

#### Full Solution Rollback

**When to Rollback**:
- ? Multiple tiers fail validation
- ? Fundamental incompatibility discovered
- ? Business-critical deadline at risk

**Rollback Process**:
1. Checkout `dotnet9` branch (source branch)
2. Abandon `upgrade-to-NET10` branch temporarily
3. Continue operations on net9.0
4. Plan comprehensive retry with learnings

**Decision Authority**:
- Tech lead approval required for full rollback
- Document decision and reasoning
- Schedule retry with additional preparation

---

### Risk Mitigation by Tier

#### Tier 1 (Foundation): Low Risk
- **Mitigation**: Batch all 6 projects, single operation
- **Validation**: Build + Domain.Tests pass
- **Rollback**: Simple (no downstream affected yet)

#### Tier 2 (Application & Frontend): Medium Risk
- **Mitigation**: Separate BlazorWeb from batch (high-risk)
- **Validation**: Build + Application unit tests + BlazorWeb manual testing
- **Rollback**: Moderate (some downstream affected)

#### Tier 3 (Infrastructure): High Risk
- **Mitigation**: Separate AuthService.Infrastructure (IdentityModel migration)
- **Additional**: Create unit tests for JWT operations before migration
- **Validation**: Build + Infrastructure integration tests + manual authentication testing
- **Rollback**: Complex (many downstream affected)

#### Tier 4 (API Services): Medium Risk
- **Mitigation**: Separate AuthService.Tests (IdentityModel migration in tests)
- **Additional**: Research deprecated package replacements before migration
- **Validation**: Build + service-level integration tests + API endpoint testing
- **Rollback**: Very complex (AppHost depends on all APIs)

#### Tier 5 (App Hosts): Medium Risk
- **Mitigation**: Separate EmployeeService.Integration.Tests (full-stack validation)
- **Additional**: Full end-to-end smoke tests before declaring complete
- **Validation**: Build + integration tests + AppHost orchestration + end-to-end scenarios
- **Rollback**: Minimal impact (top-level only)

---

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

#### Level 1: Per-Project Testing (During Each Project Migration)

**Build Validation**:
```bash
dotnet build <ProjectPath> --configuration Release
```
- ? No compilation errors
- ? No compilation warnings (or documented exceptions)
- ? Package restore succeeds
- ? Target framework correctly set to net10.0

**Static Analysis** (if applicable):
- Run code analyzers
- Check for API usage warnings
- Verify no deprecated API usage

---

#### Level 2: Tier Testing (After Each Tier Completion)

**All Projects in Tier**:
```bash
# Build all projects in tier
dotnet build --configuration Release

# Run all unit tests in tier
dotnet test --configuration Release --no-build
```

**Tier-Specific Validation**:

**Tier 1 (Foundation)**:
- ? All 6 projects build successfully
- ? No dependency conflicts
- ? Domain models serialize/deserialize correctly
- ? ServiceDefaults extensions work
- ? Shared.Contracts DTOs valid

**Tier 2 (Application & Frontend)**:
- ? All 7 projects build successfully
- ? Application unit tests pass (AttendanceService.Domain.Tests, EmployeeService.Domain.Tests)
- ? BlazorWeb manual testing:
  - Launch application
  - Navigate all major pages
  - Test HTTP client calls (HttpContent behavioral changes)
  - Verify authentication flow
  - Check browser console for errors

**Tier 3 (Infrastructure)**:
- ? All 5 projects build successfully
- ? EmployeeService.Application.Tests pass
- ? AuthService.Infrastructure manual testing:
  - Generate JWT tokens
  - Validate token structure
  - Verify claims extraction
  - Test token expiration
- ? Database access validation:
  - EF Core migrations apply
  - CRUD operations work
  - Connection pooling functional

**Tier 4 (API Services)**:
- ? All 5 projects build successfully
- ? AuthService.Tests pass (JWT validation in tests)
- ? API endpoint testing:
  - Swagger UI loads
  - Send sample requests to each endpoint
  - Verify responses match contracts
  - Check authentication/authorization
  - Validate error handling

**Tier 5 (App Hosts)**:
- ? Both projects build successfully
- ? EmployeeService.Integration.Tests pass
- ? AppHost orchestration:
  - All services start correctly
  - Service-to-service communication works
  - Health checks pass
  - Dashboard accessible

---

#### Level 3: Integration Testing (Between Tiers)

**After Each Tier, Validate Downstream Consumers**:

**Post-Tier 1**:
- Tier 2 projects (on net9.0) can still reference Tier 1 projects (on net10.0)
- Build sample Tier 2 project to verify compatibility
- Run Tier 2 tests (AttendanceService.Domain.Tests, EmployeeService.Domain.Tests) against upgraded Tier 1

**Post-Tier 2**:
- Tier 3 projects (on net9.0) can still reference Tier 2 projects (on net10.0)
- Build sample Tier 3 project to verify compatibility

**Post-Tier 3**:
- Tier 4 projects (on net9.0) can still reference Tier 3 projects (on net10.0)
- Build sample Tier 4 project to verify compatibility

**Post-Tier 4**:
- Tier 5 projects (on net9.0) can still reference Tier 4 projects (on net10.0)
- Build AppHost to verify all API references work

**Post-Tier 5**:
- Full solution builds successfully
- All tests pass

---

#### Level 4: Full Solution Testing (After All Tiers Complete)

**Complete Build**:
```bash
cd D:\Repos\runceel\DotnetEmployeeManagementSystem
dotnet build DotnetEmployeeManagementSystem.slnx --configuration Release
```
- ? All 25 projects build without errors
- ? No warnings (or documented exceptions)

**Complete Test Suite**:
```bash
dotnet test DotnetEmployeeManagementSystem.slnx --configuration Release
```
- ? All test projects pass:
  - AttendanceService.Domain.Tests
  - AuthService.Tests
  - EmployeeService.Application.Tests
  - EmployeeService.Domain.Tests
  - EmployeeService.Integration.Tests

**End-to-End Scenarios**:
1. **Employee Management Flow**:
   - Authenticate user (AuthService)
   - Create new employee (EmployeeService)
   - View employee list (EmployeeService)
   - Update employee details (EmployeeService)
   - Delete employee (EmployeeService)

2. **Attendance Tracking Flow**:
   - Authenticate user (AuthService)
   - Clock in (AttendanceService)
   - Clock out (AttendanceService)
   - View attendance history (AttendanceService)

3. **Notification Flow**:
   - Trigger notification (NotificationService)
   - Verify delivery (NotificationService)

4. **Cross-Service Flow**:
   - Create employee Å® Send welcome notification
   - Clock in Å® Send attendance notification

**Performance Baseline**:
- Measure API response times (compare to net9.0 baseline)
- Check memory usage (compare to baseline)
- Validate throughput (compare to baseline)

---

### Testing Checklist per Project

#### For All Projects:
- [ ] Project file (.csproj) updated to `<TargetFramework>net10.0</TargetFramework>`
- [ ] `dotnet build` succeeds without errors
- [ ] `dotnet build` produces no warnings (or warnings documented)
- [ ] Package restore succeeds (`dotnet restore`)
- [ ] No dependency conflicts reported

#### For Projects with Package Updates:
- [ ] All packages updated to target versions
- [ ] No deprecated package warnings
- [ ] Package compatibility verified (no version conflicts)

#### For Projects with Breaking Changes:
- [ ] Breaking changes identified and listed
- [ ] Code modifications applied
- [ ] Compilation succeeds after modifications
- [ ] Functional testing passed

#### For Library Projects (Domain, Application, Infrastructure):
- [ ] Public API surface unchanged (unless documented)
- [ ] Unit tests pass
- [ ] Integration tests pass (if applicable)

#### For API Projects:
- [ ] Application starts without errors
- [ ] Swagger/OpenAPI documentation loads
- [ ] Health check endpoint responds
- [ ] Sample requests to endpoints succeed
- [ ] Authentication/authorization works
- [ ] Error handling functional

#### For Web Projects (BlazorWeb):
- [ ] Application starts without errors
- [ ] Home page loads
- [ ] Navigation works across all pages
- [ ] HTTP client calls succeed
- [ ] Authentication flow works
- [ ] No console errors in browser

#### For Test Projects:
- [ ] All tests compile
- [ ] `dotnet test` succeeds
- [ ] No skipped tests (unless documented)
- [ ] Test coverage maintained (if tracked)

#### For AppHost:
- [ ] All services referenced
- [ ] Orchestration configuration valid
- [ ] All services start successfully
- [ ] Service discovery works
- [ ] Health checks pass

---

### Smoke Tests (Quick Validation)

**Purpose**: Rapid validation after each tier completion (5-10 minutes)

**Tier 1 Smoke Test**:
```bash
# Build all Tier 1 projects
dotnet build src/Services/AttendanceService/Domain/AttendanceService.Domain.csproj
dotnet build src/Services/AuthService/Domain/AuthService.Domain.csproj
dotnet build src/Services/EmployeeService/Domain/EmployeeService.Domain.csproj
dotnet build src/Services/NotificationService/Domain/NotificationService.Domain.csproj
dotnet build src/ServiceDefaults/ServiceDefaults.csproj
dotnet build src/Shared/Contracts/Shared.Contracts.csproj

# Run Domain.Tests (from Tier 2, but test Tier 1)
dotnet test tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj
dotnet test tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj
```

**Tier 2 Smoke Test**:
```bash
# Build all Tier 2 projects
dotnet build src/Services/AttendanceService/Application/AttendanceService.Application.csproj
dotnet build src/Services/AuthService/Application/AuthService.Application.csproj
dotnet build src/Services/EmployeeService/Application/EmployeeService.Application.csproj
dotnet build src/Services/NotificationService/Application/NotificationService.Application.csproj
dotnet build src/WebApps/BlazorWeb/BlazorWeb.csproj

# Run Application tests
dotnet test tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj

# Manual: Launch BlazorWeb, navigate to home page
dotnet run --project src/WebApps/BlazorWeb/BlazorWeb.csproj
```

**Tier 3 Smoke Test**:
```bash
# Build all Tier 3 projects
dotnet build src/Services/AttendanceService/Infrastructure/AttendanceService.Infrastructure.csproj
dotnet build src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj
dotnet build src/Services/EmployeeService/Infrastructure/EmployeeService.Infrastructure.csproj
dotnet build src/Services/NotificationService/Infrastructure/NotificationService.Infrastructure.csproj

# Manual: Test JWT token generation (AuthService.Infrastructure)
# Create simple console app or use existing test
```

**Tier 4 Smoke Test**:
```bash
# Build all Tier 4 projects
dotnet build src/Services/AttendanceService/API/AttendanceService.API.csproj
dotnet build src/Services/AuthService/API/AuthService.API.csproj
dotnet build src/Services/EmployeeService/API/EmployeeService.API.csproj
dotnet build src/Services/NotificationService/API/NotificationService.API.csproj

# Run AuthService.Tests
dotnet test tests/AuthService.Tests/AuthService.Tests.csproj

# Manual: Launch one API, send sample request
dotnet run --project src/Services/EmployeeService/API/EmployeeService.API.csproj
# curl http://localhost:5000/health
```

**Tier 5 Smoke Test**:
```bash
# Build all Tier 5 projects
dotnet build src/AppHost/AppHost.csproj

# Run EmployeeService.Integration.Tests
dotnet test tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj

# Manual: Launch AppHost, verify dashboard
dotnet run --project src/AppHost/AppHost.csproj
```

---

### Comprehensive Validation (Before Final Sign-Off)

**Purpose**: Thorough validation before declaring migration complete (1-2 hours)

**Steps**:
1. **Clean Build**:
   ```bash
   dotnet clean
   dotnet build --configuration Release
   ```
   - Verify all 25 projects build

2. **Full Test Suite**:
   ```bash
   dotnet test --configuration Release --logger "console;verbosity=detailed"
   ```
   - Verify all 5 test projects pass
   - Review test output for warnings

3. **Launch All Services**:
   ```bash
   dotnet run --project src/AppHost/AppHost.csproj
   ```
   - Verify all services start
   - Check Aspire dashboard for health status

4. **End-to-End Scenarios** (listed above)

5. **Performance Comparison**:
   - Run performance tests (if available)
   - Compare to net9.0 baseline
   - Acceptable: < 10% regression

6. **Security Scan**:
   ```bash
   dotnet list package --vulnerable
   ```
   - Verify no vulnerabilities introduced

7. **Documentation Review**:
   - Verify breaking changes documented
   - Update README if needed
   - Confirm migration notes complete

---

---

## Complexity & Effort Assessment

### Per-Tier Complexity Ratings

| Tier | Projects | Total LOC | Risk Level | Dependencies | Complexity | Rationale |
|------|----------|-----------|------------|--------------|------------|-----------|
| **Tier 1** | 6 | 1,978 | ?? Low | 0 internal | **Low** | Pure domain models, no project dependencies, minimal external packages. Straightforward framework update. |
| **Tier 2** | 7 | 3,055 | ?? Medium | 1-2 per project | **Medium** | Application logic + BlazorWeb (53 API issues). BlazorWeb requires extensive manual testing. |
| **Tier 3** | 5 | 3,831 | ?? High | 2-3 per project | **High** | AuthService.Infrastructure has 17 IdentityModel issues. JWT migration complex. Database access validation required. |
| **Tier 4** | 5 | 1,707 | ?? Medium | 3-4 per project | **Medium** | Deprecated packages to replace. AuthService.Tests has 20 IdentityModel issues (reuses Tier 3 patterns). API endpoint testing. |
| **Tier 5** | 2 | 1,712 | ?? Medium | 5+ per project | **Medium** | EmployeeService.Integration.Tests has 48 issues. Full-stack validation. AppHost orchestration. |
| **Total** | **25** | **12,283** | - | - | **Medium** | Well-structured for Bottom-Up approach. |

### Per-Project Complexity Details

#### Low Complexity Projects (14 projects)
**Characteristics**: < 1,000 LOC, 0-1 package updates, 0-1 mandatory issues, no API breaking changes

- AttendanceService.Domain (625 LOC)
- AuthService.Domain (19 LOC)
- EmployeeService.Domain (282 LOC)
- NotificationService.Domain (193 LOC)
- ServiceDefaults (127 LOC)
- Shared.Contracts (732 LOC)
- AttendanceService.Application (454 LOC)
- AuthService.Application (40 LOC)
- EmployeeService.Application (460 LOC)
- NotificationService.Application (140 LOC)
- AttendanceService.Domain.Tests (741 LOC)
- EmployeeService.Domain.Tests (241 LOC)
- EmployeeService.Application.Tests (472 LOC)
- AuthService.API (70 LOC)

**Effort**: 1-2 hours per project (framework update + basic testing)

---

#### Medium Complexity Projects (8 projects)
**Characteristics**: 1,000-1,500 LOC, 2-4 package updates, 1-6 mandatory issues, some API changes

- AttendanceService.Infrastructure (930 LOC, 2 packages)
- EmployeeService.Infrastructure (1,008 LOC, 2 packages)
- NotificationService.Infrastructure (751 LOC, 3 packages)
- AttendanceService.API (1,119 LOC, 4 packages, 6 behavioral changes)
- EmployeeService.API (346 LOC, 4 packages, 5 source incompatible)
- NotificationService.API (152 LOC, 4 packages)
- AppHost (42 LOC, 4 packages, orchestration complexity)
- AuthService.Tests (320 LOC, 5 packages, 20 IdentityModel issues - reuses Tier 3 patterns)

**Effort**: 3-6 hours per project (package updates + API changes + testing)

---

#### High Complexity Projects (3 projects)
**Characteristics**: > 1,000 LOC, multiple package updates, > 10 mandatory issues, significant API breaking changes

1. **AuthService.Infrastructure** (1,171 LOC, 3 packages, 17 IdentityModel issues)
   - **Complexity Factors**:
     - 15 binary incompatible JWT APIs
     - 2 source incompatible APIs
     - Core authentication functionality
     - Requires migration to Microsoft.IdentityModel.* packages
   - **Effort**: 12-16 hours (research, migration, testing)

2. **BlazorWeb** (1,488 LOC, 35 files, 53 API issues)
   - **Complexity Factors**:
     - 52 behavioral changes (HttpContent, Uri)
     - 1 source incompatible (Task.WhenAll)
     - Large UI codebase across many files
     - Requires extensive manual testing
   - **Effort**: 10-14 hours (code changes, UI testing)

3. **EmployeeService.Integration.Tests** (1,670 LOC, 8 files, 48 issues)
   - **Complexity Factors**:
     - 13 binary incompatible (IdentityModel)
     - 35 behavioral changes (HttpContent)
     - Full-stack integration testing
     - Depends on all lower tiers being stable
   - **Effort**: 8-12 hours (test updates, full-stack validation)

---

### Tier Effort Breakdown (Bottom-Up Strategy Approach)

#### Tier 1: Foundation Layer
**Complexity**: ?? Low  
**Projects**: 6 (batch together)  
**Estimated Effort**: Low  

**Operations**:
1. **Preparation**: Review tier projects, verify dependencies stable (single task)
2. **Update**: Update all 6 .csproj files to net10.0 + update packages (single batch task)
3. **Testing**: Build all + run Domain.Tests (single task)
4. **Stabilization**: Review results, mark tier complete (single task)

**Task Count**: ~4 tasks

---

#### Tier 2: Application & Frontend Layer
**Complexity**: ?? Medium (BlazorWeb high-risk)  
**Projects**: 7 (4 Application + 2 Domain.Tests batched, BlazorWeb separate)  
**Estimated Effort**: Medium  

**Operations**:
1. **Preparation**: Review tier projects, identify BlazorWeb risks (single task)
2. **Update (Batch)**: Update 6 projects (Application layers + Domain.Tests) - framework + packages (single task)
3. **Update (BlazorWeb)**: Update BlazorWeb separately - framework (single task)
4. **Testing (Batch)**: Build 6 projects + run tests (single task)
5. **Testing (BlazorWeb)**: Build + extensive manual UI testing (single task)
6. **Stabilization**: Review results, mark tier complete (single task)

**Task Count**: ~6 tasks

---

#### Tier 3: Infrastructure Layer
**Complexity**: ?? High (AuthService.Infrastructure critical)  
**Projects**: 5 (3 Infrastructure batched, AuthService.Infrastructure separate, 1 test project separate)  
**Estimated Effort**: High  

**Operations**:
1. **Preparation**: Review tier, create JWT unit tests for AuthService.Infrastructure (single task)
2. **Update (Batch)**: Update 3 Infrastructure projects (Attendance, Employee, Notification) + EmployeeService.Application.Tests - framework + packages (single task)
3. **Update (AuthService.Infrastructure)**: Update framework + migrate IdentityModel APIs (single task)
4. **Testing (Batch)**: Build 4 projects + run tests (single task)
5. **Testing (AuthService.Infrastructure)**: Build + comprehensive JWT validation + manual authentication testing (single task)
6. **Stabilization**: Review results, mark tier complete (single task)

**Task Count**: ~6 tasks

---

#### Tier 4: API Services Layer
**Complexity**: ?? Medium (AuthService.Tests, deprecated packages)  
**Projects**: 5 (4 API services batched, AuthService.Tests separate)  
**Estimated Effort**: Medium  

**Operations**:
1. **Preparation**: Research deprecated package replacements (single task)
2. **Update (API Batch)**: Update 4 API projects - framework + replace deprecated packages (single task)
3. **Update (AuthService.Tests)**: Update framework + migrate IdentityModel APIs in tests (single task)
4. **Testing (API Batch)**: Build 4 APIs + test endpoints (single task)
5. **Testing (AuthService.Tests)**: Build + run tests + verify JWT validation (single task)
6. **Stabilization**: Review results, mark tier complete (single task)

**Task Count**: ~6 tasks

---

#### Tier 5: Application Hosts
**Complexity**: ?? Medium (full-stack validation)  
**Projects**: 2 (both separate)  
**Estimated Effort**: Medium  

**Operations**:
1. **Preparation**: Review tier, prepare end-to-end test scenarios (single task)
2. **Update (AppHost)**: Update framework + replace deprecated packages (single task)
3. **Update (EmployeeService.Integration.Tests)**: Update framework + packages + migrate IdentityModel + HttpContent (single task)
4. **Testing (AppHost)**: Build + orchestration smoke test (single task)
5. **Testing (Integration.Tests)**: Build + run tests + full-stack validation (single task)
6. **Stabilization**: Review results, mark tier complete (single task)

**Task Count**: ~6 tasks

---

### Resource Requirements

#### Skills Needed

1. **.NET/C# Development** (all tiers)
   - Framework migration experience
   - NuGet package management
   - MSBuild/project file editing

2. **ASP.NET Core** (Tiers 2, 4, 5)
   - Web API configuration
   - Blazor development
   - Middleware pipeline

3. **Authentication/Security** (Tier 3, 4, 5)
   - JWT token handling
   - IdentityModel package migration
   - Claims-based authentication

4. **Testing** (all tiers)
   - Unit testing (xUnit/NUnit)
   - Integration testing
   - Manual testing (BlazorWeb)

5. **.NET Aspire** (Tier 5)
   - AppHost configuration
   - Service orchestration

#### Parallel Work Capacity

**Within Tier** (if multiple developers available):
- **Tier 1**: 1 developer (batch all 6 projects)
- **Tier 2**: 2 developers (1 for batch, 1 for BlazorWeb)
- **Tier 3**: 2 developers (1 for batch, 1 for AuthService.Infrastructure)
- **Tier 4**: 2 developers (1 for API batch, 1 for AuthService.Tests)
- **Tier 5**: 2 developers (1 for AppHost, 1 for Integration.Tests)

**Between Tiers**: Strictly sequential (cannot parallelize tiers)

#### Timeline Considerations

**Note**: This plan uses **relative complexity** ratings (Low/Medium/High) rather than time estimates. Actual duration depends on:
- Team experience with .NET upgrades
- Familiarity with IdentityModel migration
- Testing infrastructure availability
- Availability of developers
- Unexpected issues encountered

**Relative Effort Distribution**:
- Tier 1: ~10% of total effort
- Tier 2: ~20% of total effort
- Tier 3: ~30% of total effort (AuthService.Infrastructure)
- Tier 4: ~20% of total effort
- Tier 5: ~20% of total effort

---

---

## Source Control Strategy

### Branching Strategy

**Main Branch**: `dotnet9` (source branch)  
**Upgrade Branch**: `upgrade-to-NET10` (current branch)  
**Target Branch**: `main` or `dotnet9` (after successful upgrade)

#### Branch Structure

```
dotnet9 (source)
  Ñ§Ñü> upgrade-to-NET10 (feature branch)
        Ñ•Ñü> tier-1-foundation (optional sub-branch)
        Ñ•Ñü> tier-2-application (optional sub-branch)
        Ñ•Ñü> tier-3-infrastructure (optional sub-branch)
        Ñ•Ñü> tier-4-api-services (optional sub-branch)
        Ñ§Ñü> tier-5-app-hosts (optional sub-branch)
```

**Recommended Approach**: Work directly on `upgrade-to-NET10` branch, use commits as checkpoints.

**Alternative Approach** (for larger teams): Create sub-branches per tier, merge back to `upgrade-to-NET10` after tier completion.

---

### Commit Strategy

#### Commit Frequency

**Per Tier**: 3-5 commits
1. **Preparation commit**: `[Tier N] Prepare for migration - review dependencies`
2. **Framework update commit**: `[Tier N] Update TargetFramework to net10.0`
3. **Package update commit**: `[Tier N] Update NuGet packages` (if applicable)
4. **Code changes commit**: `[Tier N] Fix breaking changes` (if applicable)
5. **Testing commit**: `[Tier N] Validate tier - all tests passing`

**Commit Granularity**:
- **DO**: Commit after each logical operation (framework update, package updates, code fixes)
- **DO**: Commit after tier validation passes
- **DON'T**: Commit broken/non-compiling code (unless explicitly marked WIP)
- **DON'T**: Mix multiple tiers in single commit

#### Commit Message Format

```
[Tier N: <Tier Name>] <Action>

<Optional detailed description>

- Project 1
- Project 2
...

Validation: <Build/Test status>
```

**Examples**:

```
[Tier 1: Foundation] Update TargetFramework to net10.0

Updated all 6 foundation projects to target net10.0:
- AttendanceService.Domain
- AuthService.Domain
- EmployeeService.Domain
- NotificationService.Domain
- ServiceDefaults
- Shared.Contracts

Validation: Build succeeds, Domain.Tests pass
```

```
[Tier 3: Infrastructure] Migrate AuthService.Infrastructure IdentityModel APIs

Replaced System.IdentityModel.Tokens.Jwt APIs with Microsoft.IdentityModel.* equivalents:
- JwtSecurityTokenHandler Å® JsonWebTokenHandler
- JwtSecurityToken constructor Å® CreateToken methods
- Updated package references

Validation: Build succeeds, JWT generation/validation tests pass
```

---

### Checkpoints and Tags

**After Each Tier Completion**:

Create Git tag for rollback purposes:
```bash
git tag -a tier-1-complete -m "Tier 1 (Foundation) migration complete and validated"
git tag -a tier-2-complete -m "Tier 2 (Application & Frontend) migration complete and validated"
git tag -a tier-3-complete -m "Tier 3 (Infrastructure) migration complete and validated"
git tag -a tier-4-complete -m "Tier 4 (API Services) migration complete and validated"
git tag -a tier-5-complete -m "Tier 5 (App Hosts) migration complete and validated"
```

**Rollback to Checkpoint**:
```bash
git reset --hard tier-N-complete
```

**Push Tags** (for team visibility):
```bash
git push origin --tags
```

---

### Review and Merge Process

#### Per-Tier Review (Optional for smaller teams)

**After Each Tier**:
1. Self-review changes for tier
2. Verify validation checklist complete
3. Document any issues or workarounds
4. Tag tier completion
5. Proceed to next tier

**Team Review** (if required):
- Create draft PR for tier (don't merge yet)
- Request review from team member
- Address feedback
- Close draft PR, continue on branch

---

#### Final Review (Before Merge to dotnet9)

**When**: After Tier 5 complete and all validation passed

**Steps**:
1. **Create Pull Request**: `upgrade-to-NET10` Å® `dotnet9`
   - Title: `.NET 10 Migration - All 25 projects`
   - Description: Link to this plan.md, summary of changes

2. **PR Checklist**:
   - [ ] All 25 projects target net10.0
   - [ ] All package updates applied
   - [ ] All breaking changes addressed
   - [ ] Full solution builds without errors
   - [ ] All tests pass (5 test projects)
   - [ ] End-to-end scenarios validated
   - [ ] No security vulnerabilities introduced
   - [ ] Performance acceptable (< 10% regression)
   - [ ] Documentation updated (README, breaking changes notes)

3. **Code Review**:
   - Focus on high-risk projects (AuthService.Infrastructure, BlazorWeb, EmployeeService.Integration.Tests)
   - Verify IdentityModel migration correctness
   - Check deprecated package replacements
   - Validate testing completeness

4. **Approval**: Require 1-2 approvals (depending on team policy)

5. **Merge Strategy**: 
   - **Recommended**: Squash merge (clean history)
   - **Alternative**: Merge commit (preserve tier history)

6. **Post-Merge**:
   - Delete `upgrade-to-NET10` branch (or keep for reference)
   - Tag merge commit: `net10-migration-complete`
   - Update CI/CD pipelines (if needed)
   - Notify team of completion

---

### Merge Criteria

**Must be TRUE before merging to dotnet9**:

? **Technical Criteria**:
- All 25 projects build successfully on net10.0
- Zero compiler errors
- Zero critical compiler warnings (or all documented and approved)
- All 5 test projects pass (100% pass rate)
- No package dependency conflicts
- No security vulnerabilities introduced

? **Quality Criteria**:
- Code review completed and approved
- High-risk projects (AuthService.Infrastructure, BlazorWeb, EmployeeService.Integration.Tests) manually validated
- End-to-end scenarios tested
- Performance benchmarks acceptable

? **Process Criteria**:
- All tier checkpoints passed
- Bottom-Up strategy principles followed (no tier skipped)
- Rollback plan documented and tested
- Migration notes documented (breaking changes, workarounds)

? **Documentation Criteria**:
- README updated (if needed)
- Breaking changes documented
- Migration notes complete
- Known issues documented (if any)

---

### Handling Merge Conflicts (If Any)

**Scenario**: `dotnet9` branch updated during migration

**Resolution**:
1. Create backup branch: `git branch upgrade-to-NET10-backup`
2. Fetch latest: `git fetch origin dotnet9`
3. Rebase or merge: `git rebase origin/dotnet9` or `git merge origin/dotnet9`
4. Resolve conflicts:
   - Prefer net10.0 target framework
   - Prefer upgraded package versions
   - Merge code changes logically
5. Re-run validation: Build + tests
6. Verify no regressions introduced
7. Continue migration or finalize PR

**Prevention**: Communicate with team to avoid changes to `dotnet9` during migration period.

---

### Bottom-Up Strategy Source Control Guidance

**Tier Ordering in Commits**:
- Commits naturally follow Bottom-Up order (Tier 1 Å® Tier 5)
- Git history reflects migration progression
- Easy to identify where issues occurred (tier-specific commits)

**Benefits of This Approach**:
- Each commit represents stable checkpoint
- Rollback granularity at tier level
- Clear audit trail of migration process
- Team can review tier-by-tier

**Commit Graph Example**:
```
* [Tier 5: App Hosts] Validate tier - all tests passing
* [Tier 5: App Hosts] Fix EmployeeService.Integration.Tests IdentityModel issues
* [Tier 5: App Hosts] Update TargetFramework to net10.0
* [Tier 4: API Services] Validate tier - all tests passing
* [Tier 4: API Services] Replace deprecated packages in API projects
* [Tier 4: API Services] Update TargetFramework to net10.0
* [Tier 3: Infrastructure] Validate tier - all tests passing
* [Tier 3: Infrastructure] Migrate AuthService.Infrastructure IdentityModel APIs
* [Tier 3: Infrastructure] Update TargetFramework to net10.0
* [Tier 2: Application] Validate tier - all tests passing
* [Tier 2: Application] Fix BlazorWeb HttpContent behavioral changes
* [Tier 2: Application] Update TargetFramework to net10.0
* [Tier 1: Foundation] Validate tier - all tests passing
* [Tier 1: Foundation] Update TargetFramework to net10.0
* [Initial] Prepare .NET 10 migration - create upgrade branch
```

---

---

## Success Criteria

### Technical Criteria

#### ? All Projects Migrated to .NET 10

**Requirement**: Every project in the solution targets net10.0

**Validation**:
```bash
# Verify all .csproj files have <TargetFramework>net10.0</TargetFramework>
grep -r "<TargetFramework>net10.0</TargetFramework>" src/ tests/
```

**Expected Result**: All 25 projects:
- src/AppHost/AppHost.csproj
- src/ServiceDefaults/ServiceDefaults.csproj
- src/Services/AttendanceService/API/AttendanceService.API.csproj
- src/Services/AttendanceService/Application/AttendanceService.Application.csproj
- src/Services/AttendanceService/Domain/AttendanceService.Domain.csproj
- src/Services/AttendanceService/Infrastructure/AttendanceService.Infrastructure.csproj
- src/Services/AuthService/API/AuthService.API.csproj
- src/Services/AuthService/Application/AuthService.Application.csproj
- src/Services/AuthService/Domain/AuthService.Domain.csproj
- src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj
- src/Services/EmployeeService/API/EmployeeService.API.csproj
- src/Services/EmployeeService/Application/EmployeeService.Application.csproj
- src/Services/EmployeeService/Domain/EmployeeService.Domain.csproj
- src/Services/EmployeeService/Infrastructure/EmployeeService.Infrastructure.csproj
- src/Services/NotificationService/API/NotificationService.API.csproj
- src/Services/NotificationService/Application/NotificationService.Application.csproj
- src/Services/NotificationService/Domain/NotificationService.Domain.csproj
- src/Services/NotificationService/Infrastructure/NotificationService.Infrastructure.csproj
- src/Shared/Contracts/Shared.Contracts.csproj
- src/WebApps/BlazorWeb/BlazorWeb.csproj
- tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj
- tests/AuthService.Tests/AuthService.Tests.csproj
- tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj
- tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj
- tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj

---

#### ? All Package Updates Applied

**Requirement**: All packages identified in assessment.md updated to target versions

**Validation**:
```bash
# List all package references and verify versions
dotnet list package --include-transitive
```

**Expected Result**:
- AuthService.Domain: 1 package updated
- ServiceDefaults: 4 packages updated
- NotificationService.Application: 1 package updated
- AttendanceService.Infrastructure: 2 packages updated
- AuthService.Infrastructure: 3 packages updated
- EmployeeService.Infrastructure: 2 packages updated
- NotificationService.Infrastructure: 3 packages updated
- AttendanceService.API: 4 packages updated (deprecated packages replaced)
- AuthService.API: 4 packages updated
- EmployeeService.API: 4 packages updated (deprecated packages replaced)
- NotificationService.API: 4 packages updated (deprecated packages replaced)
- AuthService.Tests: 5 packages updated
- AppHost: 4 packages updated (deprecated packages replaced)
- EmployeeService.Integration.Tests: 3 packages updated

**No Deprecated Packages**: Run `dotnet list package --deprecated` Å® Empty result

---

#### ? All Builds Succeed Without Errors

**Requirement**: Full solution builds successfully

**Validation**:
```bash
cd D:\Repos\runceel\DotnetEmployeeManagementSystem
dotnet clean
dotnet build DotnetEmployeeManagementSystem.slnx --configuration Release
```

**Expected Result**: 
- Exit code: 0
- Output: `Build succeeded. 0 Warning(s). 0 Error(s).`
- All 25 projects compile successfully

---

#### ? All Builds Succeed Without Warnings

**Requirement**: No compiler warnings (or all documented and approved)

**Validation**:
```bash
dotnet build DotnetEmployeeManagementSystem.slnx --configuration Release /p:TreatWarningsAsErrors=true
```

**Expected Result**: 
- Exit code: 0
- No warning messages in output
- OR: All warnings documented in known issues section

---

#### ? All Tests Pass

**Requirement**: 100% test pass rate across all test projects

**Validation**:
```bash
dotnet test DotnetEmployeeManagementSystem.slnx --configuration Release --no-build --logger "console;verbosity=detailed"
```

**Expected Result**:
- Exit code: 0
- Test projects:
  - AttendanceService.Domain.Tests: 100% pass
  - AuthService.Tests: 100% pass (JWT validation works on net10.0)
  - EmployeeService.Application.Tests: 100% pass
  - EmployeeService.Domain.Tests: 100% pass
  - EmployeeService.Integration.Tests: 100% pass (IdentityModel + HttpContent issues resolved)
- No skipped tests (unless documented)
- No flaky tests

---

#### ? No Package Dependency Conflicts

**Requirement**: All package dependencies resolve without conflicts

**Validation**:
```bash
dotnet restore DotnetEmployeeManagementSystem.slnx
```

**Expected Result**:
- Exit code: 0
- No conflict warnings
- No downgrade warnings
- All packages compatible with net10.0

---

#### ? No Security Vulnerabilities

**Requirement**: No known security vulnerabilities in dependencies

**Validation**:
```bash
dotnet list package --vulnerable --include-transitive
```

**Expected Result**:
- Output: `No vulnerable packages found`
- OR: All vulnerabilities documented and accepted (with justification)

---

### Quality Criteria

#### ? Code Quality Maintained

**Requirement**: Code quality standards upheld during migration

**Validation**:
- Run static code analysis (if enabled): `dotnet build /p:RunAnalyzers=true`
- No new code smells introduced
- Public API surfaces unchanged (unless documented)
- Code formatting consistent

**Expected Result**:
- Analysis warnings: 0 new warnings
- Code coverage: Maintained or improved (if tracked)
- API compatibility: No breaking changes in libraries (Domain, Application, Shared.Contracts)

---

#### ? Test Coverage Maintained

**Requirement**: Test coverage remains at or above baseline

**Validation** (if coverage tracking enabled):
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Expected Result**:
- Coverage percentage: >= baseline (pre-migration)
- Critical paths covered (authentication, authorization, CRUD operations)
- High-risk code covered (AuthService.Infrastructure JWT generation)

---

#### ? Documentation Updated

**Requirement**: All necessary documentation reflects .NET 10 migration

**Validation**:
- [ ] README.md: Updated with .NET 10 requirements (if applicable)
- [ ] Migration notes: Breaking changes documented
- [ ] Known issues: Any outstanding issues or workarounds documented
- [ ] API documentation: Updated if public APIs changed
- [ ] Deployment guides: Updated if deployment process changed

---

### Process Criteria

#### ? Bottom-Up Strategy Followed

**Requirement**: Migration executed according to Bottom-Up strategy principles

**Validation**:
- [ ] Tier 1 (Foundation) migrated first
- [ ] Tier 2 (Application) migrated after Tier 1 complete
- [ ] Tier 3 (Infrastructure) migrated after Tier 2 complete
- [ ] Tier 4 (API Services) migrated after Tier 3 complete
- [ ] Tier 5 (App Hosts) migrated last
- [ ] No tiers skipped
- [ ] Each tier validated before proceeding to next

**Git History Verification**:
```bash
git log --oneline --grep="Tier"
```
- Commits show progression: Tier 1 Å® Tier 5
- Each tier has validation commit

---

#### ? Source Control Strategy Followed

**Requirement**: Migration followed documented source control strategy

**Validation**:
- [ ] Work performed on `upgrade-to-NET10` branch
- [ ] Each tier has checkpoint commits
- [ ] Git tags created: `tier-1-complete` through `tier-5-complete`
- [ ] Commit messages follow format
- [ ] No merge conflicts unresolved
- [ ] PR created for final merge

**Git Verification**:
```bash
git tag --list "tier-*-complete"
```
- Expected: 5 tags (tier-1-complete through tier-5-complete)

---

#### ? Bottom-Up Principles Applied

**Requirement**: Migration demonstrated Bottom-Up strategy advantages

**Validation**:
- [ ] **Stability**: Each tier built on proven foundation (no tier blocked by downstream issues)
- [ ] **No Multi-Targeting**: All projects on net10.0 after completion (no net9.0 remaining)
- [ ] **Clear Validation**: Each tier passed validation before next tier started
- [ ] **Easier Debugging**: Issues isolated to specific tiers (documented in commits)
- [ ] **Dependency Ordering**: All projects upgraded after their dependencies

**Evidence**:
- Zero rollbacks due to dependency issues
- Clean tier-by-tier progression in Git history
- No projects on mixed frameworks at completion

---

### Functional Criteria

#### ? End-to-End Scenarios Work

**Requirement**: All major user scenarios function correctly

**Validation**:

1. **Employee Management Flow**:
   - [ ] Authenticate user via AuthService
   - [ ] Create new employee via EmployeeService
   - [ ] Retrieve employee list via EmployeeService
   - [ ] Update employee details via EmployeeService
   - [ ] Delete employee via EmployeeService

2. **Attendance Tracking Flow**:
   - [ ] Authenticate user via AuthService
   - [ ] Clock in via AttendanceService
   - [ ] Clock out via AttendanceService
   - [ ] View attendance history via AttendanceService

3. **Notification Flow**:
   - [ ] Trigger notification via NotificationService
   - [ ] Verify notification delivery

4. **Cross-Service Flow**:
   - [ ] Create employee Å® Notification sent
   - [ ] Clock in Å® Notification sent

5. **BlazorWeb UI Flow**:
   - [ ] Login page loads
   - [ ] Navigate to employee list
   - [ ] Create/edit employee via UI
   - [ ] View attendance records via UI
   - [ ] No console errors in browser

**Expected Result**: All scenarios complete successfully without errors

---

#### ? AppHost Orchestration Works

**Requirement**: .NET Aspire AppHost successfully orchestrates all services

**Validation**:
```bash
dotnet run --project src/AppHost/AppHost.csproj
```

**Expected Result**:
- [ ] All 5 services start (AttendanceService.API, AuthService.API, EmployeeService.API, NotificationService.API, BlazorWeb)
- [ ] Aspire dashboard accessible (typically http://localhost:15000)
- [ ] All services show "Running" status
- [ ] Health checks pass for all services
- [ ] Service-to-service communication works
- [ ] Logs show no critical errors

---

#### ? Performance Acceptable

**Requirement**: Performance metrics within acceptable range compared to net9.0 baseline

**Validation**:
- Measure API response times (sample endpoints)
- Measure memory usage (sample services)
- Measure startup times (services)

**Expected Result**:
- API response times: < 10% increase from baseline (acceptable)
- Memory usage: < 15% increase from baseline (acceptable)
- Startup times: < 10% increase from baseline (acceptable)
- No major performance regressions

**Recommendation**: If performance regression > 10%, investigate before declaring complete. If regression > 20%, consider rollback and optimization.

---

### Deployment Criteria

#### ? Deployment Process Verified

**Requirement**: Application can be deployed to target environment(s)

**Validation** (if applicable):
- [ ] Build release artifacts: `dotnet publish --configuration Release`
- [ ] Deploy to staging environment
- [ ] Smoke test in staging
- [ ] Verify .NET 10 runtime available on target servers
- [ ] Validate configuration files (appsettings.json) compatible

**Expected Result**:
- Deployment succeeds
- Application runs in target environment
- No runtime errors specific to .NET 10

---

### Sign-Off Checklist

**Final Verification Before Declaring Migration Complete**:

- [ ] ? All 25 projects target net10.0
- [ ] ? All package updates applied (no deprecated packages)
- [ ] ? Full solution builds without errors
- [ ] ? Full solution builds without warnings (or documented)
- [ ] ? All tests pass (100% pass rate)
- [ ] ? No package dependency conflicts
- [ ] ? No security vulnerabilities
- [ ] ? Code quality maintained
- [ ] ? Test coverage maintained
- [ ] ? Documentation updated
- [ ] ? Bottom-Up strategy followed (5 tiers completed)
- [ ] ? Source control strategy followed (checkpoints, tags, PR)
- [ ] ? End-to-end scenarios work
- [ ] ? AppHost orchestration works
- [ ] ? Performance acceptable (< 10% regression)
- [ ] ? Deployment verified (if applicable)

**Approval Required**: Tech lead or architect sign-off

---

**Migration Declared Complete When**: All checkboxes above are ?

---
