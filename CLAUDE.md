# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MiSTerLauncher is a web application to search and launch games on a [MiSTer FPGA](https://mister-devel.github.io/MkDocs_MiSTer/) device. It integrates with ScreenScraper.fr for game metadata, MongoDB for storage, MiSTer Remote API for game launching, and FTP for SD card access.

## Solution Structure

```
MiSTerLauncher.sln
├── MiSTerLauncher.Server/   # ASP.NET Core 8.0 Web API + SPA host
├── libMisterLauncher/       # Class library: all business logic and modules
└── misterlauncher.client/   # Angular 18 frontend (CoreUI)
```

## Build & Run Commands

**Backend (from repo root):**
```bash
dotnet build MiSTerLauncher.sln
dotnet run --project MiSTerLauncher.Server
```

**Frontend (standalone dev):**
```bash
cd misterlauncher.client
npm install
npm start        # starts Angular dev server with HTTPS proxy to backend
npm run build    # production build
ng test          # run Karma/Jasmine unit tests
```

**Full-stack dev**: Running `dotnet run --project MiSTerLauncher.Server` launches the SPA proxy automatically (via `SpaProxyLaunchCommand=npm start`), serving Angular at `https://localhost:4200` and the API on the .NET port.

**Required environment variable:**
- `GDB_MONGO_CNX` — MongoDB connection string (e.g. `mongodb://localhost`)
- `GDB_MONGO_DBNAME` — optional, database name override

## Architecture

### Backend Module System (`libMisterLauncher/`)

All external integrations are **modules** implementing `IMisterModule` / `IMisterSettings` (defined in `libMisterLauncher/IMisterLancher.cs`). Modules use `BaseModule<T>` as base class with health-check support.

**Modules:**
| Module | Purpose |
|---|---|
| `GamedbService` | MongoDB data layer — the primary persistence layer |
| `MisterRemoteService` | Calls MiSTer Remote HTTP API to launch games, get current game, run scripts |
| `MisterFtpService` | FTP connection to MiSTer SD card to scan ROM files |
| `MisterMediaService` | Downloads and stores game media (fanart, screenshots, videos) |
| `ScrapperScService` | Calls ScreenScraper.fr API to match ROMs to game metadata |
| `MisterAuthService` | Manages admin/guest access with JWT tokens |

**Module settings are stored in MongoDB** (not in appsettings.json). Only the MongoDB connection itself is configured via env var.

### Central Orchestrator: `MisterManager` (`libMisterLauncher/MisterManager.cs`)

`MisterManager` owns all module instances in a type-keyed dictionary. It provides the full business logic surface:
- `Initialize(GameDbSettings)` → connects to MongoDB, then loads all other module settings from DB via `LoadModules()`
- `ScanRom()` / `AutomaticScanRom()` — FTP-based ROM discovery jobs
- `LinkRomToVideoGame()` / `AutomaticLinkRomToVideoGame()` — ScreenScraper matching jobs
- `LaunchVideoGame()` — sends launch command via MiSTer Remote
- Job progress is reported via `OnJobRomUpdate` event; cache updates via `OnCacheUpdated` event

### Background Service & SignalR (`MiSTerLauncher.Server/`)

`MisterBackgroundTask` (IHostedService) holds the singleton `MisterManager` instance. It:
- Polls every 30 seconds to refresh the cache and broadcast via SignalR
- Enters sleep mode after 20 minutes of inactivity; wakes on any API call
- Each controller receives `MisterBackgroundTask` via DI and calls `hostedService.Wakeup()`

`MisterHub` is the SignalR hub at `/hub/misterhub`. Clients receive:
- `RefreshCache(MisterManagerCache)` — updated game stats, health, playing game
- `JobRomScanRefresh(JobMister)` — real-time progress of scan/matching jobs

### Frontend (`misterlauncher.client/`)

Angular 18 with CoreUI 5. Key services:
- `mister-signalr.service.ts` — SignalR client, maintains live connection to hub
- `auth.service.ts` — JWT auth with `@auth0/angular-jwt`
- Models mirror the C# entities from `libMisterLauncher/Entity/`

### Authentication

JWT Bearer auth is required on all controllers (`MapControllers().RequireAuthorization()`). JWT configuration (key, issuer, audience) is in `appsettings.json`. The `MisterAuth` module manages the admin password and guest access flow stored in MongoDB.

### Data Model Key Concepts

- **Rom** — a file on the MiSTer SD card (identified by path + hash)
- **VideoGameDb** — a game entry with metadata from ScreenScraper, linked to one or more Roms
- **SystemDb** — a gaming platform (Console or Arcade), matched between MiSTer cores and ScreenScraper systems
- **MediaDb** — media reference (stored path + source URL), downloaded on demand
- **MisterManagerCache** — in-memory snapshot broadcast to all clients via SignalR
