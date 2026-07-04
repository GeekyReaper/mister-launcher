# misterlauncher.client — Contexte

Frontend Angular 18 avec CoreUI 5 (Free). SPA servie par `MiSTerLauncher.Server` en production (fichiers statiques + fallback `/index.html`).
En dev, tourne sur `https://localhost:4201` avec proxy HTTPS vers le backend (`npm start` → `aspnetcore-https` génère/lit le certificat dans `%APPDATA%\ASP.NET\https`, puis `ng serve --ssl`, port fixé via `angular.json` → `serve.options.port`). Build de prod → `dist/misterlauncher.client/browser/`.

> Ce projet n'est **pas** un frontend Angular "moderne" strict façon standalone/signals-only — voir la section Conventions ci-dessous pour l'état réel observé dans le code.

## Structure réelle

```
src/app/
├── app.module.ts / app-routing.module.ts   # restes NgModule (legacy)
├── app.config.ts / app.routes.ts           # config standalone actuelle (utilisée réellement)
├── app.component.ts                        # bootstrap IconSetService (singleton)
├── auth.guard.ts                            # authGuard, utilisé sur presque toutes les routes
├── components/                              # composants partagés réutilisables
│   ├── filter-videogame, itemlist-system, itemlist-videogame, list-roms
│   ├── modal-confirmation, modaledit-system, modaledit-videogame
│   ├── navbar, footer, sidebar, pagination-videogame
│   ├── part-videogame-categories, part-videogame-launchbutton, select-rom, view-job
├── layout/ + layouts/                       # coquille CoreUI (DefaultLayoutComponent, header, footer)
├── views/                                    # pages routées, chacune avec son propre routes.ts (lazy loading)
│   ├── videogames/, systems/, mister-remote/, guest-access/, mister-script/, mister-settings/, job-scan/
│   ├── pages/ (login, register, 404, 500, apierror)
│   └── base/, buttons/, charts/, dashboard/, forms/, icons/, notifications/, theme/, widgets/, test/, games/  # scaffolding CoreUI d'origine, en grande partie inutilisé
├── services/                                 # voir tableau ci-dessous
│   └── models/                               # interfaces TS miroir des entités C# de libMisterLauncher/Entity/
├── pipe/                                      # mediaurl, filesize, timeofday, timelapse, misterdate, misterstate-color, jobstate-color
└── icons/icon-subset.ts                      # registre central des icônes CoreUI
```

Le dossier `documentation/` à la racine du client est un bundle statique de démo CoreUI/Material Dashboard — sans rapport avec l'app réelle, à ignorer.

## Conventions Angular — état réel du code (pas de règle aspirationnelle)

- **Standalone** : la grande majorité des composants sont `standalone: true` (79 fichiers), mais `app.module.ts`/`app-routing.module.ts` legacy coexistent encore à côté de `app.config.ts`/`app.routes.ts` — ne pas supposer que le projet est 100% standalone.
- **Templates** : la syntaxe `*ngIf`/`*ngFor` classique est **très largement majoritaire** (256 occurrences dans 28 fichiers) ; `@if`/`@for` est quasi absent (4 occurrences dans 3 fichiers seulement). Continuer avec `*ngIf`/`*ngFor` par cohérence avec le reste du code, sauf demande explicite de migration.
- **State** : géré par des services avec `BehaviorSubject`/`Observable` (`MisterSignalrService`, `StateService`), pas par des `signal()`/`computed()` (7 occurrences seulement, marginal). Ne pas introduire de `signal()` isolé dans un coin du code sans cohérence avec le reste du composant/service.
- **Forms** : vérifier le composant existant avant de choisir Reactive vs Template-driven — les deux styles peuvent coexister selon les vues.

## CoreUI — icônes

Toutes les icônes CoreUI utilisées dans l'app sont centralisées dans [icons/icon-subset.ts](src/app/icons/icon-subset.ts) : import group depuis `@coreui/icons`, agrégées dans `iconSubset` (objet) et dupliquées dans l'enum `IconSubset` (pour un typage sûr des noms d'icône). `AppComponent` charge `iconSetService.icons = { ...iconSubset }` une seule fois au bootstrap (singleton `IconSetService`).

**Pour ajouter une icône** : l'importer depuis `@coreui/icons`, l'ajouter à la fois dans l'objet `iconSubset` et dans l'enum `IconSubset` de `icon-subset.ts` — ne pas l'importer localement dans un composant, sous peine d'incohérence entre les icônes déclarées et celles réellement enregistrées dans le singleton.

**Règle héritée du CLAUDE.md racine** : ne jamais ajouter `providers: [IconSetService]` dans un composant enfant — cela crée une instance isolée sans les icônes enregistrées par `AppComponent`.

## Routes applicatives (`app.routes.ts`)

| Route | Chargement | Garde |
|---|---|---|
| `/` | redirige vers `login` | — |
| `/videogames` | `views/videogames/routes` (lazy) | `authGuard` |
| `/systems` | `views/systems/routes` (lazy) | `authGuard` |
| `/misterremote` | `views/mister-remote/routes` (lazy) | `authGuard` |
| `/guestaccess` | `views/guest-access/routes` (lazy) | `authGuard` |
| `/script` | `views/mister-script/routes` (lazy) | `authGuard` |
| `/mistersettings` | `views/mister-settings/routes` (lazy) | `authGuard` |
| `/jobscan` | `views/job-scan/routes` (lazy) | `authGuard` |
| `/pages/*` | `views/pages/routes` (lazy) | — (login, register, erreurs) |
| `/login`, `/login/:cmd`, `/register` | composants standalone directs | — |
| `/404`, `/500`, `/apierror` | composants standalone directs | — |
| `**` | redirige vers `login` | — |

`dashboard` est présent en commentaire dans `app.routes.ts` — désactivé, ne pas le réactiver sans vérifier son état.

## Services clés

| Service | Rôle |
|---|---|
| `AuthService` (`auth.service.ts`) | JWT via `@auth0/angular-jwt` (`JwtHelperService`). `api_login`, `api_loginwithoutauthentication`, flux invité (`api_requestguestaccess` signe une clé en MD5 côté client, `api_guestaccessconsumed/state/current/action`). Stocke le JWT brut dans `localStorage['access_token']`, décode le claim `role` en `usertype`. Exporte aussi `authInterceptor` (fonction) qui ajoute `Authorization: Bearer <token>` aux requêtes sortantes. |
| `MisterSignalrService` (`mister-signalr.service.ts`) | Connexion `signalR.HubConnectionBuilder` vers `/hub/misterhub` (transport WebSockets, reconnexion auto avec backoff `[1000,5000,5000,5000,10000,10000,20000]`). Expose `managerCacheRefresh$` (event serveur `RefreshCache`) et `managerJobRomScan$` (event `JobRomScanRefresh`) via `BehaviorSubject`. Écrit `api_state` (santé) dans `localStorage` à chaque cache reçu. Boucle de retry manuelle (`interval(5000)`) si `.start()` échoue, indépendante du backoff SignalR intégré. |
| `QuerygamesService` (`querygames.service.ts`) | Requêtes de recherche/filtre jeux côté API. |
| `StateService` (`state.service.ts`) | Wrapper `BehaviorSubject<any>` pour du state UI partagé simple. |
| `services/models/` | ~40 interfaces/classes TS miroir des entités C# de `libMisterLauncher/Entity/` (ex. `videogame-db.ts`, `system-db.ts`, `rom-db.ts`, `manager-cache.ts`, `job-romscan.ts`, `guest-access.ts`, `ha-switch-state.ts`, `core-savestate.ts`, `module-setting.ts`). Toute évolution d'entité côté `libMisterLauncher/Entity/` doit être répercutée ici. |

## Notes techniques importantes (rappel)

- **`http.get<string>()`** : préférer un objet JSON `{ state }` plutôt qu'une chaîne brute pour éviter les erreurs de parsing du `HttpClient` Angular (déjà signalé dans le CLAUDE.md racine, s'applique en particulier aux endpoints type `haswitch`).
- Voir [MiSTerLauncher.Server/CLAUDE.md](../MiSTerLauncher.Server/CLAUDE.md) pour le détail des routes API consommées et [libMisterLauncher/CLAUDE.md](../libMisterLauncher/CLAUDE.md) pour les entités métier miroir.
