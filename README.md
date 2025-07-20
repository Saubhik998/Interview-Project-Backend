# AI-AudioInterviewer-BACKEND

AudioInterviewer is a backend service built with ASP.NET Core for conducting automated audio-based interviews. It exposes RESTful APIs to initialize interviews, generate questions, record answers, upload audio files to MongoDB GridFS, and generate AI-powered interview reports. Accompanied by a test suite, Docker support, and load-testing scripts, this project ensures robust, scalable, and maintainable interview workflows.

---

## Table of Contents

1. [What Is this?](#what-is-this)
2. [Architecture Overview](#architecture-overview)
3. [Folder Structure](#folder-structure)
4. [Prerequisites](#prerequisites)
5. [Running the Backend Locally](#running-the-backend-locally)
6. [Running Tests](#running-tests)
7. [Docker Support](#docker-support)
8. [Load Testing](#load-testing)
9. [Mock FastAPI Service](#mock-fastapi-service)
10. [Available APIs](#available-apis)

---

## What Is this?

AI-AudioInterviewer-BACKEND provides:

* **Interview Session Management:** Initialize sessions, generate and fetch questions.
* **Answer Submission:** Submit text or audio responses.
* **Audio Storage:** Upload and retrieve audio via MongoDB GridFS.
* **AI Report Generation:** Generate post-interview reports using an external AI service.

This service is ideal for automated screening interviews, remote assessments, and voice-based surveys.

## Architecture Overview

1. **ASP.NET Core API** (`AudioInterviewer.API`): Main backend.
2. **MongoDB**: Stores session data and audio files via GridFS.
3. **AI Service** (`mock-fastapi` / external FastAPI): Generates interview reports.
4. **Load Testing** (`load-testing`): Scripts to benchmark API performance.
5. **Test Suite** (`AudioInterviewer.Tests`): Unit and integration tests using xUnit and Moq.
6. **Docker**: Containerization with Dockerfile and Docker Compose for easy deployment.

## Folder Structure

```
├── AudioInterviewer.sln           # Solution file
├── AudioInterviewer.API          # ASP.NET Core Web API project
│   ├── Controllers/             # API controllers (InterviewController)
│   ├── Data/                    # MongoDB context and GridFS setup
│   ├── Models/                  # Data models 
│   ├── Services/                # Business logic (InterviewService)
│   ├── wwwroot/                 
│   ├── Program.cs               # Application entry point
│   └── appsettings*.json        # Configuration files
├── AudioInterviewer.Tests        # Test project
│   ├── Services/                # Unit tests for InterviewService
│   ├── Controllers/             # Integration tests for controllers
│   └── InterviewIntegrationTests.cs
├── load-testing/                # Load testing scripts (e.g., loadtest.js)
├── mock-fastapi/                # Mock AI service (FastAPI for report generation)
├── Dockerfile                   # Docker image build instructions
├── docker-compose.yml           # Compose file to orchestrate API + MongoDB
└── .dockerignore                # Files to ignore in Docker builds
```

## Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
* [MongoDB](https://www.mongodb.com/try/download/community) (local or remote)
* [Python 3.10+](https://www.python.org/downloads/) (for mock-fastapi)
* [Docker & Docker Compose](https://docs.docker.com/)

## Running the Backend Locally

1. **Configure Connection Strings**

   * Update `appsettings.json` in `AudioInterviewer.API` with your MongoDB URI.

2. **Restore & Build**

   ```bash
   cd AudioInterviewer.API
   dotnet restore
   dotnet build
   ```

3. **Run the API**

   ```bash
   dotnet run
   ```

   The API will be available at `http://localhost:7080` (or as configured in `launchSettings.json`).

## Running Tests

From the solution root:

```bash
cd AudioInterviewer.Tests
dotnet test
```

This runs both unit tests (with Moq) and integration tests against a test MongoDB instance.

## Docker Support

### Build Docker Image

```bash
docker build -t audiointerviewer:latest .
```

### Run with Docker Compose

Ensure Docker is running, then:

```bash
docker-compose up -d
```

This starts:

* **audiointerviewer\_api**: The ASP.NET Core service
* **mongodb**: MongoDB instance

To stop and remove containers:

```bash
docker-compose down
```

## Load Testing

Use the provided `load-testing/loadtest.js` (e.g., with [Artillery](https://artillery.io/) or [k6](https://k6.io/)). Example:

```bash

k6 run load-testing/loadtest.js
```


## Mock FastAPI Service

A simple FastAPI application serves as a mock AI report generator.

1. **Setup Virtual Env**

   ```bash
   cd mock-fastapi
   python -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   ```

2. **Run Mock Service**

   ```bash
   uvicorn mock_llm:app --reload
   ```

By default, it listens on `http://localhost:8000` for report generation requests.

## Available APIs

### `POST /api/interview/init`

Initializes a new interview session. Returns a session ID.

### `GET /api/interview/{sessionId}/questions`

Fetches the list of interview questions for the specified session.

### `POST /api/interview/{sessionId}/answer`

Submits a textual answer for a specific question in the session.

### `POST /api/interview/{sessionId}/answer-audio`

Uploads an audio file as an answer for a specific question.

### `GET /api/interview/{sessionId}/audio/{questionId}`

Retrieves the uploaded audio file for a specific question.

### `POST /api/interview/{sessionId}/report`

Triggers the generation of an AI-powered interview report.

---
