# .NET 10 Upgrade - Execution Tasks

## Progress Dashboard

**Overall Progress**: 1/35 tasks complete (3%) ![3%](https://progress-bar.xyz/3)

**By Tier**:
- Tier 1 (Foundation): 1/4 complete
- Tier 2 (Application & Frontend): 0/6 complete  
- Tier 3 (Infrastructure): 0/6 complete
- Tier 4 (API Services): 0/6 complete
- Tier 5 (App Hosts): 0/6 complete
- Validation & Completion: 0/7 complete

**Status Legend**:
- `[ ]` Not started
- `[?]` In progress
- `[?]` Complete
- `[?]` Failed
- `[?]` Skipped

---

## Tier 1: Foundation Layer

### [?] TASK-001: Verify prerequisites and prepare for Tier 1 migration *(Completed: 2026-01-17 11:44)*
**Tier**: 1 (Foundation)  
**Risk**: Low  
**Dependencies**: None

**Actions**:
- [?] (1) Verify .NET 10 SDK installed on machine
        Expected: `dotnet --list-sdks` shows 10.x.x version
- [?] (2) Verify no pending changes in Git repository
        Expected: `git status` shows clean working tree
- [?] (3) Review Tier 1 projects list (6 projects):
        - AttendanceService.Domain
        - AuthService.Domain  
        - EmployeeService.Domain
        - NotificationService.Domain
        - ServiceDefaults
        - Shared.Contracts
- [?] (4) Verify all Tier 1 projects currently on net9.0
        Expected: All 6 .csproj files have `<TargetFramework>net9.0</TargetFramework>`

**Verification**:
- .NET 10 SDK available
- Git working tree clean
- All 6 Tier 1 projects identified and on net9.0

**Commit**: Do not commit (preparation only)

---

### [?] TASK-002: Update Tier 1 projects to .NET 10
**Tier**: 1 (Foundation)  
**Risk**: Low  
**Dependencies**: TASK-001

**Actions**:
- [?] (1) Update AttendanceService.Domain.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Update AuthService.Domain.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (3) Update EmployeeService.Domain.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (4) Update NotificationService.Domain.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (5) Update ServiceDefaults.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (6) Update Shared.Contracts.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

**Verification**:
- All 6 .csproj files now have `<TargetFramework>net10.0</TargetFramework>`
- `dotnet restore` succeeds for all 6 projects
- No errors during restore

**Commit**: `[Tier 1: Foundation] Update TargetFramework to net10.0`

---

### [ ] TASK-003: Build and test Tier 1 projects
**Tier**: 1 (Foundation)  
**Risk**: Low  
**Dependencies**: TASK-002

**Actions**:
- [ ] (1) Build all Tier 1 projects
        Command: `dotnet build --configuration Release`
        Expected: All 6 projects build successfully
- [ ] (2) Run Domain.Tests (from Tier 2, but tests Tier 1)
        Command: `dotnet test tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj`
        Expected: All tests pass
- [ ] (3) Run Domain.Tests (from Tier 2, but tests Tier 1)
        Command: `dotnet test tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj`
        Expected: All tests pass

**Verification**:
- All 6 Tier 1 projects build without errors
- All 6 Tier 1 projects build without warnings
- AttendanceService.Domain.Tests pass (100%)
- EmployeeService.Domain.Tests pass (100%)

**Commit**: Do not commit (testing only)

---

### [ ] TASK-004: Tag Tier 1 completion
**Tier**: 1 (Foundation)  
**Risk**: Low  
**Dependencies**: TASK-003

**Actions**:
- [ ] (1) Create Git tag for Tier 1 completion
        Command: `git tag -a tier-1-complete -m "Tier 1 (Foundation) migration complete and validated"`
- [ ] (2) Push tag to remote
        Command: `git push origin tier-1-complete`

**Verification**:
- Tag `tier-1-complete` created
- Tag pushed to remote repository

**Commit**: Final commit for Tier 1: `[Tier 1: Foundation] Validate tier - all tests passing`

---

## Tier 2: Application & Frontend Layer

### [ ] TASK-005: Prepare for Tier 2 migration
**Tier**: 2 (Application & Frontend)  
**Risk**: Medium (BlazorWeb high-risk)  
**Dependencies**: TASK-004

**Actions**:
- [ ] (1) Review Tier 2 projects list (7 projects):
        - AttendanceService.Application
        - AuthService.Application
        - EmployeeService.Application
        - NotificationService.Application
        - BlazorWeb ?? (HIGH RISK - 53 API issues)
        - AttendanceService.Domain.Tests
        - EmployeeService.Domain.Tests
- [ ] (2) Verify all Tier 2 projects currently on net9.0
- [ ] (3) Identify BlazorWeb behavioral changes:
        - 52 HttpContent behavioral changes
        - 1 Task.WhenAll source incompatible

**Verification**:
- All 7 Tier 2 projects identified
- BlazorWeb risks documented

**Commit**: Do not commit (preparation only)

---

### [ ] TASK-006: Update Application layers and Domain.Tests to .NET 10
**Tier**: 2 (Application & Frontend)  
**Risk**: Low  
**Dependencies**: TASK-005

**Actions**:
- [ ] (1) Update AttendanceService.Application.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Update AuthService.Application.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (3) Update EmployeeService.Application.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (4) Update NotificationService.Application.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (5) Update AttendanceService.Domain.Tests.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (6) Update EmployeeService.Domain.Tests.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

**Verification**:
- All 6 .csproj files now have `<TargetFramework>net10.0</TargetFramework>`
- `dotnet restore` succeeds for all 6 projects

**Commit**: `[Tier 2: Application] Update Application layers and Domain.Tests to net10.0`

---

### [ ] TASK-007: Update BlazorWeb to .NET 10
**Tier**: 2 (Application & Frontend)  
**Risk**: Medium-High  
**Dependencies**: TASK-006

**Actions**:
- [ ] (1) Update BlazorWeb.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Restore packages
        Command: `dotnet restore src/WebApps/BlazorWeb/BlazorWeb.csproj`
        Expected: Restore succeeds

**Verification**:
- BlazorWeb.csproj now has `<TargetFramework>net10.0</TargetFramework>`
- Package restore succeeds

**Commit**: `[Tier 2: Application] Update BlazorWeb to net10.0`

---

### [ ] TASK-008: Build and test Application layers and Domain.Tests
**Tier**: 2 (Application & Frontend)  
**Risk**: Low  
**Dependencies**: TASK-006

**Actions**:
- [ ] (1) Build Application layers
        Command: `dotnet build --configuration Release`
        Projects: AttendanceService.Application, AuthService.Application, EmployeeService.Application, NotificationService.Application
        Expected: All 4 projects build successfully
- [ ] (2) Run Domain.Tests
        Command: `dotnet test tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj`
        Expected: All tests pass
- [ ] (3) Run Domain.Tests
        Command: `dotnet test tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj`
        Expected: All tests pass

**Verification**:
- All 6 projects build without errors
- All Domain.Tests pass (100%)

**Commit**: Do not commit (testing only)

---

### [ ] TASK-009: Build and manually test BlazorWeb
**Tier**: 2 (Application & Frontend)  
**Risk**: Medium-High  
**Dependencies**: TASK-007

**Actions**:
- [ ] (1) Build BlazorWeb
        Command: `dotnet build src/WebApps/BlazorWeb/BlazorWeb.csproj --configuration Release`
        Expected: Build succeeds
- [ ] (2) Launch BlazorWeb locally
        Command: `dotnet run --project src/WebApps/BlazorWeb/BlazorWeb.csproj`
        Expected: Application starts without errors
- [ ] (3) Manual UI testing:
        - Navigate to home page
        - Test all major navigation links
        - Verify no console errors in browser
        - Test HTTP client calls (if any visible features)
- [ ] (4) Stop application
        Expected: Graceful shutdown

**Verification**:
- BlazorWeb builds without errors
- Application launches successfully
- UI navigation works
- No critical console errors
- Application stops gracefully

**Commit**: Do not commit (testing only)

---

### [ ] TASK-010: Tag Tier 2 completion
**Tier**: 2 (Application & Frontend)  
**Risk**: Low  
**Dependencies**: TASK-008, TASK-009

**Actions**:
- [ ] (1) Create Git tag for Tier 2 completion
        Command: `git tag -a tier-2-complete -m "Tier 2 (Application & Frontend) migration complete and validated"`
- [ ] (2) Push tag to remote
        Command: `git push origin tier-2-complete`

**Verification**:
- Tag `tier-2-complete` created
- Tag pushed to remote repository

**Commit**: Final commit for Tier 2: `[Tier 2: Application] Validate tier - all tests passing`

---

## Tier 3: Infrastructure Layer

### [ ] TASK-011: Prepare for Tier 3 migration
**Tier**: 3 (Infrastructure)  
**Risk**: High (AuthService.Infrastructure critical)  
**Dependencies**: TASK-010

**Actions**:
- [ ] (1) Review Tier 3 projects list (5 projects):
        - AttendanceService.Infrastructure
        - AuthService.Infrastructure ?? (HIGH RISK - 17 IdentityModel issues)
        - EmployeeService.Infrastructure
        - NotificationService.Infrastructure
        - EmployeeService.Application.Tests
- [ ] (2) Review AuthService.Infrastructure breaking changes:
        - 15 binary incompatible (System.IdentityModel.Tokens.Jwt APIs)
        - 2 source incompatible (IdentityEntityFrameworkBuilderExtensions)
        - File: src/Services/AuthService/Infrastructure/Services/JwtTokenGenerator.cs
- [ ] (3) Research Microsoft.IdentityModel.* migration path
        Read documentation for replacing JWT APIs

**Verification**:
- All 5 Tier 3 projects identified
- AuthService.Infrastructure breaking changes documented
- Migration path researched

**Commit**: Do not commit (preparation only)

---

### [ ] TASK-012: Update non-Auth Infrastructure layers to .NET 10
**Tier**: 3 (Infrastructure)  
**Risk**: Low  
**Dependencies**: TASK-011

**Actions**:
- [ ] (1) Update AttendanceService.Infrastructure.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Update EmployeeService.Infrastructure.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (3) Update NotificationService.Infrastructure.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (4) Update EmployeeService.Application.Tests.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

**Verification**:
- All 4 .csproj files now have `<TargetFramework>net10.0</TargetFramework>`
- `dotnet restore` succeeds for all 4 projects

**Commit**: `[Tier 3: Infrastructure] Update non-Auth Infrastructure layers to net10.0`

---

### [ ] TASK-013: Update AuthService.Infrastructure to .NET 10 and migrate IdentityModel APIs ??
**Tier**: 3 (Infrastructure)  
**Risk**: High  
**Dependencies**: TASK-011

**Actions**:
- [ ] (1) Update AuthService.Infrastructure.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Attempt build to see compilation errors
        Command: `dotnet build src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj`
        Expected: Build fails with IdentityModel errors
- [ ] (3) Fix JwtTokenGenerator.cs breaking changes (assessment line 37-62):
        File: src/Services/AuthService/Infrastructure/Services/JwtTokenGenerator.cs
        - Replace `JwtSecurityTokenHandler` usage
        - Replace `JwtSecurityToken` constructor
        - Replace `JwtRegisteredClaimNames` constants
        - Update JWT token generation logic
- [ ] (4) Rebuild AuthService.Infrastructure
        Expected: Build succeeds

**Verification**:
- AuthService.Infrastructure.csproj has `<TargetFramework>net10.0</TargetFramework>`
- Build succeeds without errors
- No IdentityModel API errors remain

**Commit**: `[Tier 3: Infrastructure] Migrate AuthService.Infrastructure IdentityModel APIs to net10.0`

---

### [ ] TASK-014: Build and test non-Auth Infrastructure layers
**Tier**: 3 (Infrastructure)  
**Risk**: Low  
**Dependencies**: TASK-012

**Actions**:
- [ ] (1) Build Infrastructure layers
        Command: `dotnet build --configuration Release`
        Projects: AttendanceService.Infrastructure, EmployeeService.Infrastructure, NotificationService.Infrastructure
        Expected: All 3 projects build successfully
- [ ] (2) Run EmployeeService.Application.Tests
        Command: `dotnet test tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj`
        Expected: All tests pass

**Verification**:
- All 4 projects build without errors
- EmployeeService.Application.Tests pass (100%)

**Commit**: Do not commit (testing only)

---

### [ ] TASK-015: Test AuthService.Infrastructure JWT functionality ??
**Tier**: 3 (Infrastructure)  
**Risk**: High  
**Dependencies**: TASK-013

**Actions**:
- [ ] (1) Build AuthService.Infrastructure
        Command: `dotnet build src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj --configuration Release`
        Expected: Build succeeds
- [ ] (2) Manual JWT token generation test:
        - Option A: Run AuthService.API (if available) and test login endpoint
        - Option B: Create temporary test console app
        - Generate JWT token
        - Verify token structure
        - Verify claims present
- [ ] (3) Validate token generation succeeds
        Expected: JWT token generated successfully with correct claims

**Verification**:
- AuthService.Infrastructure builds without errors
- JWT token generation works
- Token structure valid
- Claims extraction works

**Commit**: Do not commit (testing only)

---

### [ ] TASK-016: Tag Tier 3 completion
**Tier**: 3 (Infrastructure)  
**Risk**: Low  
**Dependencies**: TASK-014, TASK-015

**Actions**:
- [ ] (1) Create Git tag for Tier 3 completion
        Command: `git tag -a tier-3-complete -m "Tier 3 (Infrastructure) migration complete and validated"`
- [ ] (2) Push tag to remote
        Command: `git push origin tier-3-complete`

**Verification**:
- Tag `tier-3-complete` created
- Tag pushed to remote repository

**Commit**: Final commit for Tier 3: `[Tier 3: Infrastructure] Validate tier - all tests passing`

---

## Tier 4: API Services Layer

### [ ] TASK-017: Prepare for Tier 4 migration
**Tier**: 4 (API Services)  
**Risk**: Medium  
**Dependencies**: TASK-016

**Actions**:
- [ ] (1) Review Tier 4 projects list (5 projects):
        - AttendanceService.API
        - AuthService.API
        - EmployeeService.API
        - NotificationService.API
        - AuthService.Tests ?? (HIGH RISK - 20 IdentityModel issues)
- [ ] (2) Identify deprecated packages in API projects
        Research replacements for deprecated packages
- [ ] (3) Review AuthService.Tests breaking changes:
        - 16 binary incompatible (System.IdentityModel.Tokens.Jwt APIs)
        - 4 source incompatible

**Verification**:
- All 5 Tier 4 projects identified
- Deprecated package replacements researched
- AuthService.Tests risks documented

**Commit**: Do not commit (preparation only)

---

### [ ] TASK-018: Update API services to .NET 10
**Tier**: 4 (API Services)  
**Risk**: Low-Medium  
**Dependencies**: TASK-017

**Actions**:
- [ ] (1) Update AttendanceService.API.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Update AuthService.API.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (3) Update EmployeeService.API.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (4) Update NotificationService.API.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`

**Verification**:
- All 4 .csproj files now have `<TargetFramework>net10.0</TargetFramework>`
- `dotnet restore` succeeds for all 4 projects

**Commit**: `[Tier 4: API Services] Update API services to net10.0`

---

### [ ] TASK-019: Update AuthService.Tests to .NET 10 and migrate IdentityModel APIs ??
**Tier**: 4 (API Services)  
**Risk**: Medium  
**Dependencies**: TASK-017

**Actions**:
- [ ] (1) Update AuthService.Tests.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Attempt build to see compilation errors
        Command: `dotnet build tests/AuthService.Tests/AuthService.Tests.csproj`
        Expected: Build fails with IdentityModel errors
- [ ] (3) Fix test files breaking changes:
        - Replace `JwtSecurityTokenHandler` usage in tests
        - Replace `JwtSecurityToken` usage in tests
        - Replace `JwtRegisteredClaimNames` constants in tests
        - Update JWT token validation logic in tests
- [ ] (4) Rebuild AuthService.Tests
        Expected: Build succeeds

**Verification**:
- AuthService.Tests.csproj has `<TargetFramework>net10.0</TargetFramework>`
- Build succeeds without errors
- No IdentityModel API errors remain

**Commit**: `[Tier 4: API Services] Migrate AuthService.Tests IdentityModel APIs to net10.0`

---

### [ ] TASK-020: Build and test API services
**Tier**: 4 (API Services)  
**Risk**: Low  
**Dependencies**: TASK-018

**Actions**:
- [ ] (1) Build all API services
        Command: `dotnet build --configuration Release`
        Projects: AttendanceService.API, AuthService.API, EmployeeService.API, NotificationService.API
        Expected: All 4 projects build successfully
- [ ] (2) Quick smoke test - launch EmployeeService.API
        Command: `dotnet run --project src/Services/EmployeeService/API/EmployeeService.API.csproj`
        Expected: API starts without errors
- [ ] (3) Verify Swagger UI loads
        Navigate to Swagger endpoint (typically /swagger)
        Expected: Swagger documentation loads
- [ ] (4) Stop API
        Expected: Graceful shutdown

**Verification**:
- All 4 API projects build without errors
- Sample API launches successfully
- Swagger UI accessible
- API stops gracefully

**Commit**: Do not commit (testing only)

---

### [ ] TASK-021: Run AuthService.Tests
**Tier**: 4 (API Services)  
**Risk**: Medium  
**Dependencies**: TASK-019

**Actions**:
- [ ] (1) Run AuthService.Tests
        Command: `dotnet test tests/AuthService.Tests/AuthService.Tests.csproj --configuration Release`
        Expected: All tests pass

**Verification**:
- AuthService.Tests build without errors
- All tests pass (100%)
- JWT validation in tests works correctly

**Commit**: Do not commit (testing only)

---

### [ ] TASK-022: Tag Tier 4 completion
**Tier**: 4 (API Services)  
**Risk**: Low  
**Dependencies**: TASK-020, TASK-021

**Actions**:
- [ ] (1) Create Git tag for Tier 4 completion
        Command: `git tag -a tier-4-complete -m "Tier 4 (API Services) migration complete and validated"`
- [ ] (2) Push tag to remote
        Command: `git push origin tier-4-complete`

**Verification**:
- Tag `tier-4-complete` created
- Tag pushed to remote repository

**Commit**: Final commit for Tier 4: `[Tier 4: API Services] Validate tier - all tests passing`

---

## Tier 5: Application Hosts

### [ ] TASK-023: Prepare for Tier 5 migration
**Tier**: 5 (App Hosts)  
**Risk**: Medium  
**Dependencies**: TASK-022

**Actions**:
- [ ] (1) Review Tier 5 projects list (2 projects):
        - AppHost
        - EmployeeService.Integration.Tests ?? (HIGH RISK - 48 issues)
- [ ] (2) Review EmployeeService.Integration.Tests breaking changes:
        - 13 binary incompatible (IdentityModel)
        - 35 behavioral changes (HttpContent)
- [ ] (3) Prepare end-to-end test scenarios
        Document expected behaviors for validation

**Verification**:
- Both Tier 5 projects identified
- Integration test risks documented
- End-to-end scenarios documented

**Commit**: Do not commit (preparation only)

---

### [ ] TASK-024: Update AppHost to .NET 10
**Tier**: 5 (App Hosts)  
**Risk**: Low-Medium  
**Dependencies**: TASK-023

**Actions**:
- [ ] (1) Update AppHost.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Restore packages
        Command: `dotnet restore src/AppHost/AppHost.csproj`
        Expected: Restore succeeds

**Verification**:
- AppHost.csproj has `<TargetFramework>net10.0</TargetFramework>`
- Package restore succeeds

**Commit**: `[Tier 5: App Hosts] Update AppHost to net10.0`

---

### [ ] TASK-025: Update EmployeeService.Integration.Tests to .NET 10 ??
**Tier**: 5 (App Hosts)  
**Risk**: Medium-High  
**Dependencies**: TASK-023

**Actions**:
- [ ] (1) Update EmployeeService.Integration.Tests.csproj
        Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
- [ ] (2) Attempt build to see compilation errors
        Command: `dotnet build tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj`
        Expected: Build may fail with IdentityModel or HttpContent errors
- [ ] (3) Fix integration test breaking changes:
        - Replace `JwtSecurityTokenHandler` usage in tests
        - Replace `JwtSecurityToken` usage in tests
        - Update HttpContent usage for behavioral changes
        - Update test expectations if needed
- [ ] (4) Rebuild EmployeeService.Integration.Tests
        Expected: Build succeeds

**Verification**:
- EmployeeService.Integration.Tests.csproj has `<TargetFramework>net10.0</TargetFramework>`
- Build succeeds without errors

**Commit**: `[Tier 5: App Hosts] Update EmployeeService.Integration.Tests to net10.0`

---

### [ ] TASK-026: Test AppHost orchestration
**Tier**: 5 (App Hosts)  
**Risk**: Medium  
**Dependencies**: TASK-024

**Actions**:
- [ ] (1) Build AppHost
        Command: `dotnet build src/AppHost/AppHost.csproj --configuration Release`
        Expected: Build succeeds
- [ ] (2) Launch AppHost
        Command: `dotnet run --project src/AppHost/AppHost.csproj`
        Expected: All services start without errors
- [ ] (3) Verify Aspire dashboard accessible
        Navigate to dashboard (typically http://localhost:15000)
        Expected: Dashboard loads, all services show "Running"
- [ ] (4) Verify health checks
        Expected: All services report healthy
- [ ] (5) Stop AppHost
        Expected: Graceful shutdown of all services

**Verification**:
- AppHost builds without errors
- All 5 services start (AttendanceService.API, AuthService.API, EmployeeService.API, NotificationService.API, BlazorWeb)
- Aspire dashboard accessible
- All health checks pass
- Graceful shutdown

**Commit**: Do not commit (testing only)

---

### [ ] TASK-027: Run EmployeeService.Integration.Tests ??
**Tier**: 5 (App Hosts)  
**Risk**: Medium-High  
**Dependencies**: TASK-025

**Actions**:
- [ ] (1) Run EmployeeService.Integration.Tests
        Command: `dotnet test tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj --configuration Release`
        Expected: All tests pass

**Verification**:
- EmployeeService.Integration.Tests build without errors
- All tests pass (100%)
- Full-stack validation succeeds

**Commit**: Do not commit (testing only)

---

### [ ] TASK-028: Tag Tier 5 completion
**Tier**: 5 (App Hosts)  
**Risk**: Low  
**Dependencies**: TASK-026, TASK-027

**Actions**:
- [ ] (1) Create Git tag for Tier 5 completion
        Command: `git tag -a tier-5-complete -m "Tier 5 (App Hosts) migration complete and validated"`
- [ ] (2) Push tag to remote
        Command: `git push origin tier-5-complete`

**Verification**:
- Tag `tier-5-complete` created
- Tag pushed to remote repository

**Commit**: Final commit for Tier 5: `[Tier 5: App Hosts] Validate tier - all tests passing`

---

## Final Validation & Completion

### [ ] TASK-029: Full solution build validation
**Tier**: Validation  
**Risk**: Low  
**Dependencies**: TASK-028

**Actions**:
- [ ] (1) Clean build entire solution
        Commands:
        ```bash
        cd D:\Repos\runceel\DotnetEmployeeManagementSystem
        dotnet clean
        dotnet build DotnetEmployeeManagementSystem.slnx --configuration Release
        ```
        Expected: All 25 projects build successfully
- [ ] (2) Verify no compiler warnings
        Expected: Output shows "0 Warning(s)"

**Verification**:
- All 25 projects build without errors
- Zero compiler warnings (or all documented)
- Exit code: 0

**Commit**: Do not commit (validation only)

---

### [ ] TASK-030: Full solution test validation
**Tier**: Validation  
**Risk**: Low  
**Dependencies**: TASK-029

**Actions**:
- [ ] (1) Run all tests in solution
        Command: `dotnet test DotnetEmployeeManagementSystem.slnx --configuration Release --logger "console;verbosity=detailed"`
        Expected: All test projects pass (100%)

**Verification**:
- AttendanceService.Domain.Tests: 100% pass
- AuthService.Tests: 100% pass
- EmployeeService.Application.Tests: 100% pass
- EmployeeService.Domain.Tests: 100% pass
- EmployeeService.Integration.Tests: 100% pass
- No skipped tests
- Exit code: 0

**Commit**: Do not commit (validation only)

---

### [ ] TASK-031: End-to-end scenario validation
**Tier**: Validation  
**Risk**: Low  
**Dependencies**: TASK-030

**Actions**:
- [ ] (1) Launch AppHost for end-to-end testing
        Command: `dotnet run --project src/AppHost/AppHost.csproj`
- [ ] (2) Test Employee Management Flow:
        - Authenticate user via AuthService
        - Create new employee via EmployeeService
        - Retrieve employee list via EmployeeService
        - Update employee details via EmployeeService
        - Delete employee via EmployeeService
- [ ] (3) Test Attendance Tracking Flow:
        - Authenticate user via AuthService
        - Clock in via AttendanceService
        - Clock out via AttendanceService
        - View attendance history via AttendanceService
- [ ] (4) Test BlazorWeb UI:
        - Navigate to BlazorWeb
        - Login
        - Navigate to employee list
        - Create/edit employee via UI
        - No console errors in browser
- [ ] (5) Stop AppHost
        Expected: Graceful shutdown

**Verification**:
- All end-to-end scenarios complete successfully
- No runtime errors
- No console errors in browser
- Services communicate correctly

**Commit**: Do not commit (validation only)

---

### [ ] TASK-032: Security and package validation
**Tier**: Validation  
**Risk**: Low  
**Dependencies**: TASK-031

**Actions**:
- [ ] (1) Check for vulnerable packages
        Command: `dotnet list package --vulnerable --include-transitive`
        Expected: "No vulnerable packages found"
- [ ] (2) Check for deprecated packages
        Command: `dotnet list package --deprecated`
        Expected: No deprecated packages
- [ ] (3) Verify all package dependencies resolve
        Command: `dotnet restore DotnetEmployeeManagementSystem.slnx`
        Expected: No conflicts, no downgrade warnings

**Verification**:
- No security vulnerabilities
- No deprecated packages
- No package dependency conflicts

**Commit**: Do not commit (validation only)

---

### [ ] TASK-033: Performance comparison (optional)
**Tier**: Validation  
**Risk**: Low  
**Dependencies**: TASK-031

**Actions**:
- [ ] (1) Measure API response times (sample endpoints)
        Compare to net9.0 baseline (if available)
- [ ] (2) Measure memory usage (sample services)
        Compare to net9.0 baseline (if available)
- [ ] (3) Measure startup times (services)
        Compare to net9.0 baseline (if available)

**Verification**:
- API response times: < 10% increase from baseline (acceptable)
- Memory usage: < 15% increase from baseline (acceptable)
- Startup times: < 10% increase from baseline (acceptable)

**Commit**: Do not commit (validation only)

---

### [ ] TASK-034: Update documentation
**Tier**: Completion  
**Risk**: Low  
**Dependencies**: TASK-032

**Actions**:
- [ ] (1) Update README.md (if needed)
        - Add .NET 10 requirements
        - Update setup instructions
- [ ] (2) Document breaking changes
        - Create or update MIGRATION.md
        - Document IdentityModel API changes
        - Document HttpContent behavioral changes
- [ ] (3) Document known issues (if any)
        - Create or update KNOWN_ISSUES.md

**Verification**:
- Documentation updated
- Breaking changes documented
- Known issues documented (if any)

**Commit**: `[Migration Complete] Update documentation for .NET 10`

---

### [ ] TASK-035: Final commit and create pull request
**Tier**: Completion  
**Risk**: Low  
**Dependencies**: TASK-034

**Actions**:
- [ ] (1) Final commit (if not done in TASK-034)
        Message: `[Migration Complete] .NET 10 upgrade - all 25 projects migrated and validated`
- [ ] (2) Push all commits and tags to remote
        Commands:
        ```bash
        git push origin upgrade-to-NET10
        git push origin --tags
        ```
- [ ] (3) Create Pull Request: `upgrade-to-NET10` ¨ `dotnet9`
        Title: ".NET 10 Migration - All 25 projects"
        Description: Link to plan.md, summary of changes
- [ ] (4) Complete PR checklist (from plan.md Success Criteria)
- [ ] (5) Request code review

**Verification**:
- All commits pushed to remote
- All tags pushed to remote
- Pull request created
- PR checklist complete

**Commit**: Already done in step (1)

---

## Execution Log

Detailed execution log will be appended here as tasks are completed.

---

**End of Tasks**
