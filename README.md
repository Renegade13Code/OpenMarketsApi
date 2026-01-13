# Open Markets ASX API

Production-ready REST API for retrieving ASX company information by code, with optimized caching for high-performance trading applications.

## Features

- **ASX Company Lookup**: Query companies by their ASX code (e.g., CBA, BHP)
- **API Key Authentication**: Secure access using X-Api-Key header authentication
- **Distributed Caching**: Redis for optimal performance
- **Real-time Data**: Fetches data from official ASX source
- **Clean Architecture**: Separation of concerns with Core/Infrastructure/API layers
- **Production Ready**: Comprehensive error handling, logging, and health checks
- **Docker Support**: Containerized deployment with Docker Compose

## Architecture

The solution follows Clean Architecture principles with four main projects:

- **OpenMarkets.Api**: ASP.NET Core Web API layer with controllers, authentication, and middleware
- **OpenMarkets.Core**: Business logic, domain models, and service interfaces
- **OpenMarkets.Infrastructure**: External concerns like CSV data providers with caching decorator
- **OpenMarkets.Core.Tests**: Unit tests for core business logic (6 tests)
- **OpenMarkets.Infrastructure.Tests**: Unit tests for infrastructure layer (3 tests)

### Caching Strategy

The entire ASX company list (~2000 companies) is cached for 24 hours on first request. Subsequent lookups retrieve from cache for optimal performance with minimal network calls.

## Prerequisites

- Docker and Docker Compose

## Getting Started

1. Build and start all services:
```bash
docker-compose up --build
```

2. The API will be available at `http://localhost:8080`

3. Stop services:
```bash
docker-compose down
```

## API Usage

### Authentication

All API endpoints (except `/health`) require authentication using the `X-Api-Key` header.

```bash
curl -H "X-Api-Key: OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c" \
  http://localhost:8080/api/companies/CBA
```

### Endpoints

- `GET /api/companies/{asxCode}` - Get company information by ASX code
- `GET /health` - Health check endpoint (no authentication required)

## Running Tests

### Unit Tests (9 total)

```bash
# Run all unit tests
cd OpenMarketsApi
dotnet test
```

### Integration Tests (9 total)

```bash
# Run integration tests (requires API to be running)
cd OpenMarketsApi/tests/integration
npm install
npm test
```


## API Keys

API key configured in `appsettings.json`:

- **API Key**: `OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c`

**Security Note**: In production, store API keys in secure configuration providers (Azure Key Vault, AWS Secrets Manager, etc.) rather than in configuration files.

## Design Decisions & Trade-offs

**Caching Strategy**: The entire ASX company list (~2000 companies) is cached for 24 hours instead of per-company caching. This reduces external API calls and ensures predictable low-latency performance for all lookups after the first request. The trade-off is higher memory usage (~200KB), but this is acceptable given the performance benefits for trading applications.

**API Key Authentication**: Simple X-Api-Key header authentication was chosen over OAuth2/Cognito for B2B API-to-API communication. API keys provide minimal latency overhead for high-frequency trading requests, are simple for clients to integrate, and are standard for B2B service authentication. OAuth2/Cognito would add unnecessary complexity and token refresh overhead for server-to-server communication where clients manage their own credentials securely.

**Architecture**: Decorator pattern for caching separation, Redis for distributed cache scalability, and Clean Architecture for testability and maintainability.

## Assumptions

- ASX CSV structure remains stable with 3 columns (Company name, ASX code, GICS industry group)
- 24-hour cache duration is acceptable as company listings change infrequently
- Single API instance with horizontal scaling capability is sufficient for initial load

## Future Improvements

- **Additional Endpoints**: List all companies, search by name, filter by industry
- **Monitoring & Observability**: Prometheus metrics, response time tracking, alerting on failures
- **Rate Limiting**: Per-API-key rate limits to prevent abuse
- **API Versioning**: URL-based versioning for backward compatibility
- **Enhanced Authentication**: API key tiers (read-only, standard, premium), key rotation mechanism