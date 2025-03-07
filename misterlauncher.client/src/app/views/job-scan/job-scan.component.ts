import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle, Location } from '@angular/common';
import { cilInfo, cilBadge, cilSearch, cilPen, cilPaperPlane } from '@coreui/icons';
import { FooterModule } from '@coreui/angular';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormControl } from '@angular/forms';
import { MisterSignalrService } from '../../services/mister-signalr.service';
import { QuerygamesService } from '../../services/querygames.service';

import {
  BorderDirective,
  BadgeModule,  
  ContainerComponent,
  ButtonDirective,
  FormCheckLabelDirective,
  TableDirective, TableModule, Tabs2Module,
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
import { JobRomscan } from '../../services/models/job-romscan';
import { SystemDb } from '../../services/models/system-db';
import { SystemSearchRequest } from '../../services/models/system-search-request';
import { SystemSearchResult } from '../../services/models/system-search-result';
import { RomInfo } from '../../services/models/rom-info';
import { TimelapsePipe } from "../../pipe/timelapse.pipe"
import { ItemCount } from '../../services/models/item-count';
import { FilesizePipe } from "../../pipe/filesize.pipe";
import { ViewJobComponent } from '../../components/view-job/view-job.component'

@Component({
  selector: 'app-job-scan',
  templateUrl: './job-scan.component.html',
  styleUrl: './job-scan.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, FormCheckComponent, FormCheckInputDirective,
    ButtonDirective,
    FormCheckLabelDirective, FormCheckInputDirective,
    ImgDirective, Tabs2Module,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, TableModule, IconDirective, TableDirective,
    BadgeModule, ProgressComponent,
    ViewJobComponent,
    InputGroupComponent, InputGroupTextDirective, 
    TimelapsePipe, FilesizePipe ],
  providers: [IconSetService]
})
export class JobScanComponent implements OnInit, OnDestroy  {

  subSendScan?: Subscription;
  subGetConsole?: Subscription;
  subGetRomsUnmatch?: Subscription;
  formJob!: FormGroup;
  formJobUnmatchRom!: FormGroup;
  consoleslist?: ItemCount[]
  consolesUnmatchRom?: ItemCount[];
  romsUnMatch?: RomInfo[];
  activeTab: string = "automatic";
  manualsystemselect: string = "";
  selectedRoms: string[] = [];

  canlaunchJob: Boolean = false;
  jobrunning: Boolean = true;

  radioOptions = [
    { value: '0', label: 'New', selected:true },
    { value: '404', label: 'Not found' },
    { value: '429', label: 'Exceed' },
    { value: '409', label: 'Unlink' },
    { value: '405', label: 'Error' }
  ];

  selectedstatefilter: string[] = ["0"]

  



  constructor(private route: ActivatedRoute,
    //private misterSignalr: MisterSignalrService,
    private querygamesservice: QuerygamesService,
    public iconSet: IconSetService,
    private formBuilder: FormBuilder,
    private location: Location) {

    iconSet.icons = {
      cilInfo, cilBadge, cilSearch, cilPen, cilPaperPlane
    };


  }
  ngOnInit(): void {

    // LoadRom
    this.route.params.subscribe(
      params => {
        const tab = params['tab'];
        this.activeTab = tab;
        this.manualsystemselect = params['system'];
        });
    if (this.activeTab != "automatic" && this.activeTab != "manual")
      this.activeTab = "automatic";

    this.location.replaceState("/jobscan/matchingrom/" + this.activeTab)

    let systemselect = true;
    if (this.manualsystemselect == "" || this.manualsystemselect == undefined) {
      systemselect = false;
      this.manualsystemselect = "Arcade";
    }
    else {
      if (this.activeTab == "manual") {
        this.location.replaceState("/jobscan/matchingrom/" + this.activeTab + "/" + this.manualsystemselect)
      }
    }

        
    this.subGetConsole = this.querygamesservice.SystemCountFilter("unmatchrom").subscribe((systems: ItemCount[]) => {
      this.consoleslist = systems;
      this.consolesUnmatchRom = systems;
    });
    this.formJob = this.formBuilder.group(
      {
        selectConsole: ['All'],        
      });
    this.formJobUnmatchRom = this.formBuilder.group(
      {
        selectConsole: [this.manualsystemselect]//['Arcade']
      });

  


    if (systemselect) {
      this.searchunmatchroms();
    }

    }

    ngOnDestroy(): void {
      this.subSendScan?.unsubscribe();
      this.subGetConsole?.unsubscribe();
      this.subGetRomsUnmatch?.unsubscribe();
    }

  requestscan(type: string): void {
    if (type == "Arcade") {
      this.subSendScan = this.querygamesservice.ScanArcadeRom().subscribe((b: Boolean) => {
        console.log(`Scan is launched : ${b}`)
      })
    };
    if (type == "Console") {
      console.log(`Request scan for : ${this.formJob.value.selectConsole}`);
      this.subSendScan = this.querygamesservice.ScanConsoleRom(this.formJob.value.selectConsole).subscribe((b: Boolean) => {
        console.log(`Scan is launched : ${b}`)
      })
    }

  }

  launchautomaticmatchrom(): void {
    let filter = 0;
    //if (this.btnRadioGroup.value.radioToggle != undefined) {
    //  filter = +this.btnRadioGroup.value.radioToggle;
    //}
    console.log(`Request scan for : ${this.formJob.value.selectConsole} with filter option ${filter}`);
    
    this.subSendScan = this.querygamesservice.AutomaticMatchRomJob(this.formJob.value.selectConsole, this.selectedstatefilter.map( (s:string) => +s)).subscribe((b: Boolean) => {
      console.log(`Scan is launched : ${b}`)
    })
  }

  searchunmatchroms(): void {
    console.log(`Seach unmatch roms for : ${this.formJobUnmatchRom.value.selectConsole}`);
    if (this.formJobUnmatchRom.value.selectConsole == "Arcade") {
      this.subGetRomsUnmatch = this.querygamesservice.GetUnmatchRoms("Arcade", "").subscribe((r: RomInfo[]) => {
        this.romsUnMatch = r;
      });
    }
    else {
      this.subGetRomsUnmatch = this.querygamesservice.GetUnmatchRoms("Console", this.formJobUnmatchRom.value.selectConsole).subscribe((r: RomInfo[]) => {
        this.romsUnMatch = r;
      });
    }

    this.manualsystemselect = this.formJobUnmatchRom.value.selectConsole

    this.location.replaceState("/jobscan/matchingrom/" + this.activeTab + "/" + this.manualsystemselect)
   
  }

  rowselected(romid : string): void {
    console.log(`Rom selected ${romid}`);
  }


  tabChange(event: any): void {
    console.log(event);
   
      this.activeTab = event as string;
      this.location.replaceState("/jobscan/matchingrom/" + this.activeTab)
   
  }

  romcheckbox($event: any, romid: string): void {
    let isChecked = $event.srcElement.checked;
    //console.log(`Checkbox - ${isChecked} - rom ${romid}`);

    if (isChecked) {
      this.selectedRoms.push(romid);
    }
    else {
      this.selectedRoms = this.selectedRoms.filter (s => s!=romid)
    }
    
    console.log(this.selectedRoms);


  }

  setCheckboxValue(value: string): void {
   
    if (this.selectedstatefilter.includes(value)) {
      this.selectedstatefilter = this.selectedstatefilter.filter((s: string) => s != value);
    }
    else {
      this.selectedstatefilter.push(value);
    }
    this.canlaunchJob = !this.jobrunning && this.selectedstatefilter.length > 0;
  }

  changeJobRunning(running: Boolean) {
    console.log(`[jobscan] changeJobRunning ${running}`)
    this.jobrunning = running;
    this.canlaunchJob = !this.jobrunning && this.selectedstatefilter.length > 0;
  }

  

  
}
