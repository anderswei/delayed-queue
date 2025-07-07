# DelayedQ API

A .NET 8 Web API project for managing delayed job execution with callback support.

## Features

- **Job Creation**: Create jobs with flexible JSON payloads
- **Callback Support**: Support for both HTTP and SQS callback types
- **RESTful API**: Clean REST API endpoints
- **Validation**: Request validation and proper error handling

## API Endpoints

### POST /job
Create a new job with callback support.

**Request Body:**
```json
{
  "eventId": "string", // Unique event identifier (serves as the key)
  "callbackPayload": {}, // Any JSON object
  "callbackType": "HTTP" | "SQS",
  "callbackUrl": "string", // HTTP URL or SQS queue URL
  "timestamp": "datetime" // When the job should be executed (ISO 8601 format)
}
```

**Response:**
```json
{
  "eventId": "string",
  "callbackPayload": {},
  "callbackType": "HTTP" | "SQS",
  "callbackUrl": "string",
  "timestamp": "datetime",
  "createdAt": "datetime",
  "executedAt": "datetime?",
  "status": "string"
}
```

### GET /job/{eventId}
Retrieve a specific job by EventId.

### GET /job
Retrieve all jobs.

## Partition Management

### POST /partition
Create daily partitions for a specified number of consecutive days.

**Request Body:**
```json
{
  "startDate": "2024-01-01T00:00:00Z",
  "numberOfDays": 7
}
```

**Response:**
```json
{
  "isSuccess": true,
  "totalPartitionsRequested": 7,
  "partitionsCreated": 5,
  "partitionsSkipped": 2,
  "partitionsFailed": 0,
  "createdPartitionNames": ["Jobs_20240101", "Jobs_20240102", "Jobs_20240103", "Jobs_20240104", "Jobs_20240105"],
  "skippedPartitionNames": ["Jobs_20240106", "Jobs_20240107"],
  "failedPartitionNames": []
}
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL database (or Docker to run the provided PostgreSQL container)
- Visual Studio Code or Visual Studio

### Database Setup

#### Option 1: Using Docker (Recommended for Development)
1. Start PostgreSQL using Docker Compose:
   ```bash
   docker-compose up -d postgres
   ```

2. Apply database migrations (includes partitioning setup):
   ```bash
   dotnet ef database update
   ```

#### Option 2: Using Local PostgreSQL
1. Install PostgreSQL locally (version 12+ required for partitioning)
2. Create a database named `delayed_q_dev`
3. Update the connection string in `appsettings.Development.json` if needed
4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```

### Table Partitioning

The application automatically sets up table partitioning for the Jobs table:

- **Partitioned by:** `Timestamp` field using range partitioning
- **Default partition size:** 1 day per partition
- **Composite primary key:** (EventId, Timestamp)
- **Initial partitions:** 7 days starting from current date

#### Partition Management API

The partition creation API automatically creates **1 partition per day** for the specified date range:

**Create daily partitions:**
```bash
curl -X POST "https://localhost:5001/partition" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01T00:00:00Z",
    "numberOfDays": 7
  }'
```
*This creates 7 daily partitions: Jobs_20240101, Jobs_20240102, ..., Jobs_20240107*

**Request Parameters:**
- `startDate`: The first date to create partitions for
- `numberOfDays`: Number of consecutive daily partitions to create (default: 7)

**Features:**
- ✅ **Automatic daily partitioning**: Always creates 1 partition per day
- ✅ **Existence checking**: Skips partitions that already exist
- ✅ **Bulk creation**: Creates multiple consecutive daily partitions
- ✅ **Detailed reporting**: Shows created, skipped, and failed partitions

**Response example:**
```json
{
  "isSuccess": true,
  "totalPartitionsRequested": 7,
  "partitionsCreated": 5,
  "partitionsSkipped": 2,
  "partitionsFailed": 0,
  "createdPartitionNames": ["Jobs_20240101", "Jobs_20240102", "Jobs_20240103", "Jobs_20240104", "Jobs_20240105"],
  "skippedPartitionNames": ["Jobs_20240106", "Jobs_20240107"],
  "failedPartitionNames": []
}
```

**Create daily partitions:**
```bash
curl -X POST "https://localhost:5001/partition" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01T00:00:00Z",
    "numberOfDays": 7
  }'
```

**View all partitions:**
```bash
curl -X GET "https://localhost:5001/partition"
```

### Running the Application

1. Clone the repository
2. Navigate to the project directory
3. Set up the database (see Database Setup above)
4. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` (or the port specified in the output).

### Testing the API

You can use the provided `DelayedQ.http` file to test the API endpoints, or use tools like Postman or curl.

Example curl command:
```bash
curl -X POST "https://localhost:5001/job" \
  -H "Content-Type: application/json" \
  -d '{
    "eventId": "test-event-123",
    "callbackPayload": {
      "message": "Hello, World!",
      "timestamp": "2024-01-01T00:00:00Z"
    },
    "callbackType": "HTTP",
    "callbackUrl": "https://example.com/callback",
    "timestamp": "2024-01-01T10:00:00Z"
  }'
```

## Project Structure

- `Controllers/` - API controllers
- `Models/` - Data models
- `DTOs/` - Data Transfer Objects
- `Services/` - Business logic services
- `Properties/` - Launch settings and configuration

## Configuration

The application uses standard ASP.NET Core configuration with the following settings:

### Database Configuration
- **appsettings.json**: Production database settings
- **appsettings.Development.json**: Development database settings

Example connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=delayed_q_dev;Username=postgres;Password=password"
  }
}
```

### Application Settings
- General application settings in `appsettings.json`
- Development-specific settings in `appsettings.Development.json`

## Database

The application uses PostgreSQL as the backend database with Entity Framework Core for data access.

### Database Schema
- **Jobs** table: Stores job information with EventId as primary key
- **Indexes**: Created on Timestamp, Status, and CreatedAt for performance
- **JSON Support**: CallbackPayload is stored as JSONB for efficient querying

### Migrations
To create a new migration:
```bash
dotnet ef migrations add MigrationName
```

To update the database:
```bash
dotnet ef database update
```

## Development

This project follows standard .NET Web API conventions:
- Dependency injection for services
- Model validation using data annotations
- Proper HTTP status codes
- Structured logging

## Next Steps

This is the foundation for the DelayedQ API. Additional features to be implemented:
1. Additional job management endpoints
2. Job scheduling and execution
3. Database persistence
4. Authentication and authorization
5. Background job processing
