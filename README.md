# Full Stack Data Processing Pipeline

## Overview
This project is a modern, full-stack monorepo featuring a microservices architecture designed to ingest, stream, process, and visualize data. The architecture revolves around event-driven data streaming using Kafka, supported by specialized backend APIs and a Next.js user interface.

## Architecture & Technologies

### 🖥️ Frontend Interface (`frontend/`)
- **Framework**: Next.js 16 (React 19)
- **Styling**: Tailwind CSS v4
- **Authentication**: NextAuth.js (Google OAuth)
- **Role**: User-facing dashboard and ingestion interface. Runs in development mode with Hot Module Replacement (HMR).
- **Port**: `3000`

### ⚙️ .NET Minimal API (`minimal-net-api/`)
- **Framework**: .NET 10.0 Minimal API (C#)
- **Role**: High-performance API for data ingestion, CSV processing, and acting as a Kafka Producer/Consumer.
- **Logging**: Serilog integrated directly with Elasticsearch.
- **Port**: `8080`

### 🐍 FastAPI Python Service (`fastapi_service/`)
- **Framework**: FastAPI (Python 3.11) with Uvicorn
- **Role**: Data processing service that interfaces with the relational databases.
- **Databases Supported**: SQLAlchemy configured to connect to both PostgreSQL and Azure SQL Edge.
- **Port**: `8000`

### 🐘 Relational Databases
- **PostgreSQL**: Reliable, open-source relational database (`postgres:15-alpine`). **Port**: `5432`
- **Azure SQL Edge**: Microsoft SQL Server optimized for IoT and development environments. **Port**: `1433`
- *(Initialization scripts are stored in the `sql-scripts/` directory)*

### 📨 Event Streaming
- **Apache Kafka & Zookeeper**: The event-streaming backbone of the architecture (using Confluent Platform 7.5.0). **Port**: `9092`
- **Kafka UI**: A graphical web interface by ProvectusLabs for managing, monitoring, and debugging Kafka clusters. **Port**: `8081`

### 🔍 Logging & Monitoring
- **Elasticsearch**: Centralized data and log storage engine. **Port**: `9200`
- **Kibana**: Interactive data visualization dashboard for analyzing data from Elasticsearch. **Port**: `5601`

### 🐳 Infrastructure & Tooling
- **Docker & Docker Compose**: The entire stack is containerized and orchestrated via `docker-compose.yml`.
- **DevContainers**: Comes with a fully configured `.devcontainer` setup for VS Code / GitHub Codespaces, ensuring a standardized development environment with necessary extensions (ESLint, Prettier, C#, Python, etc.).

## Getting Started

### Prerequisites
- Docker and Docker Compose
- *(Alternatively, simply open the project in GitHub Codespaces or a VS Code Dev Container!)*

### Running the Project
Bring up the entire microservice infrastructure with a single command:
```bash
docker compose up -d --build
```

The services are configured to watch for file changes where applicable (e.g., `dotnet watch`, `uvicorn --reload`, and `next dev`), giving you a seamless local development experience.

### Mass Data Input
Raw CSV data files (such as `Customers.csv`) intended for ingestion testing can be found in the `Massive-Data-Input/` directory.