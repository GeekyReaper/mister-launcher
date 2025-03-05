import { Component, Input, OnDestroy, OnInit, signal, Output, EventEmitter } from '@angular/core';
import { Observable, Subscription, timer } from 'rxjs'
import { CommonModule, NgStyle, Location, DatePipe, AsyncPipe } from '@angular/common';
import { cilInfo, cilBadge } from '@coreui/icons';
import { IconSetService } from '@coreui/icons-angular';

import { MisterSignalrService } from '../../services/mister-signalr.service';
import { QuerygamesService } from '../../services/querygames.service';


import { JobRomscan } from '../../services/models/job-romscan';
import { TimelapsePipe } from "../../pipe/timelapse.pipe"
import { FilesizePipe } from "../../pipe/filesize.pipe";
import { MisterDatePipe } from "../../pipe/misterdate.pipe"
import { JobstateColorPipe } from "../../pipe/jobstate-color.pipe"

import {
  BorderDirective,
  BadgeModule, BadgeComponent,
  Tabs2Module,
  ContainerComponent,
  ButtonDirective,
  TableDirective, TableModule,
  CardBodyComponent, CardComponent, CardFooterComponent, CardGroupComponent, CardHeaderComponent, CardImgDirective, CardLinkDirective, CardSubtitleDirective, CardTextDirective, CardTitleDirective,
  ColComponent, RowComponent,
  GutterDirective,
  ListGroupDirective,  ListGroupItemDirective,  
  TextColorDirective,  
  CollapseDirective,
  FormSelectDirective,
  AlertComponent,
  ProgressComponent
} from '@coreui/angular';


@Component({
  selector: 'app-view-job',
  templateUrl: './view-job.component.html',
  styleUrl: './view-job.component.scss',
  standalone: true,
  imports: [TimelapsePipe, FilesizePipe, MisterDatePipe, DatePipe, JobstateColorPipe,
    BorderDirective,
    BadgeModule,
    Tabs2Module,
    CommonModule,
    ContainerComponent,
    ButtonDirective,
    TableDirective,
    TableModule,
    CardBodyComponent,
    CardComponent,
    CardFooterComponent,
    CardGroupComponent,
    CardHeaderComponent,
    CardImgDirective,
    CardLinkDirective,
    CardSubtitleDirective,
    CardTextDirective,
    CardTitleDirective,
    ColComponent,
    GutterDirective,
    ListGroupDirective,
    ListGroupItemDirective,   
    RowComponent,
    TextColorDirective,    
    BadgeComponent,
    CollapseDirective,
    FormSelectDirective,
    ProgressComponent,
    AlertComponent,
    AsyncPipe
  ],
  providers: [IconSetService]
})
export class ViewJobComponent implements OnInit, OnDestroy {

  subCurrentJob?: Subscription;
  subTimerCurrentJob?: Subscription;
  subTimerJobrequest? : Subscription

  mytimer = timer(5000,5000);
  currentJob?: JobRomscan;

  public managerJobScanRom$: Observable<JobRomscan> = this.misterSignalr.managerJobRomScan$;

  jobRunning: Boolean = false;
  timerOn: Boolean = false;

  @Input() jobtypefilter!: string;
  @Output() changeJobRunning = new EventEmitter<Boolean>();

  constructor(
    private misterSignalr: MisterSignalrService,
    private querygamesservice: QuerygamesService,
    public iconSet: IconSetService,
    private location: Location) {
    iconSet.icons = {
      cilInfo, cilBadge
    };
  }


    ngOnInit(): void {
      this.subCurrentJob = this.managerJobScanRom$.subscribe((job: JobRomscan) => {
        this.currentJob = job
        this.jobRunning = job.state == "RUNNING";
        this.changeJobRunning.emit(this.jobRunning);
        if (this.jobRunning) {
          this.setTimer();
        }
        else {
          this.closeTimer();
        }

      });
    }
    ngOnDestroy(): void {
      this.subCurrentJob?.unsubscribe();
      this.subTimerCurrentJob?.unsubscribe();
      this.subTimerJobrequest?.unsubscribe();
  }
  setTimer(): void {
    if (!this.timerOn) {
      console.log("[view-job] Set Timer")
      this.timerOn = true;    
      this.subTimerCurrentJob = this.mytimer.subscribe((val) => {
        console.log(`[view-job] Timer tick ${val} `);
        this.subTimerJobrequest = this.querygamesservice.GetCurrentJob().subscribe(
          (job: JobRomscan) => {
          this.currentJob = job;
          console.log('[view - job] Timer receive job');
            this.jobRunning = job.state == "RUNNING";
            this.changeJobRunning.emit(this.jobRunning);
          if (this.jobRunning) {
            this.setTimer();
          }
          else {
            this.closeTimer();
          }
          },
          error => {
            console.log('[view-job] Timer Http ' + error)
            this.closeTimer();
            if (this.currentJob) {
              this.currentJob.state = "DONE";
            }
            //if (this.currentJob?.state == "RUNNING") {
            //  this.currentJob = undefined;
            //}           
            this.jobRunning = false;
            this.changeJobRunning.emit(this.jobRunning);
          },
          () => { });
      });
    }
     
  }
  closeTimer(): void  {
    console.log("[view-job] Close timer")
    this.subTimerCurrentJob?.unsubscribe();
    this.timerOn = false;
  }

  }



