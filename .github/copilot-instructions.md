<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# DelayedQ API Project

This is a .NET 8 Web API project for managing delayed job execution with callback support.

## Project Structure
- **Models**: Data models for Job entities
- **Controllers**: API controllers for handling HTTP requests
- **Services**: Business logic and service implementations
- **DTOs**: Data Transfer Objects for API requests/responses

## API Conventions
- Use RESTful conventions
- Return appropriate HTTP status codes
- Use proper JSON serialization
- Include validation for incoming requests
- Support both HTTP and SQS callback types

## Key Features
- Job creation with callback support
- Support for HTTP and SQS callback types
- Flexible JSON payload handling
- Event-driven architecture
