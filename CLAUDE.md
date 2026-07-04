# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MiSTerLauncher is a web application to search and launch games on a [MiSTer FPGA](https://mister-devel.github.io/MkDocs_MiSTer/) device. It integrates with ScreenScraper.fr for game metadata, MongoDB for storage, MiSTer Remote API for game launching, FTP for SD card access, and Home Assistant for MiSTer power control.

## Architecture — 3 projets

```
MiSTerLauncher.sln
├── libMisterLauncher/       # CORE métier : modules (Mongo, FTP, MiSTer Remote, ScreenScraper, Auth, Home Assistant), MisterManager
├── MiSTerLauncher.Server/   # ASP.NET Core 8.0 — API REST + Hub SignalR + Auth JWT + host SPA
└── misterlauncher.client/   # Angular 18 (CoreUI) — frontend
```

**Dépendances entre projets :**
- `MiSTerLauncher.Server` → `libMisterLauncher`
- `misterlauncher.client` consomme l'API et le hub SignalR de `MiSTerLauncher.Server` (via proxy en dev)
- `libMisterLauncher` n'a aucune dépendance vers les deux autres projets

Chaque projet a son propre `CLAUDE.md` avec le détail (`libMisterLauncher/CLAUDE.md`, `MiSTerLauncher.Server/CLAUDE.md`, `misterlauncher.client/CLAUDE.md`) — ce fichier racine ne couvre que la vue d'ensemble.

## Stack technique

| Couche | Choix |
|---|---|
| Backend | ASP.NET Core 8.0, C# (libMisterLauncher en `net6.0`, Server en `net8.0` — TFM mixte) |
| Frontend | Angular 18, CoreUI 5 (Free) |
| Temps réel | SignalR (`/hub/misterhub`) |
| Auth | JWT (clé auto-générée dans `MiSTerLauncher.Server/data/jwt-key.json`) |
| Base de données | MongoDB (`MongoDB.Driver`, schemaless — pas de migrations) |
| Métadonnées jeux | ScreenScraper.fr API |
| Lancement de jeux | MiSTer Remote API (HTTP + WebSocket) |
| Accès SD card | FTP (FluentFTP) |
| Domotique | Home Assistant REST API (switch power MiSTer) |

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

## Variables d'environnement

| Variable | Requis | Description |
|---|---|---|
| `GDB_MONGO_CNX` | Oui | Chaîne de connexion MongoDB (ex. `mongodb://localhost`) |
| `GDB_MONGO_DBNAME` | Non | Nom de la base, surcharge le défaut |

## Architecture — vue d'ensemble

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
| `HomeAssistantService` | Calls Home Assistant REST API to read/control a switch entity (MiSTer power) |

**Module settings are stored in MongoDB** (not in appsettings.json). Only the MongoDB connection itself is configured via env var.

Détail complet (orchestrateur `MisterManager`, système de jobs, entités) : voir [libMisterLauncher/CLAUDE.md](libMisterLauncher/CLAUDE.md).

### Background Service & SignalR (`MiSTerLauncher.Server/`)

`MisterBackgroundTask` (IHostedService) holds the singleton `MisterManager` instance. It:
- Polls every 30 seconds to refresh the cache and broadcast via SignalR
- Enters sleep mode after 20 minutes of inactivity; wakes on any API call
- Each controller receives `MisterBackgroundTask` via DI and calls `hostedService.Wakeup()`

`MisterHub` is the SignalR hub at `/hub/misterhub`. Clients receive:
- `RefreshCache(MisterManagerCache)` — updated game stats, health, playing game
- `JobRomScanRefresh(JobMister)` — real-time progress of scan/matching jobs

Détail complet (contrôleurs, routes, auth JWT) : voir [MiSTerLauncher.Server/CLAUDE.md](MiSTerLauncher.Server/CLAUDE.md).

### Frontend (`misterlauncher.client/`)

Angular 18 with CoreUI 5. Key services:
- `mister-signalr.service.ts` — SignalR client, maintains live connection to hub
- `auth.service.ts` — JWT auth with `@auth0/angular-jwt`
- Models mirror the C# entities from `libMisterLauncher/Entity/`

Détail complet (structure, conventions Angular réelles, routes) : voir [misterlauncher.client/CLAUDE.md](misterlauncher.client/CLAUDE.md).

### Authentication

JWT Bearer auth is required on all controllers (`MapControllers().RequireAuthorization()`). JWT configuration (key, issuer, audience) is in `appsettings.json`. The `MisterAuth` module manages the admin password and guest access flow stored in MongoDB.

### Data Model Key Concepts

- **Rom** — a file on the MiSTer SD card (identified by path + hash)
- **VideoGameDb** — a game entry with metadata from ScreenScraper, linked to one or more Roms
- **SystemDb** — a gaming platform (Console or Arcade), matched between MiSTer cores and ScreenScraper systems
- **MediaDb** — media reference (stored path + source URL), downloaded on demand
- **MisterManagerCache** — in-memory snapshot broadcast to all clients via SignalR
- **HaSwitchState** — DTO returned by `GET /api/core/haswitch` with `state` ("on"/"off") and `lastChanged` date

## Conventions transversales

- Code (classes, méthodes, variables) : **anglais**. Documentation et commentaires : **français** quand c'est pertinent (cohérent avec le reste de ce fichier).
- Pas de secrets en dur dans le code — connexion Mongo via variable d'environnement, clé JWT auto-générée au premier démarrage dans `MiSTerLauncher.Server/data/jwt-key.json` (à ne pas committer).
- Les settings de chaque module métier vivent en base Mongo, jamais dans `appsettings.json`.

### Notes techniques importantes

- **CoreUI `IconSetService`** : singleton initialisé dans `AppComponent`. Ne pas ajouter `providers: [IconSetService]` dans les composants enfants — cela crée une instance isolée sans icônes.
- **`http.get<string>()`** : préférer retourner un objet JSON `{ state }` plutôt qu'une chaîne brute pour éviter les erreurs de parsing Angular HttpClient.
- **`CheckConnection()` dans les modules** : utilise `.Result` (synchrone). Fonctionne en ASP.NET Core (pas de SynchronizationContext), mais attention dans d'autres contextes async.
