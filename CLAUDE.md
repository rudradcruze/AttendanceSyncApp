# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AttendanceSyncApp is an ASP.NET MVC 5 web application (.NET Framework 4.5) that manages attendance synchronization records. It provides a UI for creating synchronization requests between date ranges and tracks their processing status.

## Technology Stack (REQUIRED)

- **ASP.NET MVC 5** - Web framework
- **Entity Framework 6** - ORM for data access
- **jQuery** - Client-side scripting
- **AJAX** - Asynchronous server communication
- **Microsoft SQL Server** - Database
- **Frontend** - Bootstrap v5.3.8

**Reference Documentation:** https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/introduction/getting-started

## Architecture Rules (MUST FOLLOW)

All code must follow this layered architecture:

```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│    (Controllers - Thin Layer)       │
│  - Handle HTTP requests/responses   │
│  - Model binding & validation       │
│  - Return views/JSON                │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Service Layer               │
│   (Business Logic & Orchestration)  │
│  - Validation & business rules      │
│  - Transaction management           │
│  - Coordinate multiple repositories │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│       Repository Layer              │
│      (Data Access Logic)            │
│  - CRUD operations                  │
│  - Query building                   │
│  - EF context interaction           │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│         Database                    │
└─────────────────────────────────────┘
```

### Layer Responsibilities

**Presentation Layer (Controllers/)**
- Keep controllers THIN - no business logic
- Only handle HTTP request/response
- Call services for all business operations
- Return Views or JSON results

**Service Layer (Services/)**
- All business logic lives here
- Validate business rules
- Manage transactions
- Coordinate between multiple repositories
- Return DTOs/ViewModels to controllers

**Repository Layer (Repositories/)**
- Data access only - no business logic
- CRUD operations
- Query building with LINQ
- Direct EF DbContext interaction
- Use Unit of Work pattern

**Models Layer (Models/)**
- Entity classes mapped to database tables
- DbContext configuration
- DTOs and ViewModels

## Folder Structure

```
AttendanceSyncApp/
├── Controllers/          # Thin controllers (Presentation Layer)
├── Services/             # Business logic (Service Layer)
│   └── Interfaces/       # Service interfaces
├── Repositories/         # Data access (Repository Layer)
│   └── Interfaces/       # Repository interfaces
├── Models/               # Entities, DTOs, ViewModels
├── Views/                # Razor views
├── Scripts/              # JavaScript (jQuery, AJAX)
├── Content/              # CSS, images
└── App_Start/            # Route configuration
```

## Build Commands

```bash
# Restore NuGet packages
nuget restore AttendanceSyncApp.sln

# Build solution
msbuild AttendanceSyncApp.sln /p:Configuration=Debug

# Build release
msbuild AttendanceSyncApp.sln /p:Configuration=Release
```

The application runs on IIS Express (HTTPS port 44340) during development.

## Database

Uses SQL Server with Entity Framework 6. Connection string `DefaultConnection` is configured in Web.config. Tables:
- `Companies` - Company master table
- `AttandanceSynchronizations` - Sync request records

## Status Workflow

Synchronization records use these status codes:
- `NR` - New Request (initial state when created)
- `IP` - In Progress
- `CP` - Completed

## Key Patterns

- Use `AsNoTracking()` for read-only queries in repositories
- Use `JsonRequestBehavior.AllowGet` for GET endpoints returning JSON
- Use `DateTime.TryParse` for date validation in services
- Frontend polls status every 2 seconds via AJAX

## Naming Convention

The codebase uses "Attandance" spelling throughout (controller, models, views, scripts). Maintain this spelling for consistency.
