# Open Markets ASX API

Production-ready REST API for retrieving Australian Stock Exchange (ASX) listed company information.

## Overview

This API provides access to ASX-listed company data by company code. It fetches data from the official ASX CSV file and implements an optimal caching strategy designed for high-performance trading applications.

## Features

- **ASX Company Lookup**: Query companies by their ASX code (e.g., CBA, BHP)
- **API Key Authentication**: Secure access using X-Api-Key header authentication
- **Optimized Caching**: Entire company list cached for 24 hours, reducing external API calls by 2000x
- **Distributed Cache Support**: Redis for production, in-memory for development
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

The API uses a decorator pattern to cache the entire ASX company list (~2000 companies) with a single cache key for optimal performance:

- **First request (any company)**: Fetches and parses entire CSV, caches all companies
- **Subsequent requests (any company)**: Retrieves from cache, performs in-memory lookup
- **Cache Duration**: 24 hours
- **Cache Key**: `AsxCompanies_All`
- **Benefits**: Minimal network calls, predictable performance, efficient for trading applications

## Prerequisites

- .NET 10.0 SDK (LTS)
- (Optional) Redis for distributed caching
- (Optional) Docker for containerized deployment

## Getting Started

### Running Locally

1. Navigate to the API project:
```bash
cd OpenMarketsApi/OpenMarkets.Api
```

2. Run the application:
```bash
dotnet run
```

3. Access the API at `https://localhost:7001` or `http://localhost:5001`

4. View Swagger documentation at `https://localhost:7001/swagger`

### Configuration

The application can be configured via `appsettings.json`:

```json
{
  "ApiKeys": {
    "Keys": [
      {
        "Key": "your-api-key-here",
        "ClientName": "YourClientName"
      }
    ]
  },
  "Caching": {
    "Provider": "Memory"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

#### Caching Configuration

**In-Memory Cache** (default):
```json
{
  "Caching": {
    "Provider": "Memory"
  }
}
```

**Redis Cache**:
```json
{
  "Caching": {
    "Provider": "Redis"
  },
  "ConnectionStrings": {
    "Redis": "your-redis-connection-string"
  }
}
```

## API Usage

### Authentication

All API endpoints (except `/health`) require authentication using the `X-Api-Key` header.

```bash
curl -H "X-Api-Key: OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c" \
  https://localhost:7001/api/companies/CBA
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

## Docker Deployment

### Using Docker Compose

1. Build and start all services:
```bash
docker-compose up --build
```

2. The API will be available at `http://localhost:8080`

3. Stop services:
```bash
docker-compose down
```

### Using Docker Only

1. Build the image:
```bash
docker build -t openmarkets-api -f OpenMarketsApi/Dockerfile .
```

2. Run the container:
```bash
docker run -p 8080:8080 openmarkets-api
```

## API Keys

API key configured in `appsettings.json`:

- **API Key**: `OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c`

**Security Note**: In production, store API keys in secure configuration providers (Azure Key Vault, AWS Secrets Manager, etc.) rather than in configuration files.

## License

This project is provided as-is for assessment purposes.
