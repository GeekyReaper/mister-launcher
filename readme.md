# MiSTerLauncher Project

This project aims to facilitate the use of [MisterFPGA](https://mister-devel.github.io/MkDocs_MiSTer/) by providing a web interface to search and launch games.

## Prerequisites
- A MongoDB database
- Access to the [ScreenScraper](https://www.screenscraper.fr/) API for retrieving game information and media.
- An operational MisterFPGA with [MisterRemote](https://github.com/wizzomafizzo/mrext/blob/main/docs/remote.md) and FTP access enabled via the dedicated script.
- ROMs loaded in the GAMES directory

## Technical Stack
- ASP .Net Core 8.0
- Angular CLI 18.0.5 with [COREUI](https://coreui.io/angular/)
- MongoDB 4.0 or higher (compatibility with 4.0 allows hosting the MongoDB database on a NAS)

## Docker Hosting
**DockerHub** => [https://hub.docker.com/repository/docker/geekyreaper/misterlauncherserver/](https://hub.docker.com/repository/docker/geekyreaper/misterlauncherserver/)

Example of Docker Compose:

```yaml
services:
  apigamedb:
    image: geekyreaper/misterlauncherserver:latest
    container_name: geekyreaper-misterlauncher
    ports:
     - 6101:8080/tcp
     - 6102:8081/tcp
    environment:
      - PUID=1029
      - PGID=100
      - TZ=Europe/Paris
      - ASPNETCORE_HTTP_PORTS=8080
      - GDB_MONGO_CNX=mongodb://...
    volumes:
      - /...:/data:rw
    restart: unless-stopped
```
**Important**: Create a volume to persist the media retrieved from the ScreenScrapper site.

## Initialization

- STEP 1: Configure the modules; they must all be operational.
- STEP 2: Scan the Systems (Console and Arcade).
- STEP 3: Scan the ROMs for each System. Traverse the directory tree of the GAMES directory present on the SD card or connected USB drives.
- STEP 4: Associate the ROMs with VideoGames from the ScreenScrapper database. This step retrieves the information and media.

## Screenshots

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot1.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot2.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot3.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot4.png)
