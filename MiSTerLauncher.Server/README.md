
## How TAG image with Date

Command Line
`docker tag geekyreaper/misterlauncherserver:latest geekyreaper/misterlauncherserver:yyyy-mm-dd`
`docker push geekyreaper/misterlauncherserver:latest`
`docker push geekyreaper/misterlauncherserver:yyyy-mm-dd`

## Environement Variable required
`GDB_MONGO_CNX` : Mongo DB connection string

Optional
`GDB_MONGO_DBNAME` : Define database name. Default is retrogaming