# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade src/AppHost/AppHost.csproj
4. Upgrade src/ServiceDefaults/ServiceDefaults.csproj
5. Upgrade src/Services/AttendanceService/API/AttendanceService.API.csproj
6. Upgrade src/Services/AttendanceService/Application/AttendanceService.Application.csproj
7. Upgrade src/Services/AttendanceService/Domain/AttendanceService.Domain.csproj
8. Upgrade src/Services/AttendanceService/Infrastructure/AttendanceService.Infrastructure.csproj
9. Upgrade src/Services/AuthService/API/AuthService.API.csproj
10. Upgrade src/Services/AuthService/Application/AuthService.Application.csproj
11. Upgrade src/Services/AuthService/Domain/AuthService.Domain.csproj
12. Upgrade src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj
13. Upgrade src/Services/EmployeeService/API/EmployeeService.API.csproj
14. Upgrade src/Services/EmployeeService/Application/EmployeeService.Application.csproj
15. Upgrade src/Services/EmployeeService/Domain/EmployeeService.Domain.csproj
16. Upgrade src/Services/EmployeeService/Infrastructure/EmployeeService.Infrastructure.csproj
17. Upgrade src/Services/NotificationService/API/NotificationService.API.csproj
18. Upgrade src/Services/NotificationService/Application/NotificationService.Application.csproj
19. Upgrade src/Services/NotificationService/Domain/NotificationService.Domain.csproj
20. Upgrade src/Services/NotificationService/Infrastructure/NotificationService.Infrastructure.csproj
21. Upgrade src/Shared/Contracts/Shared.Contracts.csproj
22. Upgrade src/WebApps/BlazorWeb/BlazorWeb.csproj
23. Upgrade tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj
24. Upgrade tests/AuthService.Tests/AuthService.Tests.csproj
25. Upgrade tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj
26. Upgrade tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj
27. Upgrade tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj
28. Run unit tests to validate upgrade in the projects listed below:
  tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj
  tests/AuthService.Tests/AuthService.Tests.csproj
  tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj
  tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj
  tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                                   | Current Version                     | New Version          | Description                                               |
|:-----------------------------------------------|:------------------------------------:|:--------------------:|:----------------------------------------------------------|
| Aspire.Hosting.AppHost                          | 9.5.2                               | 13.0.0               | Recommended for .NET 10.0                                 |
| Aspire.Hosting.Redis                            | 9.5.2                               | 13.0.0               | Recommended for .NET 10.0                                 |
| Aspire.StackExchange.Redis                      | 9.5.2                               | 13.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.AspNetCore.Authentication.JwtBearer   | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 9.0.10                           | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.AspNetCore.OpenApi                    | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.EntityFrameworkCore                   | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.EntityFrameworkCore.Design            | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.EntityFrameworkCore.InMemory          | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.EntityFrameworkCore.Sqlite            | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Configuration              | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Configuration.Binder       | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 9.0.10                     | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Configuration.Json         | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Hosting.Abstractions       | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Http.Resilience            | 9.10.0                             | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.Logging.Abstractions       | 9.0.10                              | 10.0.0               | Recommended for .NET 10.0                                 |
| Microsoft.Extensions.ServiceDiscovery           | 9.5.2                               | 10.0.0               | Recommended for .NET 10.0                                 |
| OpenTelemetry.Instrumentation.AspNetCore        | 1.13.0                              | 1.14.0-rc.1          | Recommended for .NET 10.0                                 |
| OpenTelemetry.Instrumentation.Http              | 1.13.0                              | 1.14.0-rc.1          | Recommended for .NET 10.0                                 |

### Project upgrade details

#### src/AppHost/AppHost.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.Hosting.AppHost should be updated from `9.5.2` to `13.0.0` (*recommended for .NET 10.0*)
  - Aspire.Hosting.Redis should be updated from `9.5.2` to `13.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/ServiceDefaults/ServiceDefaults.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Http.Resilience should be updated from `9.10.0` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.ServiceDiscovery should be updated from `9.5.2` to `10.0.0` (*recommended for .NET 10.0*)
  - OpenTelemetry.Instrumentation.AspNetCore should be updated from `1.13.0` to `1.14.0-rc.1` (*recommended for .NET 10.0*)
  - OpenTelemetry.Instrumentation.Http should be updated from `1.13.0` to `1.14.0-rc.1` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/AttendanceService/API/AttendanceService.API.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.StackExchange.Redis should be updated from `9.5.2` to `13.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/AttendanceService/Application/AttendanceService.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/AttendanceService/Domain/AttendanceService.Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/AttendanceService/Infrastructure/AttendanceService.Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore.Design should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/AuthService/API/AuthService.API.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Design should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/AuthService/Application/AuthService.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/AuthService/Domain/AuthService.Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/AuthService/Infrastructure/AuthService.Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Design should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/EmployeeService/API/EmployeeService.API.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.StackExchange.Redis should be updated from `9.5.2` to `13.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.Authentication.JwtBearer should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/EmployeeService/Application/EmployeeService.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/EmployeeService/Domain/EmployeeService.Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/EmployeeService/Infrastructure/EmployeeService.Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore.Design should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/NotificationService/API/NotificationService.API.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Aspire.StackExchange.Redis should be updated from `9.5.2` to `13.0.0` (*recommended for .NET 10.0*)
  - Microsoft.AspNetCore.OpenApi should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Design should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Services/NotificationService/Application/NotificationService.Application.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/NotificationService/Domain/NotificationService.Domain.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/Services/NotificationService/Infrastructure/NotificationService.Infrastructure.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.Sqlite should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Hosting.Abstractions should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### src/Shared/Contracts/Shared.Contracts.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### src/WebApps/BlazorWeb/BlazorWeb.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### tests/AttendanceService.Domain.Tests/AttendanceService.Domain.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### tests/AuthService.Tests/AuthService.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore.InMemory should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Configuration should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Configuration.Binder should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Configuration.EnvironmentVariables should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Configuration.Json should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None

#### tests/EmployeeService.Application.Tests/EmployeeService.Application.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### tests/EmployeeService.Domain.Tests/EmployeeService.Domain.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

Other changes:
  - None

#### tests/EmployeeService.Integration.Tests/EmployeeService.Integration.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.Mvc.Testing should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.EntityFrameworkCore.InMemory should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)
  - Microsoft.Extensions.Configuration should be updated from `9.0.10` to `10.0.0` (*recommended for .NET 10.0*)

Other changes:
  - None
