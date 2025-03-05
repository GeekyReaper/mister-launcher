import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Observable, BehaviorSubject, Subscription, interval } from 'rxjs';
import { ManagerCache } from './models/manager-cache';
import { JobRomscan } from './models/job-romscan';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MisterSignalrService {

  private hubConnection: signalR.HubConnection;
  private managerCacheRefreshSubject = new BehaviorSubject<ManagerCache>({
    health: {
      moduleHealthchecks: [{
        name: 'API',
        misterState: 'ERROR'
      }],
      misterState: "ERROR",
      messages: ["API disconnected"]
    },
    stats: {
      videogamesCount: 0,
      systemsCount: 0,
      systemsCountWithVideogames: 0,
      romsCount: 0,
      romsCountMatch: 0,
      mediaCount: 0,
      mediaDownloadCount: 0,
      mediaDownloadSize:0
    },
    playingVideoGame: {
      currentVideoGame: {
        isPlaying: false
      },
      haschanged: false
    },
    lastUpdate: new Date()
  });
  private managerJoRomScanSubject = new BehaviorSubject<JobRomscan>({
    jobName: "none",
    jobType: "UNKNOW",
    logs : [],
    delay : 0,
    start : new Date(),
    state : "RUNNING",
    result : {
      income : 0,
      insert : 0,
      match : 0,
      update : 0,
      videogameCreate : 0,
      videogameUpdate: 0,
      progress: 0
    }
    
  });
  managerCacheRefresh$: Observable<ManagerCache> = this.managerCacheRefreshSubject.asObservable();
  managerJobRomScan$: Observable<JobRomscan> = this.managerJoRomScanSubject.asObservable();
  retrySubscription?: Subscription;

  //misterState : string = "ERROR";

  constructor(private auth : AuthService) {

    //localStorage.setItem("api_state", "ERROR") 
    //console.debug('constructor signaleR');

    this.hubConnection = new signalR.HubConnectionBuilder()    
        .withUrl('/hub/misterhub',
          {
            transport: signalR.HttpTransportType.WebSockets,
            timeout : 5000
          }
    ) // Replace with your SignalR hub URL
      .withAutomaticReconnect([1000, 5000, 5000, 5000, 10000, 10000, 20000])
      .build();    

    this.StartConnection();

    this.hubConnection.on('RefreshCache', (cache: ManagerCache) => {
      this.managerCacheRefreshSubject.next(cache);
      //this.misterState = cache.health.misterState;
      localStorage.setItem("api_state", cache.health.misterState);
      //console.log(`SignalR receive: mister state = ${this.misterState}`);
      //console.log(cache);

    });

    this.hubConnection.on('JobRomScanRefresh', (job: JobRomscan) => {
      this.managerJoRomScanSubject.next(job);
      //console.log('SignalR receive:');
      //console.log(job);

    });

    //console.log(`signalR state : ${this.hubConnection.state}`);

    //console.log(`my id ${this.hubConnection.connectionId}`);

    //this.sendMessage("send from consutror");
  }


    sendMessage(message: string): void {
      this.hubConnection.send("RefreshCache", message);
  }

  StartConnection() {
    this.hubConnection
      .start()
      .then(() => {
        //console.log('Connected to SignalR hub');
        //console.log(`signalR state : ${this.hubConnection.state}`);
        //console.log(`my id ${this.hubConnection.connectionId}`);
        this.retrySubscription?.unsubscribe();
        this.retrySubscription = undefined;
        //console.log("[MisterSignalrService] Close Retry")
      })
      .catch(err => {
        console.log('Error connecting to SignalR hub:', err)
        // Active connection retry
        if (this.retrySubscription == undefined) {
          //
          console.log("[MisterSignalrService] Activate Retry")
          this.retrySubscription = interval(5000).subscribe(() => {
            this.StartConnection();

          });
        }


      });
  }
  


}
