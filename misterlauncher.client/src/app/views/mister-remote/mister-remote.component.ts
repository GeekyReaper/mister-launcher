import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle } from '@angular/common';

import { FooterModule } from '@coreui/angular';
//import { QuerygamesService } from '../../services/querygames.service';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';

import {
  BorderDirective,
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
  FormDirective, FormLabelDirective, FormControlDirective, FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  Tabs2Module,
  TemplateIdDirective
} from '@coreui/angular';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';


import { ManagerCache } from '../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../services/models/module-healthcheck';
import { PlayingVideogame } from '../../services/models/playing-videogame';
import { QuerygamesService } from '../../services/querygames.service';
import { MisterSignalrService } from '../../services/mister-signalr.service';
import { CoreSavestate } from '../../services/models/core-savestate';
import { HaSwitchState } from '../../services/models/ha-switch-state';
import { MediaurlPipe } from '../../pipe/mediaurl.pipe'

@Component({
  selector: 'app-mister-remote',
  templateUrl: './mister-remote.component.html',
  styleUrl: './mister-remote.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    ImgDirective,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, TableModule, IconDirective, TableDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    BadgeModule,
    SpinnerComponent, SpinnerModule,
    TemplateIdDirective, Tabs2Module, MediaurlPipe ],
})
export class MisterRemoteComponent implements OnInit, OnDestroy {

  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  public modulehealth?: ModuleHealthcheck;
  public haModuleHealth?: ModuleHealthcheck;
  public haSwitchState?: HaSwitchState;
  public haToggleLoading: boolean = false;
  public playingvideogame?: PlayingVideogame;
  subscription?: Subscription;
  subscription2?: Subscription;
  subSaveState?: Subscription;
  subHaSwitch?: Subscription;
  public savestates?: CoreSavestate[];
  public ratingBadgeColor: string = "";
  public allowMemorySave: Boolean = false;

  savestateAction: string = "";

  constructor(private misterSignalr: MisterSignalrService, private querygamesservice: QuerygamesService, public iconSet: IconSetService) {
  }

  ngOnInit(): void {    
    this.subscription2 = this.managercache$.subscribe(w => {
      console.log('[remote] managercache update');
      
      this.playingvideogame = w.playingVideoGame.currentVideoGame
      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.modulehealth = h;
        }
        if (h.name == "HomeAssistant") {
          this.haModuleHealth = h;
          if (h.misterState === 'OK' && this.haSwitchState === undefined) {
            console.log("--- Module is OK check refreshSwitchstate");
            this.refreshHaSwitchState();            
          }
        }
      });

      

      if (this.playingvideogame?.currentVideogame !=null) {
        this.ratingBadgeColor = this.playingvideogame?.currentVideogame?.rating > 17 ? 'success' : this.playingvideogame?.currentVideogame?.rating > 14 ? 'primary' : this.playingvideogame?.currentVideogame?.rating > 10 ? 'info' : this.playingvideogame?.currentVideogame?.rating > 5 ? 'warning' : 'danger'
      }
      
      this.allowMemorySave = this.playingvideogame.isPlaying && this.playingvideogame.systemDb!=null && this.playingvideogame.systemDb.allowSaveMemory;
      console.log(this.playingvideogame.systemDb);
      console.log(`memorysave : ${this.allowMemorySave}`)

      if (this.playingvideogame.isPlaying && (this.playingvideogame.systemDb?.allowSaveStates)) {
        this.subSaveState = this.querygamesservice.getSaveStates(this.playingvideogame.currentVideogame?._id, this.playingvideogame.playingRomdb?.romid).subscribe((savestats: CoreSavestate[]) => {
          this.savestates = savestats;
          console.log(savestats);
        })
      }
      else {
        this.savestates = undefined;
      }
    });

    this.refreshHaSwitchState();
  }

  saveStateIsLoading(source : string, savestate: CoreSavestate): Boolean {
    return this.savestateAction == `${source}_${savestate.slot}`;
  }

  saveStateIsDisable(source: string, savestate: CoreSavestate): Boolean {
    return this.savestateAction != "" && this.savestateAction != `${source}_${savestate.slot}`;   
  }

  LoadSaveState(savestate: CoreSavestate): void{
    this.savestateAction = `load_${savestate.slot}`;
    this.subSaveState = this.querygamesservice
      .SavestateCmdLoad(savestate.videogameid, savestate.romid, savestate.slot)
      .subscribe((savestats: CoreSavestate[]) => {
        if (savestats != null) {
          this.savestates = savestats;
          //console.log("Load SaveState DONE");
        }
        this.savestateAction = "";
      });     
  }

  SaveSaveState(savestate: CoreSavestate): void {
    this.savestateAction = `save_${savestate.slot}`;
    this.subSaveState = this.querygamesservice
      .SavestateCmdSave(savestate.videogameid, savestate.romid, savestate.slot)
      .subscribe((savestats: CoreSavestate[]) => {
        if (savestats != null) {
          this.savestates = savestats;
          //console.log("Save Savestate DONE");
        }
        this.savestateAction = "";
      });     
  }


  refreshHaSwitchState(): void {
    this.subHaSwitch = this.querygamesservice.GetHaSwitchState().subscribe({
      next: (result: HaSwitchState) => { this.haSwitchState = result; },
      error: () => { this.haSwitchState = undefined; }
    });
  }

  formatElapsed(date?: Date): string {
    if (!date) return '';
    const diffMin = Math.floor((Date.now() - new Date(date).getTime()) / 60000);
    if (diffMin < 60) return `${diffMin} min`;
    const diffH = Math.floor(diffMin / 60);
    if (diffH < 24) return `${diffH} h`;
    return `${Math.floor(diffH / 24)} j`;
  }

  toggleHaSwitch(): void {
    const turnOn = this.haSwitchState?.state !== 'on';
    this.haToggleLoading = true;
    this.querygamesservice.SetHaSwitch(turnOn).subscribe({
      next: () => {
        if (this.haSwitchState) this.haSwitchState = { ...this.haSwitchState, state: turnOn ? 'on' : 'off', lastChanged: new Date() };
        this.haToggleLoading = false;
      },
      error: () => { this.haToggleLoading = false; }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.subscription2?.unsubscribe();
    this.subSaveState?.unsubscribe();
    this.subHaSwitch?.unsubscribe();
  }


  sendRemoteCmd(cmd : string, israw:boolean = false) {
    this.subscription = this.querygamesservice.sendMisterCommand(cmd, israw).subscribe((b: Boolean) => {
      console.log("ok");
    })
  }

  sendSaveMemoryCmd() {
    let cmds : string[] = ["osd", "osd"]    
    this.subscription = this.querygamesservice.sendMisterCommands(cmds, 2000).subscribe((b: Boolean) => {
      console.log("ok");
    })
  }
}
