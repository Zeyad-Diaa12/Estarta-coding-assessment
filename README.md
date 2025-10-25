# Employee Status API

A .NET 9 Web API for retrieving employee status information with advanced features including Redis caching, PostgreSQL with logging, Polly-based resilience, and API token authentication.


## Architecture

### Technology Stack

- **Framework**: .NET 9 with FastEndpoints
- **Database**: PostgreSQL 16
- **Cache**: Redis 7
- **Logging**: Serilog with PostgreSQL sink
- **Resilience**: Polly (exponential backoff retry policies)
- **Authentication**: API token-based
- **Containerization**: Docker & Docker Compose
- **API Documentation**: Swagger/OpenAPI

## Quick Start

### Prerequisites

- Docker Desktop installed and running
- Git (to clone the repository)

### Running the Application

1. **Clone the repository** (or navigate to the project directory):
   ```powershell
   cd f:\Estarta-coding-assessment
   ```

2. **Start all services with Docker Compose**:
   ```powershell
   docker-compose up --build
   ```

   This will start:
   - PostgreSQL on `localhost:5432`
   - Redis on `localhost:6379`
   - API on `http://localhost:5000`

3. **Wait for services to be healthy** (check logs for "Employee Status API started")

4. **Access Swagger UI**:
   ```
   http://localhost:5000/swagger
   ```

## API Endpoints

### 1. Get Employee Status

**Endpoint**: `POST /api/GetEmpStatus`

**Headers**:
```
Authorization: Bearer secure-api-token-2025
```

**Request Body**:
```json
{
  "nationalNumber": "NAT1001"
}
```

**Success Response** (200):
```json
{
  "employeeName": "jdoe",
  "nationalNumber": "NAT1001",
  "averageSalary": 2587.50,
  "highestSalary": 3300.00,
  "sumSalary": 31050.00,
  "status": "GREEN",
  "isActive": true,
  "lastUpdated": "2025-10-25T14:13:00Z"
}
```

**Error Responses**:

- **404** - User not found:
  ```json
  { "error": "Invalid National Number" }
  ```

- **406** - User not active:
  ```json
  { "error": "User is not Active" }
  ```

- **422** - Insufficient salary data (< 3 records):
  ```json
  { "error": "INSUFFICIENT_DATA" }
  ```

- **401** - Invalid/missing token:
  ```json
  { "error": "Unauthorized" }
  ```

- **400** - Invalid request format:
  ```json
  { "error": "National Number is required" }
  ```

### 2. Health Check

**Endpoint**: `GET /health`

**Response** (200):
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "PostgreSQL",
      "status": "Healthy",
      "description": null,
      "duration": 45.2
    },
    {
      "name": "Redis",
      "status": "Healthy",
      "description": null,
      "duration": 12.8
    }
  ],
  "totalDuration": 58.0
}
```

### 3. Admin Logs

**Endpoint**: `GET /admin/logs`

**Headers**:
```
Authorization: Bearer secure-api-token-2025
```

**Response** (200): Array of log entries from the database

## Testing with Sample Data

The database is pre-seeded with test data. Here are the test scenarios:

### Resilience

Polly retry policies (exponential backoff):
- **Database operations**: 3 retries (2s, 4s, 8s delays)
- **Redis operations**: 3 retries (2s, 4s, 8s delays)
- All retry attempts are logged with context