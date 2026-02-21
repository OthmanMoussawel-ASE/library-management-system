# Mini Library Management System

A full-stack library management system built with .NET 10, Angular 18, and PostgreSQL. Features include book management, check-in/check-out, AI-powered features (Groq), role-based access control, and more.

## Live Demo

- **Frontend**: [https://library-management-system-chi-fawn.vercel.app](https://library-management-system-chi-fawn.vercel.app)
- **API / Swagger**: [https://library-api-hbnu.onrender.com/swagger](https://library-api-hbnu.onrender.com/swagger)

### Demo Credentials

| Role      | Email                  | Password       |
|-----------|------------------------|----------------|
| Admin     | admin@library.com      | Admin@123!     |
| Librarian | librarian@library.com  | Librarian@123! |

Register a new account to get the Patron role.

## Tech Stack

| Layer          | Technology                                                    |
|----------------|---------------------------------------------------------------|
| Backend        | .NET 10 Web API, EF Core, MediatR (CQRS), FluentValidation, xUnit |
| Frontend       | Angular 18, Angular Material, Standalone Components           |
| Database       | PostgreSQL 16 (Neon.tech)                                     |
| Auth           | ASP.NET Core Identity, JWT                                    |
| AI             | Groq (Llama 3.1 8B) - completely free tier                    |
| Logging        | Serilog (structured, console sink)                            |
| Deployment     | Render (API), Vercel (SPA), Neon.tech (DB), UptimeRobot       |

## Architecture

Clean Architecture + DDD + CQRS with 4 layers:

```
Domain (zero dependencies) → Application (MediatR, DTOs) → Infrastructure (EF Core, Identity, AI) → API (Controllers)
```

### Key Patterns

- **Generic Repository** with Specification pattern for dynamic queries
- **CQRS** via MediatR — every operation (Auth, Books, Authors, Categories, Checkouts, AI, Dashboard) goes through commands/queries with pipeline behaviors (validation, logging, caching)
- **Thin controllers** — controllers only delegate to MediatR, all business logic lives in Application layer handlers
- **Dynamic pagination, sorting, and filtering** via `QueryParameters` + `PagedResult<T>` (all server-side)
- **Domain Events** dispatched on `SaveChanges`
- **Soft deletes** with query filters
- **Cache invalidation** on all CRUD operations

## Features

### Core
- Book CRUD with authors, categories, and metadata
- Author management (CRUD with pagination, sorting, search)
- Category management (CRUD with pagination, sorting, search)
- Check-in (borrow) / Check-out (return) with due dates
- Patrons can return their own books
- Search by title, author, ISBN, description, or category
- Dynamic filtering by author, language, availability
- Paginated, sortable listings for books, authors, categories, and checkouts
- All pagination, sorting, and search handled server-side

### AI Features (Groq - Free)
- **Auto-generate book descriptions** - Creates engaging, creative descriptions from title + author
- **AI-powered book recommendations**:
  - "From Your Library" - Suggests books from your catalog you haven't read
  - "Discover More" - AI suggestions of books not in your catalog
- **Smart search** - Natural language queries converted to structured search (e.g., "science fiction by Asimov" → searches author and genre)
- **AI category suggestions**:
  - Prioritizes matching existing categories in your database
  - Suggests new categories that can be created with one click
  - Shows existing matches and new suggestions separately

### Auth & Security
- JWT authentication (15-min access + 7-day refresh tokens)
- Role-based access (Admin, Librarian, Patron)
- Admin can manage user roles (promote Patron to Librarian, etc.)
- Rate limiting, CORS, security headers
- Hashed refresh tokens (SHA-256)
- FluentValidation on every command/query (Auth, Books, Authors, Categories, Checkouts, AI)

### Dashboard
- Stats: total books, available, authors, checkouts, overdue, patrons
- Role-based visibility (Patrons see only their own stats)
- AI recommendations panel with two sections
- Overdue alerts

### User Management (Admin)
- View all users
- Change user roles (Admin, Librarian, Patron)

## Running Locally

### Prerequisites

- .NET 10 SDK
- Node.js 18+
- Docker (for PostgreSQL) OR a local PostgreSQL instance

### Backend

```bash
# Start PostgreSQL
docker-compose up -d

# Run the API
cd src/LibraryManagement.API
dotnet run
```

The API runs at `http://localhost:5000` with Swagger at `/swagger`.

### Frontend

```bash
cd client
npm install
npx ng serve
```

The app runs at `http://localhost:4200`.

### Configuration

Backend config is in `src/LibraryManagement.API/appsettings.json`. For local development, create `appsettings.Development.json` (git-ignored) with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=librarydb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "YourSecretKeyAtLeast32CharactersLong!!"
  },
  "AI": {
    "Groq": {
      "ApiKey": "YOUR_GROQ_API_KEY",
      "Model": "llama-3.1-8b-instant"
    }
  }
}
```

## Deployment

### 1. Database - Neon.tech

1. Sign up at [neon.tech](https://neon.tech) (no credit card)
2. Create a project and copy the connection string
3. Set as `ConnectionStrings__DefaultConnection` env var on Render

### 2. Backend - Render

1. Sign up at [render.com](https://render.com) (no credit card)
2. Create a new Web Service, connect your GitHub repo
3. Set Docker as the build method, Dockerfile path: `src/LibraryManagement.API/Dockerfile`
4. Add environment variables:
   - `ConnectionStrings__DefaultConnection` = Neon connection string
   - `Jwt__Key` = a random 32+ character string
   - `Jwt__Issuer` = LibraryManagement
   - `Jwt__Audience` = LibraryManagement
   - `Cors__AllowedOrigins__0` = your Vercel URL
   - `AI__Groq__ApiKey` = your Groq API key (optional)
   - `AI__Groq__Model` = llama-3.1-8b-instant (optional, this is the default)

### 3. Frontend - Vercel

1. Sign up at [vercel.com](https://vercel.com) (no credit card)
2. Import the repo, set root directory to `client`
3. Set build command: `npx ng build --configuration production`
4. Set output directory: `dist/client/browser`
5. Update `client/src/environments/environment.prod.ts` with your Render URL before deploying

### 4. Keep-Alive - UptimeRobot

1. Sign up at [uptimerobot.com](https://uptimerobot.com) (no credit card)
2. Add a new HTTP(s) monitor pointing to `https://YOUR_RENDER_URL/healthz`
3. Set interval to 5 minutes
4. This prevents Render from sleeping your free service

### 5. AI - Groq 

1. Go to [Groq Console](https://console.groq.com/keys)
2. Create an API key (completely free, no credit card needed)
3. Add as `AI__Groq__ApiKey` env var on Render
4. Groq offers fast inference with generous free limits

## API Endpoints

| Method | Endpoint                       | Auth              | Description                    |
|--------|--------------------------------|--------------------|-------------------------------|
| POST   | /api/auth/login                | Public             | Login with email/password      |
| POST   | /api/auth/register             | Public             | Register new account           |
| POST   | /api/auth/refresh              | Public             | Refresh access token           |
| GET    | /api/books                     | Public             | List books (paginated)         |
| GET    | /api/books/:id                 | Public             | Get book details               |
| POST   | /api/books                     | Librarian/Admin    | Create book                    |
| PUT    | /api/books/:id                 | Librarian/Admin    | Update book                    |
| DELETE | /api/books/:id                 | Librarian/Admin    | Delete book (soft)             |
| GET    | /api/authors                   | Public             | List authors (paginated)       |
| GET    | /api/authors/all               | Public             | List all authors (for dropdowns)|
| POST   | /api/authors                   | Librarian/Admin    | Create author                  |
| PUT    | /api/authors/:id               | Librarian/Admin    | Update author                  |
| DELETE | /api/authors/:id               | Librarian/Admin    | Delete author                  |
| GET    | /api/categories                | Public             | List categories (paginated)    |
| GET    | /api/categories/all            | Public             | List all categories (for dropdowns)|
| POST   | /api/categories                | Librarian/Admin    | Create category                |
| PUT    | /api/categories/:id            | Librarian/Admin    | Update category                |
| DELETE | /api/categories/:id            | Librarian/Admin    | Delete category                |
| GET    | /api/checkouts                 | Authenticated      | List checkouts (paginated)     |
| POST   | /api/checkouts                 | Authenticated      | Checkout a book                |
| POST   | /api/checkouts/:id/return      | Authenticated      | Return a book                  |
| GET    | /api/checkouts/overdue         | Librarian/Admin    | List overdue checkouts         |
| GET    | /api/dashboard/stats           | Authenticated      | Dashboard statistics           |
| GET    | /api/ai/recommendations        | Authenticated      | AI book recommendations        |
| POST   | /api/ai/smart-search           | Authenticated      | AI smart search                |
| POST   | /api/ai/generate-description   | Librarian/Admin    | AI generate book description   |
| POST   | /api/ai/categorize             | Librarian/Admin    | AI suggest categories          |
| GET    | /api/ai/status                 | Public             | Check if AI is available       |
| GET    | /api/users                     | Admin              | List users                     |
| PUT    | /api/users/:id/role            | Admin              | Assign user role               |
| GET    | /healthz                       | Public             | Health check                   |

## Project Structure

```
├── src/
│   ├── LibraryManagement.Domain/          # Entities, Value Objects, Interfaces, Specifications
│   ├── LibraryManagement.Application/     # CQRS Commands/Queries, DTOs, Validators, Behaviors
│   ├── LibraryManagement.Infrastructure/  # EF Core, Repositories, Identity, AI Service, Cache
│   └── LibraryManagement.API/             # Thin Controllers, Middleware, Program.cs, Dockerfile
├── tests/
│   └── LibraryManagement.Tests/           # xUnit + Moq + FluentAssertions (Book CRUD tests)
├── client/                                # Angular 18 SPA
│   ├── src/app/
│   │   ├── core/                          # Services, Guards, Interceptors, Models
│   │   └── features/                      # Auth, Books, Authors, Categories, Checkouts, Dashboard, Admin
│   └── vercel.json                        # Vercel deployment config
├── docker-compose.yml                     # Local PostgreSQL
└── README.md
```


### Testing
- Added xUnit test project with Moq and FluentAssertions
- Book CRUD operations covered with 15 unit tests using EF Core InMemory provider
