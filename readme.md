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
**DockerHub** => [https://hub.docker.com/r/geekyreaper/misterlauncherserver/](https://hub.docker.com/r/geekyreaper/misterlauncherserver/)

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
      - GDB_MONGO_CNX=mongodb://servicename
    volumes:
      - /...:/data:rw
    restart: unless-stopped
```
**Important**: 
- The image does not contain mongodb, it should be defined as a second service.
- Create a volume to persist the media retrieved from the ScreenScrapper site.

## Initialization

- STEP 1: Go to "Settings" and setup all modules. Don't forget to turn on your mister. They must all be operational.
- STEP 2: Go to "Scan," select tab "Systems" and launch "Scan System".
- STEP 3: Go to "Scan," select tab "Rom", pick a system or "All system", then launch "Scan Rom". The first full scan will be very long. The automatic process will check all Games repositories on the SD card and on connected USB drives.
- STEP 4: Go to "Matching", select tab "Automatic", pick a system or "All system". The first full scan will be very long.

## Screenshots

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot1.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot2.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot3.png)

![screenshot](https://github.com/GeekyReaper/mister-launcher/blob/main/screenshots/misterlauncher-screenshot4.png)
