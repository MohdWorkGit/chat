# Customer Engagement Platform

A self-hosted, omnichannel customer engagement and support platform built with **.NET 8** and **Angular 17+**. The platform enables businesses to manage customer conversations across web chat, email, and API channels, automate workflows, and deliver AI-assisted support — all from a unified dashboard.

Inspired by Chatwoot, this project is engineered for **fully offline / air-gapped deployment**: every cloud dependency has been replaced with a self-hostable equivalent so the entire stack can run on isolated networks with no internet connectivity.

---

## Table of Contents

1. [Key Features](#key-features)
2. [Architecture](#architecture)
3. [Technology Stack](#technology-stack)
4. [Repository Layout](#repository-layout)
5. [Prerequisites](#prerequisites)
6. [Quick Start (Docker Compose)](#quick-start-docker-compose)
7. [Running Locally for Development](#running-locally-for-development)
8. [Configuration](#configuration)
9. [Service Endpoints](#service-endpoints)
10. [Testing](#testing)
11. [Database Migrations](#database-migrations)
12. [Offline / Air-Gapped Deployment](#offline--air-gapped-deployment)
13. [Project Documentation](#project-documentation)
14. [Contributing](#contributing)
15. [License](#license)

---

## Key Features

### Communication Channels
- **Web Widget** — Embeddable JavaScript chat widget with HMAC security, pre-chat forms, domain whitelisting, file attachments, and email continuity.
- **Email** — IMAP inbound / SMTP outbound with proper threading via `In-Reply-To` and `References` headers.
- **API Channel** — REST channel with HMAC token auth and outbound webhooks for integration with third-party systems.

### Conversation & Contact Management
- Full CRUD for conversations and contacts with statuses (open, resolved, pending, snoozed) and priorities (low → urgent).
- Message types: incoming, outgoing, activity, and template messages with file/image/video/audio attachments.
- Manual or round-robin assignment to agents and teams, with watchers and `@mentions`.
- Labels, custom attributes, saved filter views, bulk actions, full-text search, draft auto-save.
- Contact merge, CSV import, IP geolocation via offline MaxMind GeoLite2.
- CSAT surveys triggered after resolution.

### Inbox, Team & User Management
- Per-inbox working hours, out-of-office responses, assignment policies, and CSAT templates.
- Teams with member management and team-level assignment.
- Multi-account (multi-tenant) memberships, role-based access control, custom roles (Enterprise).

### Automation & AI
- Workflow automation engine with triggers, conditions, and actions.
- Canned responses and message templates.
- Captain AI assistant with self-hosted LLM inference (Ollama / vLLM) and Rasa NLU integration.
- SLA policies, business-hours-aware escalations, and reporting.

### Reporting & Analytics
- Conversation, agent, inbox, label, and team reports.
- CSAT analytics and SLA compliance dashboards.
- Live agent and conversation monitoring.

### Enterprise
- Captain AI, Custom Roles, and SAML SSO via self-hosted IdPs (e.g., Keycloak).

For the full feature inventory, see [`PROJECT_SPEC.md`](./PROJECT_SPEC.md).

---

## Architecture

The backend follows **Clean Architecture** with clear separation between domain, application, infrastructure, and presentation layers:

```
┌──────────────────────────────────────────────────────────┐
│              CustomerEngagement.Api (ASP.NET)            │
│   Controllers · Middleware · Auth · SignalR Hubs         │
└──────────────────────────────────────────────────────────┘
                            │
┌──────────────────────────────────────────────────────────┐
│           CustomerEngagement.Application                 │
│   CQRS · Services · Background Jobs · DTOs · Validators  │
└──────────────────────────────────────────────────────────┘
                            │
┌──────────────────────────────────────────────────────────┐
│              CustomerEngagement.Core                     │
│   Entities · Enums · Domain Events · Interfaces          │
└──────────────────────────────────────────────────────────┘
                            ▲
┌──────────────────────────────────────────────────────────┐
│           CustomerEngagement.Infrastructure              │
│   EF Core · Repositories · External Integrations        │
└──────────────────────────────────────────────────────────┘
```

`CustomerEngagement.Enterprise` builds on top with premium capabilities (Captain AI, Custom Roles, SAML SSO).

The frontend is composed of three independent Angular applications that consume the same API surface:

- **Dashboard** — internal agent/admin management portal.
- **Portal** — customer-facing self-service help center.
- **Widget** — embeddable chat widget for third-party websites.

---

## Technology Stack

| Layer            | Technology                                                       |
|------------------|------------------------------------------------------------------|
| Backend          | .NET 8, ASP.NET Core, EF Core, MediatR, FluentValidation, SignalR |
| Database         | PostgreSQL 16 with `pgvector` extension                          |
| Cache / Queue    | Redis 7                                                          |
| Object Storage   | MinIO (S3-compatible)                                            |
| Email            | SMTP / IMAP (MailHog for development)                            |
| AI / NLU         | Ollama (LLM inference) and Rasa (dialogue engine)                |
| GeoIP            | MaxMind GeoLite2 (offline `.mmdb`)                               |
| Frontend         | Angular 17+, TypeScript, RxJS                                    |
| Containerization | Docker / Docker Compose                                          |
| Identity (SSO)   | Self-hosted SAML IdP (e.g., Keycloak)                            |

---

## Repository Layout

```
.
├── src/
│   ├── CustomerEngagement.Api             # ASP.NET Core Web API host
│   ├── CustomerEngagement.Application     # CQRS, services, background jobs
│   ├── CustomerEngagement.Core            # Domain entities, enums, interfaces
│   ├── CustomerEngagement.Infrastructure  # EF Core, repositories, integrations
│   └── CustomerEngagement.Enterprise      # Captain AI, Custom Roles, SAML SSO
├── frontend/
│   ├── dashboard                          # Agent/admin Angular app
│   ├── portal                             # Customer help-center Angular app
│   └── widget                             # Embeddable chat widget
├── tests/
│   ├── CustomerEngagement.Api.Tests
│   ├── CustomerEngagement.Application.Tests
│   ├── CustomerEngagement.Infrastructure.Tests
│   ├── CustomerEngagement.Tests
│   └── e2e                                # End-to-end tests
├── docker-compose.yml                     # Full stack orchestration
├── CustomerEngagement.sln                 # .NET solution file
└── PROJECT_SPEC.md                        # Full feature specification
```

---

## Prerequisites

To run the platform you will need:

- **Docker** 24+ and **Docker Compose** v2 (recommended path)

For local (non-containerized) development:

- **.NET 8 SDK**
- **Node.js 20+** and **npm 10+**
- **Angular CLI 17+** (`npm install -g @angular/cli`)
- **PostgreSQL 16** (with the `pgvector` extension)
- **Redis 7**
- **MinIO** (or any S3-compatible storage)

---

## Quick Start (Docker Compose)

The fastest way to get the entire platform running is via Docker Compose. This brings up the API, all three frontends, PostgreSQL, Redis, and MinIO in a single command.

```bash
# 1. Clone the repository
git clone <your-repo-url>
cd chat

# 2. (Optional) Create a .env file to override defaults
cp .env.example .env   # if provided; otherwise create as needed

# 3. Start the core stack
docker compose up -d

# 4. Start with development extras (MailHog SMTP catcher)
docker compose --profile dev up -d

# 5. Start with AI services (Ollama + Rasa)
docker compose --profile ai up -d

# 6. View logs
docker compose logs -f api
```

Once all containers are healthy:

| Service         | URL                          |
|-----------------|------------------------------|
| API             | http://localhost:8080        |
| Dashboard       | http://localhost:3000        |
| Help Portal     | http://localhost:3100        |
| Chat Widget     | http://localhost:3200        |
| MinIO Console   | http://localhost:9001        |
| MailHog UI      | http://localhost:8025 (`dev` profile) |
| Ollama API      | http://localhost:11434 (`ai` profile) |
| Rasa API        | http://localhost:5005 (`ai` profile)  |

To stop the stack:

```bash
docker compose down            # stop containers
docker compose down -v         # stop and remove all volumes (destructive)
```

---

## Running Locally for Development

### Backend API

```bash
# Restore and build
dotnet restore
dotnet build

# Apply database migrations
dotnet ef database update \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api

# Run the API (defaults to https://localhost:5001)
dotnet run --project src/CustomerEngagement.Api
```

You can also start just the infrastructure dependencies (Postgres, Redis, MinIO) via Docker while running the API natively:

```bash
docker compose up -d postgres redis minio minio-init
dotnet run --project src/CustomerEngagement.Api
```

### Frontend Applications

Each Angular application is independent and can be developed in isolation:

```bash
# Dashboard (agent/admin portal)
cd frontend/dashboard
npm install
npm start         # http://localhost:4200

# Help Portal
cd frontend/portal
npm install
npm start

# Embeddable Widget
cd frontend/widget
npm install
npm start
```

---

## Configuration

The API is configured via environment variables (or `appsettings.{Environment}.json`). The most important settings:

| Variable                              | Description                                         | Default                     |
|---------------------------------------|-----------------------------------------------------|-----------------------------|
| `ASPNETCORE_ENVIRONMENT`              | `Development` / `Production`                        | `Production`                |
| `ConnectionStrings__DefaultConnection`| PostgreSQL connection string                        | (see `docker-compose.yml`)  |
| `Redis__Url`                          | Redis host and port                                 | `redis:6379`                |
| `Jwt__Secret`                         | JWT signing secret (≥ 32 chars)                     | **must be set in prod**     |
| `Jwt__Issuer` / `Jwt__Audience`       | JWT issuer and audience                             | `CustomerEngagement`        |
| `Storage__Provider`                   | Storage backend (`S3`)                              | `S3`                        |
| `Storage__S3__Endpoint`               | MinIO / S3 endpoint URL                             | `http://minio:9000`         |
| `Storage__S3__AccessKey` / `SecretKey`| Object storage credentials                          | `minioadmin` / `minioadmin` |
| `Storage__S3__BucketName`             | Bucket name                                         | `customer-engagement`       |
| `Email__Smtp__Host` / `Port`          | Outbound SMTP host and port                         | `mailhog` / `1025`          |
| `Email__Smtp__FromAddress`            | Default From address                                | `noreply@example.com`       |
| `Ollama__BaseUrl`                     | Ollama LLM endpoint                                 | `http://ollama:11434`       |
| `Rasa__BaseUrl`                       | Rasa NLU endpoint                                   | `http://rasa:5005`          |
| `Cors__AllowedOrigins__N`             | Allowed CORS origins (indexed list)                 | dashboard/portal/widget URLs|

> **Production:** Always override `Jwt__Secret`, MinIO credentials, and database passwords with strong, unique values.

---

## Service Endpoints

| Endpoint                  | Description                       |
|---------------------------|-----------------------------------|
| `GET  /health`            | Liveness probe                    |
| `GET  /swagger`           | OpenAPI / Swagger UI (dev only)   |
| `POST /api/v1/auth/login` | Authenticate and receive JWT      |
| `GET  /api/v1/conversations` | List conversations             |
| `GET  /api/v1/contacts`   | List contacts                     |
| `WS   /hubs/conversation` | SignalR real-time conversation hub|

The complete API surface is documented via Swagger when running in `Development`.

---

## Testing

```bash
# Run the entire .NET test suite
dotnet test

# Run a specific project
dotnet test tests/CustomerEngagement.Application.Tests

# Run frontend unit tests
cd frontend/dashboard && npm test
```

End-to-end tests live in `tests/e2e` and can be executed against a running Compose stack.

---

## Database Migrations

The project uses **EF Core migrations**. To create or apply migrations:

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api

# Apply pending migrations
dotnet ef database update \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api
```

See [`src/CustomerEngagement.Infrastructure/Persistence/Migrations/README.md`](./src/CustomerEngagement.Infrastructure/Persistence/Migrations/README.md) for migration conventions.

---

## Offline / Air-Gapped Deployment

This platform is designed to operate on isolated networks. Every cloud dependency has been replaced with a self-hosted equivalent:

| Cloud Service                       | Self-Hosted Replacement                                |
|-------------------------------------|--------------------------------------------------------|
| AWS S3 / Azure Blob                 | **MinIO** (S3-compatible)                              |
| Google Dialogflow                   | **Rasa Open Source**                                   |
| Firebase Cloud Messaging            | **Self-hosted Web Push** (VAPID + custom relay)        |
| MaxMind GeoIP (online)              | **MaxMind GeoLite2** offline `.mmdb`                   |
| OpenAI / cloud LLMs                 | **Ollama** or **vLLM**                                 |
| Google / Microsoft OAuth            | Local SMTP credentials or **SAML IdP** (e.g., Keycloak)|
| Cloud CI/CD                         | **Gitea/GitLab + Drone CI** + local container registry |
| npm / NuGet registries              | **Verdaccio** (npm) + **BaGet** (NuGet) mirrors        |
| Seq Cloud                           | **Seq self-hosted** or **Grafana Loki**                |

Before deploying to an air-gapped environment, pre-load all container images, npm packages, NuGet packages, and the GeoLite2 database onto the target network.

---

## Project Documentation

- [`PROJECT_SPEC.md`](./PROJECT_SPEC.md) — Complete feature specification and architectural reference.
- [`CLAUDE.md`](./CLAUDE.md) — Build verification rules and contributor guidance.

---

## Contributing

1. Create a feature branch from `main`.
2. Make focused, well-tested changes — see [`CLAUDE.md`](./CLAUDE.md) for the build verification checklist.
3. Run `dotnet test` and the relevant frontend tests before opening a PR.
4. Submit a pull request with a clear description of the change and any required migration steps.

---

## License

Proprietary. All rights reserved unless otherwise stated.
