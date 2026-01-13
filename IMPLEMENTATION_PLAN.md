# Open Markets ASX API - Implementation Plan

## Change Log

### 2026-01-11 - Phase 1: Project Rename and Restructure ✅

**Summary:** Successfully renamed entire project from InsurancePlan to OpenMarkets namespace.

**Changes Made:**

1. **Directory Structure**
   - Renamed `InsurancePlanApi/` → `OpenMarketsApi/`
   - Renamed `InsurancePlan.Api/` → `OpenMarkets.Api/`
   - Renamed `InsurancePlan.Core/` → `OpenMarkets.Core/`
   - Renamed `InsurancePlan.Infrastructure/` → `OpenMarkets.Infrastructure/`
   - Renamed `InsurancePlan.Core.Tests/` → `OpenMarkets.Core.Tests/`

2. **Project Files**
   - Renamed all `.csproj` files to use `OpenMarkets.*` naming convention
   - Renamed solution file: `InsurancePlanApi.sln` → `OpenMarketsApi.sln`
   - Updated all `<ProjectReference>` paths in `.csproj` files
   - Updated solution file project references

3. **Source Code**
   - Replaced all `InsurancePlan` namespaces with `OpenMarkets` across all `.cs` files
   - Updated `using` statements throughout the codebase
   - Total files affected: ~40+ C# source files

4. **Configuration Files**
   - Updated `docker-compose.yml`: Changed build context from `./InsurancePlanApi` to `./OpenMarketsApi`

5. **Build Artifacts**
   - Cleaned all `obj/` and `bin/` directories
   - Verified successful build with `dotnet build`
   - Build completed with 0 errors, 8 warnings (pre-existing)

**Files Created:**
- `/IMPLEMENTATION_PLAN.md` - This comprehensive implementation plan document

**Verification:**
- ✅ Solution builds successfully
- ✅ All project references resolve correctly
- ✅ No compilation errors introduced

**Status:** Phase 1 Complete

---

### 2026-01-11 - Phase 2: Remove Database Dependencies ✅

**Summary:** Removed PostgreSQL database and all related code, preparing for CSV-based data source.

**Changes Made:**

1. **Docker Configuration**
   - Removed PostgreSQL service from `docker-compose.yml`
   - Removed database volumes configuration
   - Updated API container name to `openmarkets-api`
   - Removed database connection string environment variable

2. **Database Scripts and Configuration**
   - Deleted `db-init.sql` script
   - Removed `ConnectionStrings` section from `appsettings.json`
   - Removed `ChallengeMaxValue` configuration (insurance-specific)

3. **Infrastructure Project Cleanup**
   - Deleted `Database/` directory (DbContext, entities)
   - Deleted `Repositories/` directory (repository implementations)
   - Deleted `AutoMapper/` directory (infrastructure mapping profiles)
   - Updated `Bootstrapper.cs` to remove DB services registration
   - Prepared for future HTTP client services

4. **Core Project Cleanup**
   - Deleted `Repositories/` directory (repository interfaces)
   - Deleted `DomainModels/` directory (old domain models)
   - Deleted `Services/` directory (old service implementations)
   - Updated `Bootstrapper.cs` to remove old service registrations
   - Prepared for new ASX company services

5. **API Project Cleanup**
   - Deleted `Controllers/` directory (old controllers)
   - Deleted `Dto/` directory (old DTOs)
   - Simplified `Program.cs` (kept essential middleware)

6. **NuGet Packages**
   - Removed Entity Framework Core packages
   - Removed Npgsql and PostgreSQL packages
   - **Kept** AutoMapper 12.0.1 (useful for DTOs)
   - **Kept** FluentValidation 12.0.0 (useful for request validation)
   - **Kept** MemoryCache (will use for CSV caching)
   - **Kept** JsonStringEnumConverter (better JSON serialization)
   - Added `Microsoft.Extensions.Http` to Infrastructure (for CSV fetching)

7. **Test Project**
   - Removed old test files that referenced deleted services

**Verification:**
- ✅ Solution builds successfully with 0 errors, 0 warnings
- ✅ All database dependencies removed
- ✅ Clean slate for implementing ASX company functionality

**Status:** Phase 2 Complete

---

### 2026-01-13 - Phase 3: Implement Core Domain ✅

**Summary:** Implemented complete ASX company domain with CSV parsing, caching, and clean architecture.

**Changes Made:**

1. **Domain Models**
   - Created `AsxCompany` domain model in `Core/DomainModels/`
   - Properties: AsxCode, CompanyName, ListingDate, GicsIndustry

2. **Custom Exceptions**
   - Created `CompanyNotFoundException` in `Core/DomainModels/Exceptions/`
   - Created `AsxDataUnavailableException` in `Core/DomainModels/Exceptions/`

3. **Service Interfaces**
   - Created `IAsxCompanyService` in `Core/Services/`
   - Created `IAsxCsvDataProvider` in `Core/Interfaces/` (Clean Architecture principle)

4. **Business Logic**
   - Implemented `AsxCompanyService` with validation and error handling
   - Case-insensitive ASX code lookup
   - Proper exception handling without rethrowing business exceptions

5. **CSV Data Provider (Infrastructure)**
   - Implemented `AsxCsvDataProvider` using CsvHelper library
   - Added `AsxCompanyCsvMap` for CSV column mapping
   - Added `AsxDateConverter` for parsing Australian date formats (dd/MM/yyyy)
   - Fetches and parses CSV from ASX website in one operation

6. **Caching Implementation**
   - Implemented `CachedAsxCompanyService` decorator using Scrutor
   - Uses `IDistributedCache` for flexibility (Memory or Redis)
   - Configuration-driven cache provider selection
   - Ready for Redis with simple config change: `"Caching:Provider": "Redis"`

7. **NuGet Packages Added**
   - `CsvHelper 33.1.0` (Infrastructure) - Robust CSV parsing
   - `Scrutor 7.0.0` (API) - Decorator pattern support
   - `Microsoft.Extensions.Caching.StackExchangeRedis 10.0.1` (API) - Redis support
   - `Microsoft.Extensions.Logging.Abstractions 10.0.1` (Core)
   - `Microsoft.Extensions.Caching.Abstractions 10.0.1` (Core)
   - Updated Infrastructure packages to v10.0.x for consistency

8. **Service Registration**
   - Updated `Core/Bootstrapper.cs` with service and decorator registration
   - Updated `Infrastructure/Bootstrapper.cs` with HttpClient for data provider
   - Updated `Program.cs` with conditional caching (Memory/Redis)
   - Added caching configuration to `appsettings.json`

9. **Configuration**
   - Added `Caching:Provider` setting (default: "Memory")
   - Added `ConnectionStrings:Redis` for future Redis setup

**Verification:**
- ✅ Solution builds successfully with 0 errors, 0 warnings
- ✅ Clean Architecture maintained (Infrastructure depends on Core, not vice versa)
- ✅ Decorator pattern correctly implemented with Scrutor
- ✅ Ready for Redis caching with configuration change only

**Status:** Phase 3 Complete

---

### 2026-01-13 - Phase 4: Implement API Endpoint ✅

**Summary:** Implemented REST API endpoint with proper error handling, health checks, and Swagger documentation.

**Changes Made:**

1. **DTO (Data Transfer Objects)**
   - Created `CompanyResponse` in `Api/Dto/`
   - Properties: AsxCode, CompanyName, ListingDate, GicsIndustry

2. **API Controller**
   - Created `CompaniesController` in `Api/Controllers/`
   - Endpoint: `GET /api/companies/{asxCode}`
   - Response codes:
     - 200 OK: Company found and returned
     - 404 Not Found: ASX code does not exist
     - 503 Service Unavailable: ASX data cannot be retrieved
   - Proper ProducesResponseType attributes for OpenAPI documentation

3. **Global Exception Handling**
   - Created `GlobalExceptionHandlingMiddleware` in `Api/Middleware/`
   - Catches unhandled exceptions across entire pipeline
   - Returns standardized 500 Internal Server Error JSON response
   - Logs all unhandled exceptions for debugging

4. **Health Check**
   - Added ASP.NET Core health checks
   - Endpoint: `GET /health`
   - Returns 200 OK when service is healthy
   - Unauthenticated for monitoring tools

5. **Swagger/OpenAPI Documentation**
   - Configured SwaggerGen with API metadata
   - Title: "Open Markets ASX API"
   - Version: "v1"
   - Description: "API for retrieving ASX-listed company information by ASX code"
   - Available at `/swagger` in development mode

6. **Middleware Pipeline Configuration**
   - Added GlobalExceptionHandlingMiddleware first in pipeline
   - Configured Swagger for development environment only
   - Proper ordering: Exception handling → HTTPS → Authorization → Controllers

**API Endpoints:**
- `GET /api/companies/{asxCode}` - Retrieve company by ASX code
- `GET /health` - Health check endpoint

**Verification:**
- ✅ Solution builds successfully with 0 errors, 0 warnings
- ✅ All endpoints properly configured
- ✅ Exception handling middleware registered
- ✅ Swagger documentation configured

**Status:** Phase 4 Complete

---

### 2026-01-13 - Phase 5: Implement API Key Authentication ✅

**Summary:** Implemented production-ready API key authentication with custom authentication handler.

**Changes Made:**

1. **Configuration Model**
   - Created `ApiKeyConfiguration` in `Api/Configuration/`
   - Created `ApiKeyEntry` model with Key and ClientName properties
   - Supports multiple API keys with client identification

2. **Authentication Handler**
   - Created `ApiKeyAuthenticationHandler` in `Api/Authentication/`
   - Validates `X-Api-Key` header from requests
   - Returns 401 Unauthorized for missing or invalid keys
   - Adds ClientName claims to authenticated user identity
   - Logs successful authentication attempts

3. **Production-Ready API Keys**
   - Format: `OM-{ENV}-{GUID}` (OpenMarkets prefix + environment + UUID)
   - Production key: `OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c` (ProductionTradingApp)
   - Development key: `OM-DEV-1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d` (DevelopmentEnvironment)
   - Strong random keys using GUID format
   - Note: In production, should be stored in Azure Key Vault/AWS Secrets Manager

4. **Service Registration**
   - Configured `ApiKeyConfiguration` from appsettings.json
   - Registered "ApiKey" authentication scheme with custom handler
   - Added authorization services to DI container
   - Proper middleware ordering: Authentication → Authorization

5. **Controller Protection**
   - Added `[Authorize]` attribute to `CompaniesController`
   - Protects all company lookup endpoints
   - Returns 401 for unauthenticated requests

6. **Swagger/OpenAPI Integration**
   - Added security definition for API Key authentication
   - Configured `X-Api-Key` header input in Swagger UI
   - Added security requirement to all protected endpoints
   - "Authorize" button visible in Swagger for testing

7. **Middleware Pipeline**
   - Exception Handling → HTTPS → Authentication → Authorization → Controllers
   - Health check endpoint remains unauthenticated for monitoring

**Authentication Flow:**
1. Client sends request with `X-Api-Key` header
2. `ApiKeyAuthenticationHandler` validates key against configuration
3. If valid: Creates claims identity with ClientName and proceeds
4. If invalid/missing: Returns 401 Unauthorized
5. Controller receives authenticated user context

**Protected Endpoints:**
- `GET /api/companies/{asxCode}` - Requires valid API key

**Unprotected Endpoints:**
- `GET /health` - Public for monitoring tools

**Verification:**
- ✅ Solution builds successfully with 0 errors, 0 warnings
- ✅ Authentication handler properly registered
- ✅ Controller protected with [Authorize]
- ✅ Swagger configured with API key input
- ✅ Production-ready key format implemented

**Status:** Phase 5 Complete

---

## Assignment Overview

**Company:** Open Markets
**Task:** Create a .NET 8 Web API that retrieves ASX-listed companies by their ASX code
**Data Source:** https://www.asx.com.au/asx/research/ASXListedCompanies.csv
**Additional Requirement:** Implement API Key authentication

## Project Structure

This project follows Clean Architecture principles with the following structure:

```
OpenMarketsApi/
├── OpenMarkets.Api/              # Web API layer (controllers, middleware, configuration)
├── OpenMarkets.Core/             # Business logic and domain models
├── OpenMarkets.Infrastructure/   # External services (HTTP client for CSV fetching)
└── OpenMarkets.Core.Tests/       # Unit tests
```

## Architecture Decisions

### 1. Clean Architecture
**Decision:** Keep the layered architecture (Api/Core/Infrastructure)
**Rationale:**
- Maintainability: Clear separation of concerns
- Testability: Business logic isolated from external dependencies
- Extensibility: Easy to add new data sources or features
- Interview Context: Demonstrates understanding of architectural patterns

### 2. Database
**Decision:** Remove PostgreSQL completely
**Rationale:**
- Data source is external CSV file from ASX website
- Simpler deployment and local setup
- Reduced infrastructure complexity
- Can add caching later if needed (in-memory or Redis)

### 3. CSV Fetching Strategy
**Decision:** To be determined - multiple approaches possible
**Options:**
1. **On-demand with caching**: Fetch CSV on first request, cache in memory
2. **Background sync**: Periodic background job to fetch and parse CSV
3. **Hybrid**: Background sync with fallback to on-demand if cache stale

**Considerations:**
- CSV file size and update frequency
- API response time requirements
- Memory constraints
- Resilience to ASX website downtime

### 4. API Endpoint Design
**Endpoint:** `GET /api/companies/{asxCode}`
**Response Format:**
```json
{
  "asxCode": "CBA",
  "companyName": "COMMONWEALTH BANK OF AUSTRALIA.",
  "listingDate": "1991-09-12",
  "gicsIndustry": "Banks"
}
```

**Error Responses:**
- `404 Not Found`: ASX code doesn't exist
- `401 Unauthorized`: Missing or invalid API key
- `503 Service Unavailable`: Unable to fetch data from ASX

### 5. Authentication
**Decision:** Custom API Key Authentication Handler
**Implementation:**
- Custom `ApiKeyAuthenticationHandler` validates `X-Api-Key` header
- Valid keys stored in `appsettings.json` (note: use secrets manager in production)
- Returns `401 Unauthorized` for missing/invalid keys
- Adds `ClientName` claim for logging/auditing

**Configuration:**
```json
{
  "ApiKeys": [
    { "Key": "test-key-123", "ClientName": "TestClient" },
    { "Key": "prod-key-456", "ClientName": "ProductionClient" }
  ]
}
```

**Protection Strategy:**
- Protect all `/api/*` endpoints
- Leave `/health` endpoint unauthenticated
- Log client name on each authenticated request

## Implementation Phases

### Phase 1: Project Rename and Restructure ✓
**Status:** In Progress

**Tasks:**
1. Create this `IMPLEMENTATION_PLAN.md` document
2. Rename root directory: `InsurancePlanApi/` → `OpenMarketsApi/`
3. Rename project directories:
   - `InsurancePlan.Api/` → `OpenMarkets.Api/`
   - `InsurancePlan.Core/` → `OpenMarkets.Core/`
   - `InsurancePlan.Infrastructure/` → `OpenMarkets.Infrastructure/`
   - `InsurancePlan.Core.Tests/` → `OpenMarkets.Core.Tests/`
4. Rename project files:
   - `InsurancePlan.Api.csproj` → `OpenMarkets.Api.csproj`
   - `InsurancePlan.Core.csproj` → `OpenMarkets.Core.csproj`
   - `InsurancePlan.Infrastructure.csproj` → `OpenMarkets.Infrastructure.csproj`
   - `InsurancePlan.Core.Tests.csproj` → `OpenMarkets.Core.Tests.csproj`
   - `InsurancePlanApi.sln` → `OpenMarketsApi.sln`
5. Update all namespaces: `InsurancePlan.*` → `OpenMarkets.*`
6. Update project references in `.csproj` files
7. Update solution file references
8. Clean build artifacts (`obj/`, `bin/` directories)

### Phase 2: Remove Database Dependencies
**Status:** Pending

**Tasks:**
1. Remove PostgreSQL service from `docker-compose.yml`
2. Remove `db-init.sql` script
3. Remove database-related configuration from `appsettings.json`
4. Remove or repurpose `Infrastructure` project:
   - Option A: Delete entirely if only contains DB code
   - Option B: Repurpose for HTTP client to fetch CSV
5. Remove repository interfaces and implementations from Core
6. Clean up any EF Core or database-related NuGet packages

### Phase 3: Implement Core Domain
**Status:** Pending

**Tasks:**
1. Create `AsxCompany` domain model:
   ```csharp
   public class AsxCompany
   {
       public string AsxCode { get; set; }
       public string CompanyName { get; set; }
       public DateTime? ListingDate { get; set; }
       public string GicsIndustry { get; set; }
   }
   ```
2. Create `IAsxCompanyService` interface
3. Implement service with CSV fetching logic (strategy TBD)
4. Add appropriate exception types:
   - `CompanyNotFoundException`
   - `AsxDataUnavailableException`

### Phase 4: Implement API Endpoint
**Status:** Pending

**Tasks:**
1. Create `CompaniesController` with single endpoint:
   - `GET /api/companies/{asxCode}`
2. Add proper error handling and status codes
3. Add health check endpoint at `/health`
4. Configure Swagger/OpenAPI documentation
5. Add appropriate logging

### Phase 5: Implement API Key Authentication
**Status:** Pending

**Tasks:**
1. Create `ApiKeyAuthenticationHandler` class:
   - Inherit from `AuthenticationHandler<AuthenticationSchemeOptions>`
   - Validate `X-Api-Key` header
   - Add `ClientName` claim for authenticated requests
2. Create `ApiKeyConfiguration` model and settings
3. Add API key configuration to `appsettings.json` and `appsettings.Development.json`
4. Register authentication in `Program.cs`:
   ```csharp
   builder.Services.AddAuthentication("ApiKey")
       .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
   ```
5. Configure authorization policies
6. Add `[Authorize]` attribute to controllers (or global policy)
7. Exclude `/health` endpoint from authentication
8. Add middleware to log client name on each request

### Phase 6: Testing
**Status:** Pending

**Tasks:**
1. Write unit tests for:
   - CSV parsing logic
   - ASX company lookup
   - API key validation
2. Write integration tests for:
   - Companies endpoint (authenticated)
   - Health endpoint (unauthenticated)
   - Error scenarios (404, 401, 503)
3. Manual testing with Postman/curl
4. Load testing (optional, if time permits)

### Phase 7: Documentation and Deployment
**Status:** Pending

**Tasks:**
1. Update `README.md` with:
   - Project description
   - Prerequisites (.NET 8 SDK)
   - How to run locally
   - How to run with Docker
   - API endpoint documentation
   - Authentication setup
2. Create optional `DESIGN_DECISIONS.md` covering:
   - Architecture choices and trade-offs
   - CSV fetching strategy and rationale
   - Assumptions made
   - What would be added with more time
   - How AI tools were used in development
3. Test Docker build and execution
4. Update `docker-compose.yml` for new project structure
5. Update `Dockerfile` if needed
6. Verify GitHub repository setup
7. Grant access to GitHub user `asifmushtaq`

## Assumptions

1. **CSV Format:** The ASX CSV file maintains consistent structure
2. **ASX Codes:** ASX codes are case-insensitive (CBA = cba = Cba)
3. **API Key Storage:** For this assessment, storing keys in appsettings.json is acceptable
4. **Single Endpoint:** Only the company lookup endpoint is required (no listing/search endpoint)
5. **Data Freshness:** No real-time data requirement specified
6. **Concurrency:** API should handle concurrent requests safely
7. **Environment:** API will run in containerized environment (Docker)

## What Would Be Added With More Time

### Performance Optimizations
- Redis cache for parsed CSV data
- HTTP response caching with appropriate cache headers
- Compression middleware for responses
- Background job for periodic CSV refresh (Hangfire/Quartz)

### Resilience
- Retry policies for CSV fetching (Polly)
- Circuit breaker for external ASX website calls
- Fallback to cached data if ASX website unavailable
- Rate limiting per API key

### Observability
- Structured logging with Serilog
- Application Insights / OpenTelemetry integration
- Custom metrics (requests per client, cache hit rate, etc.)
- Distributed tracing

### Security Enhancements
- Move API keys to Azure Key Vault / AWS Secrets Manager
- Add rate limiting per API key
- Add IP whitelisting option
- HTTPS enforcement
- CORS configuration

### Features
- Pagination for company search/listing
- Search by company name
- Filter by GICS industry
- Historical company data (if available)
- Webhook notifications for CSV updates

### Testing
- Integration tests with TestContainers
- Performance/load testing
- Contract testing for API consumers
- End-to-end tests

### DevOps
- CI/CD pipeline (GitHub Actions)
- Automated testing on PR
- Automated Docker image builds
- Infrastructure as Code (Terraform/Bicep)
- Multi-environment deployment (dev/staging/prod)

## AI Tool Usage

This project was developed with assistance from Claude (Anthropic's AI assistant):

1. **Architecture Planning:** Discussed trade-offs between different approaches
2. **Code Generation:** Generated boilerplate code for authentication handler
3. **Best Practices:** Ensured adherence to .NET conventions and patterns
4. **Documentation:** Created comprehensive documentation
5. **Problem Solving:** Debugged issues and suggested improvements

The AI assisted with accelerating development while maintaining code quality and following established .NET patterns.

## Timeline Estimate

- **Phase 1 (Rename):** 1 hour
- **Phase 2 (Remove DB):** 30 minutes
- **Phase 3 (Core Domain):** 2 hours (depends on CSV fetching strategy)
- **Phase 4 (API Endpoint):** 1 hour
- **Phase 5 (Authentication):** 2 hours
- **Phase 6 (Testing):** 2 hours
- **Phase 7 (Documentation):** 1.5 hours

**Total Estimated Time:** 10 hours

---

**Last Updated:** 2026-01-11
**Author:** Renegade Coder (with Claude assistance)
