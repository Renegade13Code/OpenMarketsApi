# Open Markets ASX API

Production-ready REST API for retrieving Australian Stock Exchange (ASX) listed company information.

## Overview

This API provides access to ASX-listed company data by company code. It fetches real-time data from the official ASX CSV file and provides it through a secure, cached REST endpoint.

## Features

- **ASX Company Lookup**: Query companies by their ASX code (e.g., CBA, BHP)
- **API Key Authentication**: Secure access using X-Api-Key header authentication
- **Distributed Caching**: Support for both in-memory and Redis caching
- **Real-time Data**: Fetches data from official ASX source
- **Clean Architecture**: Separation of concerns with Core/Infrastructure/API layers
- **Production Ready**: Comprehensive error handling, logging, and health checks

## Architecture

The solution follows Clean Architecture principles with three main projects:

- **OpenMarkets.Api**: ASP.NET Core Web API layer with controllers, authentication, and middleware
- **OpenMarkets.Core**: Business logic, domain models, and service interfaces
- **OpenMarkets.Infrastructure**: External concerns like CSV data providers and database access
- **OpenMarkets.Core.Tests**: Unit tests for core business logic

## Prerequisites

- .NET 8.0 SDK
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

Run all unit tests:
```bash
dotnet test OpenMarketsApi/OpenMarkets.Core.Tests/OpenMarkets.Core.Tests.csproj
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

Production API keys are configured in `appsettings.json`:

- **Production**: `OM-PROD-8f9e2c7a-4b3d-4e8f-9c1a-2d5e6f7a8b9c`
- **Development**: `OM-DEV-1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d`

**Security Note**: In production, store API keys in secure configuration providers (Azure Key Vault, AWS Secrets Manager, etc.) rather than in configuration files.

## License

This project is provided as-is for assessment purposes.
