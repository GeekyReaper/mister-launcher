import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle, Location, DecimalPipe } from '@angular/common';
import { cilInfo, cilBadge, cilPaperPlane } from '@coreui/icons';
import { FooterModule } from '@coreui/angular';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { QuerygamesService } from '../../../services/querygames.service';

import {
  AccordionButtonDirective,  AccordionComponent,  AccordionItemComponent,  TemplateIdDirective,
  BorderDirective,
  BadgeModule,
  TableDirective, TableModule, Tabs2Module,
  ContainerComponent,
  ButtonDirective,
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
  ImgDirective,
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective, FormCheckComponent, FormCheckInputDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  ProgressComponent,
  InputGroupComponent, InputGroupTextDirective
} from '@coreui/angular';
import { JobRomscan } from '../../../services/models/job-romscan';
import { SystemDb } from '../../../services/models/system-db';
import { SystemSearchRequest } from '../../../services/models/system-search-request';
import { SystemSearchResult } from '../../../services/models/system-search-result';
import { RomInfo } from '../../../services/models/rom-info';
import { TimelapsePipe } from "../../../pipe/timelapse.pipe"
import { ItemCount } from '../../../services/models/item-count';
import { FilesizePipe } from "../../../pipe/filesize.pipe";
import { ManagerCache } from '../../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../../services/models/module-healthcheck';
import { SystemInfo } from '../../../services/models/system-info';
import { ViewJobComponent } from '../../../components/view-job/view-job.component'
import { System } from 'typescript';

@Component({
  selector: 'app-job-scan-rom',
  templateUrl: './job-scan-rom.component.html',
  styleUrl: './job-scan-rom.component.scss',
  standalone: true,
  imports: [AccordionButtonDirective, AccordionComponent, AccordionItemComponent, TemplateIdDirective,
    RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, FormCheckComponent, FormCheckInputDirective,
    ButtonDirective,
    ImgDirective, Tabs2Module,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, TableModule, IconDirective, TableDirective,
    BadgeModule, ProgressComponent,
    ViewJobComponent,
    InputGroupComponent, InputGroupTextDirective, 
    TimelapsePipe, FilesizePipe, DecimalPipe],
  providers: [IconSetService]
})

export class JobScanRomComponent implements OnInit, OnDestroy {

  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  subListSystem?: Subscription;
  subListConsoleSystem?: Subscription;
  subListArcadeSystem?: Subscription;
  subManagerCache?: Subscription;
  subSendScan?: Subscription;
  subSelectSystem?: Subscription;
  activeTab?: string = "system";

  selectedsystemid: string = "All";

  selectedSystem?: SystemInfo;

  requiredModule?: ModuleHealthcheck;

  consolesystemsfound: SystemInfo[] = [];
  arcadesystemsfound: SystemInfo[] = [];

  consolestat: systemStat = { statromfound : 0, statrommatch : 0, statvideogame : 0, systems : 0 };
  arcadestat: systemStat = { statromfound: 0, statrommatch: 0, statvideogame: 0, systems : 0};


  systemlist: ItemCount[] = [];
  formJobScanRom!: FormGroup;
  canlaunchJob: Boolean = false;

  constructor(private route: ActivatedRoute,
    private misterSignalr: MisterSignalrService,
    private querygamesservice: QuerygamesService,
    public iconSet: IconSetService,
    private formBuilder: FormBuilder,
    private location: Location) {

    iconSet.icons = {
      cilInfo, cilBadge, cilPaperPlane
    };
  }

  ngOnInit(): void {

    this.route.params.subscribe(
      params => {
        const tab = params['tab'];
        this.activeTab = tab;

        this.selectedsystemid = params['system'];
        if (this.selectedsystemid != undefined && this.selectedsystemid != "" && this.selectedsystemid != "All" && (this.selectedsystemid != "Arcade")) {
          this.subSelectSystem = this.querygamesservice.getSystemDetail(this.selectedsystemid).subscribe((s: SystemInfo) => this.selectedSystem = s);
        }
        if (this.selectedsystemid == undefined || this.selectedsystemid == "") {
          this.selectedsystemid = "All"
        }
      });
    if (this.activeTab != "system" && this.activeTab != "rom")
      this.activeTab = "rom";

    this.location.replaceState(this.activeTab == "rom" ? `/jobscan/scan/${this.activeTab}/${this.selectedsystemid}` : `/jobscan/scan/${this.activeTab}`);


    this.subListSystem = this.querygamesservice.SystemCountFilter("allrom").subscribe((systems: ItemCount[]) => {
        this.systemlist = systems;     

    });
    let systemconsolesearch: SystemSearchRequest =
    {
      category: 'Console',
      AllowNoVideoGame : true,
      sortFields: [{
        field: 'name',
        isAscending: true
      }]

    }
    let systemarcadesearch: SystemSearchRequest =
    {
      category: 'Arcade',
      AllowNoVideoGame : true,
      sortFields: [{
        field: 'name',
        isAscending: true
      }]

    }


    this.subListConsoleSystem = this.querygamesservice.getSystems(systemconsolesearch).subscribe((sysresult: SystemSearchResult) => {
      this.consolesystemsfound = sysresult.systems;
      this.consolestat = this.AggregateSystemStat(sysresult.systems);
    });

    this.subListArcadeSystem = this.querygamesservice.getSystems(systemarcadesearch).subscribe((sysresult: SystemSearchResult) => {
      this.arcadesystemsfound = sysresult.systems;
      this.arcadestat = this.AggregateSystemStat(sysresult.systems);
    });

     



      this.subManagerCache = this.managercache$.subscribe(w => {
        //console.log('[remote] managercache update');


        w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

          if (h.name == "MongoDb") { //"MisterFtp") {
            this.requiredModule = h;
          }
        });
      });

      this.formJobScanRom = this.formBuilder.group(
        {
          selectSystem: [this.selectedsystemid]
        });
    }
  ngOnDestroy(): void {
      this.subListSystem?.unsubscribe();
      this.subManagerCache?.unsubscribe();
      this.subSendScan?.unsubscribe();
      this.subSelectSystem?.unsubscribe();
      this.subListConsoleSystem?.unsubscribe();
      this.subListArcadeSystem?.unsubscribe();
  }

  launchJobScanRom(): void {
    this.subSendScan = this.querygamesservice.LaunchJobScanRom(this.formJobScanRom.value.selectSystem).subscribe((b: Boolean) => {
      console.log(`Scan is launched : ${b}`)
    })
  }

  launchJobScanSystem(): void {
    this.subSendScan = this.querygamesservice.LaunchJobScanSystem().subscribe((b: Boolean) => {
      console.log(`Scan is launched : ${b}`)
    })
  }

  onChangeSystem(): void {
    this.selectedsystemid = this.formJobScanRom.value.selectSystem;
    console.log(`Selected system ${this.selectedsystemid}`);
    if (this.selectedsystemid != "" && this.selectedsystemid != "All" && (this.selectedsystemid != "Arcade")) {
      this.subSelectSystem = this.querygamesservice.getSystemDetail(this.selectedsystemid).subscribe((s: SystemInfo) => this.selectedSystem = s);
    }
    else {
      this.selectedSystem = undefined;
    }
  }

  changeJobRunning(running: Boolean) {
    console.log(`[jobscan] chnageJobRunning ${running}`)
    this.canlaunchJob = running;
  }

  tabChange(event: any): void {
    
    this.activeTab = event as string;
    this.location.replaceState(this.activeTab == "rom" ? `/jobscan/scan/${this.activeTab}/${this.selectedsystemid}` : `/jobscan/scan/${this.activeTab}`);

  }

  AggregateSystemStat(systems: SystemInfo[]): systemStat {

    let stat: systemStat = { statromfound: 0, statrommatch: 0, statvideogame: 0, systems : 0 };
    systems.forEach((s: SystemInfo) => {
      stat.systems++;
      stat.statvideogame += s.statvideogame;
      stat.statromfound += s.statromfound;
      stat.statrommatch += s.statrommatch;
    });
    return stat;
  }
}

export interface systemStat {
  systems: number;
  statvideogame: number;
  statromfound: number;
  statrommatch: number;
}
