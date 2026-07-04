# MiSTerLauncher.Server — Contexte

Backend ASP.NET Core 8.0. Sert l'API REST, le hub SignalR, l'auth JWT, et les fichiers statiques Angular (single-unit).
Dépend de `libMisterLauncher` (le `MisterManager` et tous les modules métier). Aucune logique métier ici — les contrôleurs délèguent tout à `MisterManager`.

## Structure

```
MiSTerLauncher.Server/
├── Controllers/
│   ├── AuthController.cs         # POST /api/auth/login, requestguestaccess, guestaccess*
│   ├── CoreController.cs         # /api/core — savestates, module settings, Home Assistant switch, jobs, scripts
│   ├── HealthcheckController.cs  # GET /api/healthcheck
│   ├── MediaController.cs        # GET /api/media/{id}?token= — sert le binaire média
│   ├── RomController.cs          # /api/rom — unmatch, détail, matching manuel, scan, delete
│   ├── SystemController.cs       # /api/system — recherche, détail, settings, scan
│   └── VideoGameController.cs    # /api/videogame — recherche, lancement, liaison rom, settings
├── HostedService/MisterBackgroundTask.cs  # IHostedService — cycle de vie du MisterManager
├── Hub/MisterHub.cs              # Hub SignalR /hub/misterhub
├── Program.cs
├── appsettings.json               # Jwt:Issuer / Jwt:Audience (Jwt:Key généré à part)
└── data/jwt-key.json              # clé JWT auto-générée au premier démarrage (ne pas committer)
```

`Models/GameModel.cs` et `WeatherForecast.cs` sont des restes du template par défaut — ne pas les prendre pour du code actif, ne pas les étendre.

## Pattern controller observé

```csharp
[ApiController]
[Route("api/[controller]")]
public class XxxController : ControllerBase
{
    private MisterManager _misterManager;

    public XxxController(MisterBackgroundTask hostedService)
    {
        hostedService.Wakeup();          // sort le background service du mode sommeil
        _misterManager = hostedService.manager;
    }
    // ...
}
```

Contrairement à un pattern `[Authorize]` explicite sur chaque contrôleur, l'auth est imposée **globalement** dans `Program.cs` via `app.MapControllers().RequireAuthorization()`. Les contrôleurs/actions publiques doivent donc explicitement s'exclure avec `[AllowAnonymous]` (voir `AuthController.Login`, `LoginWithoutAuthentication`, `RequestGuestAccess`, etc.).

**Autorisation par rôle** : au-dessus de l'auth globale, certaines actions sensibles ajoutent `[Authorize(Roles = "admin")]` (ex. `CoreController.SetModuleSettings`, `GetCurrentJob`, `ExcecuteScript`, `SetHaSwitch`). Les endpoints sans cet attribut sont accessibles à tout utilisateur authentifié, y compris le rôle `guest`. Vérifier ce détail avant d'ajouter une nouvelle action sur une ressource sensible (settings, scripts, save states en écriture).

Chaque constructeur de contrôleur appelle `hostedService.Wakeup()` avant de lire `hostedService.manager` — c'est ce qui sort `MisterBackgroundTask` du mode sommeil sur n'importe quel appel API.

## Auth JWT

- Clé générée au premier démarrage dans `Program.cs` : si `Jwt:Key` est vide en config, génère 32 octets aléatoires, les écrit dans `data/jwt-key.json`, puis les injecte dans la config au runtime (fichier ajouté via `builder.Configuration.AddJsonFile("data/jwt-key.json", optional: true)`).
- `Jwt:Issuer`/`Jwt:Audience` dans `appsettings.json` (les deux valent `"MisterLauncher"`).
- Deux rôles : `admin` (mot de passe vérifié par `MisterManager.CheckAdminPassword`) et `guest` (flux de demande/approbation via `AuthController.RequestGuestAccess` → `GuestAccessState` → `GuestAccessConsume`, approuvé par un admin via `GuestAccessAction`).
- `GET /api/auth/loginwithoutauthentication` — particularité dev/no-auth : ne fonctionne que si `_misterManager.AuthorisationIsrequired()` est `false` (pas de mot de passe admin configuré), retourne alors directement un token admin. À garder en tête, ce n'est pas un bypass permanent.
- Durée du token dépendante du rôle : `_misterManager.GetTokenDelay(role)`.

## Routes API (par contrôleur)

| Contrôleur | Exemples de routes | Rôle |
|---|---|---|
| `AuthController` | `POST login`, `GET loginwithoutauthentication`, `POST requestguestaccess`, `guestaccessstate`, `guestaccessconsumed` | public (`AllowAnonymous`) |
| `AuthController` | `GET guestaccesscurrent`, `POST guestaccessaction` | `admin` |
| `CoreController` | `POST keyboardcmd`, `getallsavetates`, `savestatecmdsave`/`load`, `GET haswitch` | authentifié |
| `CoreController` | `getmodulesettings`/`setmodulesettings`/`checkmodulesettings`, `POST haswitch`, `automaticmatchrom`, `GET currentjob`, `scripts`, `executescript` | `admin` |
| `HealthcheckController` | `GET api/healthcheck` → `MisterManagerCache` | authentifié |
| `MediaController` | `GET api/media/{id}?token=` — stream du binaire média | authentifié (token en query, pas en header — utilisé pour les balises `<img>`/`<video>`) |
| `RomController` | `unmatchroms`, `id`, `linkromtoscrappervideogame`, `launchjobscanrom`, `delete` | authentifié |
| `SystemController` | `search`, `id`, `updatesettings`, `countfilter`, `scan` | authentifié |
| `VideoGameController` | `search`, `id`, `launch`, `playing`, `playlist`, `unlinkrom`/`linkrom`/`setprimaryrom`, `delete`, `updatesettings`, `searchvideogamefromscrapper`, `searchvideogamefromromid` | authentifié |

## `MisterBackgroundTask` (`HostedService/MisterBackgroundTask.cs`)

`IHostedService, IDisposable`, enregistré **à la fois** en singleton et en hosted service pointant vers la même instance (`AddSingleton<MisterBackgroundTask>()` + `AddHostedService<MisterBackgroundTask>(provider => provider.GetService<MisterBackgroundTask>())`) — ce qui permet aux contrôleurs et au hub d'injecter directement le singleton pour appeler `Wakeup()`/lire `.manager`.

- `StartAsync` : lit `GDB_MONGO_CNX`/`GDB_MONGO_DBNAME`, câble `manager.OnCacheUpdated`/`OnJobRomUpdate` vers `IHubContext<MisterHub, IMisterHubClient>`, appelle `manager.Initialize(...)`, démarre un `Timer` toutes les 30s (`DoWork`)
- `DoWork` : rafraîchit le cache et pousse `RefreshCache` en SignalR, sauf en mode sommeil
- Sommeil après 20 minutes sans appel à `Wakeup()`
- `Wakeup()` : réinitialise le timer d'inactivité ; si on sort du sommeil, appelle `ForceRefresh()` → `manager.checkModuleList()` (recrée les modules manquants) → relance `DoWork`

Voir [libMisterLauncher/CLAUDE.md](../libMisterLauncher/CLAUDE.md) pour le détail de `MisterManager`, des modules et du système de jobs (`JobMister`).

## Hub SignalR (`Hub/MisterHub.cs`)

`MisterHub : Hub<IMisterHubClient>`, injecte le singleton `MisterBackgroundTask`.
- `OnConnectedAsync` — pousse immédiatement le cache courant au client qui vient de se connecter
- Méthode hub `RefreshCache()` — un client peut la déclencher pour forcer une diffusion à tous
- `IMisterHubClient` : `JobRomScanRefresh(JobMister)`, `RefreshCache(MisterManagerCache)`, `SendMessage(string)`

## Convention JSON

`JsonStringEnumConverter` est ajouté globalement (Minimal API JSON options **et** MVC JSON options dans `Program.cs`) — tous les enums (`JobState`, `JobType`, `MisterStateEnum`, ...) sérialisent en string côté API/SignalR, jamais en valeur numérique.

## Pipeline HTTP (`Program.cs`)

Ordre important : `UseDefaultFiles`/`UseStaticFiles` (sert le build Angular) → CORS permissif (`AllowAnyMethod/Header`, `SetIsOriginAllowed(_ => true)`, `AllowCredentials`) → Swagger (Development seulement) → `UseHttpsRedirection` → `UseAuthentication`/`UseAuthorization` → `MapControllers().RequireAuthorization()` → `MapFallbackToFile("/index.html")` (fallback SPA) → `MapHub<MisterHub>("/hub/misterhub")` (WebSockets + LongPolling).
