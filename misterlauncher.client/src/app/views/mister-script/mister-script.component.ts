import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription, interval } from 'rxjs'
import { CommonModule, NgStyle } from '@angular/common';
import { cilMediaPlay } from '@coreui/icons';
import { FooterModule } from '@coreui/angular';
//import { QuerygamesService } from '../../services/querygames.service';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';

import {
  BorderDirective,
  AlertComponent, AlertModule,
  BadgeModule,
  ContainerComponent,
  SpinnerComponent, SpinnerModule,
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
  ImgDirective,
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective
} from '@coreui/angular';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';


import { ManagerCache } from '../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../services/models/module-healthcheck';
import { PlayingVideogame } from '../../services/models/playing-videogame';
import { QuerygamesService } from '../../services/querygames.service';
import { MisterSignalrService } from '../../services/mister-signalr.service';
import { ScriptsResult } from '../../services/models/scripts-result';

@Component({
  selector: 'app-mister-script',
  templateUrl: './mister-script.component.html',
  styleUrl: './mister-script.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    ImgDirective,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, TableModule, IconDirective, TableDirective,
    BadgeModule,
    AlertComponent, AlertModule,
    SpinnerComponent, SpinnerModule],
  providers: [IconSetService]
})
export class MisterScriptComponent implements OnInit, OnDestroy {

  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  subManagerCache?: Subscription; 
  public modulehealth?: ModuleHealthcheck;

  subGetScript?: Subscription;
  subTimerGetScript?: Subscription;
  subExecuteScript?: Subscription;

  scriptinfo: ScriptsResult = {
    canLaunch: false,
    scripts : []
  }


  constructor(private misterSignalr: MisterSignalrService, private querygamesservice: QuerygamesService, public iconSet: IconSetService) {

    iconSet.icons = {
      cilMediaPlay
    };
  }
    ngOnDestroy(): void {
      this.subManagerCache?.unsubscribe();
      this.subExecuteScript?.unsubscribe();
      this.subGetScript?.unsubscribe();
      this.subTimerGetScript?.unsubscribe();
    }
  ngOnInit(): void {
    this.subManagerCache = this.managercache$.subscribe(w => {
      
      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.modulehealth = h;
        }
      });
    });

    //Load scripts
    this.subGetScript = this.querygamesservice.GetScripts().subscribe((r: ScriptsResult) => {
      this.scriptinfo = r;
      this.subGetScript?.unsubscribe();
    });

    this.launchautomaticrefresh();

  }

  launchautomaticrefresh(): void {
    this.subTimerGetScript?.unsubscribe();

    this.subTimerGetScript = interval(5000).subscribe(() => {
      if (!this.scriptinfo.canLaunch) {
        this.subGetScript = this.querygamesservice.GetScripts().subscribe((r: ScriptsResult) => {
          this.scriptinfo = r;
          this.subGetScript?.unsubscribe();
        });
      }
    })
  }


  executeScript(name : string) {
    if (this.scriptinfo.canLaunch) {
      this.subExecuteScript = this.querygamesservice.ExecuteScript(name, false).subscribe((b:Boolean) => {
        if (b) {
          this.scriptinfo.canLaunch = false;
          // reset timer          
          this.launchautomaticrefresh()          
          
        }
      });
    }
  }

 
}
