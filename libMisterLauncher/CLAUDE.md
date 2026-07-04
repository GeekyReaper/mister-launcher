# libMisterLauncher — Contexte

Cœur métier de MiSTerLauncher. Contient tous les modules d'intégration externe, l'orchestrateur `MisterManager` et les entités du domaine.
Aucune dépendance vers `MiSTerLauncher.Server` ni `misterlauncher.client`.

Cible `net6.0` — à la différence de `MiSTerLauncher.Server` qui est en `net8.0`. TFM mixte à garder en tête pour toute évolution de dépendances partagées.

## Structure

```
libMisterLauncher/
├── IMisterLancher.cs        # Interfaces IMisterSettings/IMisterModule, BaseModule<T>, Tools, Cast
├── MisterManager.cs         # Orchestrateur principal (~2500 lignes)
├── GamedbService.cs         # Module — persistance MongoDB
├── MisterRemoteService.cs   # Module — API MiSTer Remote (lancement de jeux)
├── MisterFtpService.cs      # Module — FTP (scan ROMs sur la carte SD)
├── MisterMediaService.cs    # Module — téléchargement/stockage des médias
├── ScrapperScService.cs     # Module — API ScreenScraper.fr
├── MisterAuth.cs            # Module — MisterAuthService (JWT admin/guest)
├── HomeAssistantService.cs  # Module — API Home Assistant (switch power)
└── Entity/                  # DTOs/modèles (Rom, VideoGame, SystemDb, ModuleSetting, JobMister, ...)
```

## Pattern module : `IMisterSettings` / `IMisterModule` / `BaseModule<T>`

Toute intégration externe suit le même contrat (`IMisterLancher.cs`) :
- `IMisterSettings` — settings d'un module (`ModuleName`, `RefreshConnection`, `isValid()`, chargés/sauvegardés via `GetModuleSettings()`/`LoadModuleSettings()`)
- `IMisterModule` — `CheckConnection()`, `CheckHealth()`, `LoadSettings(IMisterSettings)`
- `BaseModule<T>` — classe de base générique implémentant `IMisterModule`

**Cache de health-check** (`BaseModule<T>.CheckHealth()`, voir [IMisterLancher.cs:151-180](IMisterLancher.cs#L151-L180)) : si une connexion a réussi il y a moins de `RefreshConnection` (settings du module), `CheckHealth()` retourne l'état en cache (`Message = "cache"`) sans réellement retester la connexion. Ne pas s'étonner qu'un `CheckConnection()` ne soit pas appelé à chaque healthcheck — c'est voulu, pour éviter de spammer les API externes.

**`CheckConnection()` est synchrone** (`.Result` sur une tâche async) dans plusieurs modules — fonctionne en ASP.NET Core (pas de `SynchronizationContext`), mais à éviter comme modèle dans d'autres contextes (ex. tests, apps desktop).

## Modules

| Classe | Fichier | Rôle | Méthodes clés |
|---|---|---|---|
| `GamedbService` | GamedbService.cs | Couche de persistance MongoDB — roms, jeux, systèmes, médias, settings, stats | `GetVideoGame`, `GetMatchVideoGame`, `UpdateVideogameRoms`, `InsertVideogameFull`, `SearchVideoGameByRomId`, `GetRom`, `UpdateRom`, `UpdateMatchRom`, `GetAllSystems`, `GetSystemDb`, `UpdateSystemDbFull`, `GetStats`, `GetModuleSettings`/`SetModuleSettings` |
| `MisterRemoteService` | MisterRemoteService.cs | Appelle l'API MiSTer Remote (HTTP + WebSocket) pour lancer un jeu, lire le jeu courant, exécuter des scripts, gérer les save states | `LaunchGame`, `CurrentGame`, `GetSystems`, `GetScript`/`ExecuteScript`, `CmdSaveState`/`CmdLoadState`, `GetAllScreenshots`/`TakeScreenshot`, `ConnectAndListenToRemoteWebSocket`, event `OnGamePlaying` |
| `MisterFtpService` | MisterFtpService.cs | Connexion FTP (FluentFTP) à la carte SD MiSTer pour lister les fichiers ROM | `GetAvailableRomPath`, `BuildArcadeRom`, `GetArcadeRoms`, `GetFile`, `CreateConnection`, `ScanArcadeDirectory`/`ScanConsoleDirectory` |
| `MisterMediaService` | MisterMediaService.cs | Télécharge et stocke les médias (fanart, screenshots, vidéos, manuels) sur disque | `AddBinaryFile`, `CompressRepository`, `CreateDir`, `DeleteFile` |
| `ScrapperScService` | ScrapperScService.cs | Appelle l'API ScreenScraper.fr pour matcher des ROMs/noms à des métadonnées de jeu | `GetVideoGameFromRom`, `GetVideoGameFromId`, `GetVideoGameFromSearchName`, `GetSystems`, `GetAllArcadeSystemJsonMatch`, `GetConsoleSystemJsonMatch`, `GetGameType` |
| `MisterAuthService` | MisterAuth.cs | Gère l'accès admin/guest (hash de mot de passe, flux de demande/approbation d'accès invité) | `HashPassword` (static), `GetCurrentGuestAccess`, `GetGuestAccess`, `GuestAccessState`, `GuessAccessAdminApprouve`, `GetTokenDelay` |
| `HomeAssistantService` | HomeAssistantService.cs | Lit/contrôle un switch Home Assistant (alimentation MiSTer) | `GetSwitchState`, `TurnOn`, `TurnOff`, `Toggle` |

Chaque settings de module est chargé/sauvegardé via `GamedbService.GetModuleSettings`/`SetModuleSettings` — jamais dans `appsettings.json`.

## `MisterManager` — orchestrateur (`MisterManager.cs`)

Namespace `libMisterLauncher.Manager`. Détient tous les modules dans un `Dictionary<Type, IMisterModule> _modules`, avec des getters typés paresseux (`gamedbService`, `misterremoteService`, `misterftpService`, `mistermediaService`, `scrapperService`, `authService`, `homeAssistantService`) qui déclenchent `LoadModules()` si le module n'est pas encore chargé.

Deux events : `OnCacheUpdated` et `OnJobRomUpdate(JobMister)`.

- `Initialize(GameDbSettings)` — connecte Mongo, puis `LoadModules()` charge les settings de chaque module depuis la base (crée les défauts si absents)
- `checkModuleList()` — recrée toute instance de module manquante (utilisé par `MisterBackgroundTask.ForceRefresh()` au réveil du serveur)
- `RefreshCache()` — relance les health-checks + stats/systèmes dans `_cache` (`MisterManagerCache`)
- Pipeline de matching : `LinkRomToVideogameId`, `LinkRomToConsoleVideogame`, `LinkRomToArcadeVideogame`, `LinkRomToVideoGame` (job batch), `AutomaticLinkRomToVideoGame`
- Pipeline de scan ROM : `ScanRom`, `AutomaticScanRom`, `ScanSystems`/`AutomaticScanSystems`
- Lancement de jeu : `LaunchVideoGame(videoGameId, romId)` → résout rom + système → `misterremoteService.LaunchGame(...)`
- Suivi de partie en cours : `CurrentVideogame()`, souscrit à `MisterremoteService_OnGamePlaying` pour incrémenter le compteur de parties et pousser une mise à jour de cache
- Médias : `InsertMedia`, `GetMedia`, `DeleteMediaWithFile`

## Système de jobs (`JobMister`)

Plus simple que les systèmes de jobs génériques d'autres projets (pas d'`AsyncLocal`, pas de progression pourcentage) — voir [Entity/GamedbServiceEntities.cs:22](Entity/GamedbServiceEntities.cs#L22) :

```csharp
public class JobMister
{
    public string jobName { get; set; }
    public List<JobLog> logs { get; set; }   // historique borné à 10 entrées (AddLog)
    public JobType jobType { get; set; }      // SCANROM | MATCHINGROM | SCANSYSTEM | UNDEFINNED
    public JobState state { get; set; }       // RUNNING | DONE | CANCEL
    public int foldersScanned { get; set; }
    public int foldersRemaining { get; set; }
    public GbdRomUpdateResult result { get; set; }
}
```

Chaque pipeline long (`ScanRom`, `AutomaticScanRom`, `LinkRomToVideoGame`, `AutomaticLinkRomToVideoGame`, `ScanSystems`) maintient un `JobMister` et notifie sa progression via l'event `OnJobRomUpdate` de `MisterManager`. C'est `MisterBackgroundTask` (côté `MiSTerLauncher.Server`) qui relaie cet event en SignalR (`JobRomScanRefresh`) — voir [MiSTerLauncher.Server/CLAUDE.md](../MiSTerLauncher.Server/CLAUDE.md).

## Persistance : MongoDB

`GamedbService` est la seule couche de persistance (`MongoDB.Driver`). MongoDB étant schemaless, il n'y a **pas de système de migration** à maintenir — un nouveau champ sur une entité C# est simplement absent (valeur par défaut) sur les documents existants tant qu'un patch de données n'a pas été exécuté. `GamedbService` contient plusieurs méthodes `Patch*` pour ce type de migration de données ponctuelle.

## Entités clés (`Entity/`)

- **`Rom`** — fichier détecté sur la carte SD. Champs notables : `_id` (slug généré via `SetId()`), `fullpath`, `systemid`, `core`, `checksum_md5`/`checksum_crc`, `isMatch`, `mraInfo` (métadonnées MRA pour l'arcade — `MraInfo` avec `setname`, `rbf`, `rotation`, etc.). `SetFileName()` dérive `name`/`extension`/`fullname` depuis le chemin FTP.
- **`VideoGame`/`VideoGameDb`** — jeu avec métadonnées ScreenScraper, lié à une ou plusieurs Rom.
- **`SystemDb`** — plateforme de jeu (Console ou Arcade), matchée entre les cores MiSTer et les systèmes ScreenScraper.
- **`MediaDb`** — référence média (chemin stocké + URL source), téléchargée à la demande.
- **`ModuleSetting`** — paramètre de module stocké en Mongo : `_id` (slug `moduleName-name`), `valueType`, `value`, `description`. C'est le mécanisme générique utilisé par `GetModuleSettings`/`SetModuleSettings`/`LoadModuleSettings` pour chaque module (pas de config typée par module dans `appsettings.json`).

## Point d'attention : imports morts

Plusieurs fichiers (`MisterManager.cs`, `Entity/Rom.cs`, ...) ont des `using Amazon.Runtime...` en tête de fichier alors qu'aucun package AWS n'est référencé dans le `.csproj`. Ce sont des imports résiduels sans effet — ne pas les prendre pour un signe d'intégration AWS existante.
