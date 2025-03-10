import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle, Location } from '@angular/common';
import { cilGamepad, cilPowerStandby, cilSettings, cilSync, cilVolumeHigh, cilLaptop, cilVolumeLow, cilVolumeOff, cilImage, cilSave, cilShareBoxed } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective, FormControl } from '@angular/forms';

import {
  BorderDirective,
  BadgeModule,
  SpinnerComponent, SpinnerModule,
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
  ButtonCloseDirective,
  Tabs2Module,
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
  ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent,
  TooltipComponent, TooltipDirective, TooltipModule,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  AlertComponent,
  InputGroupComponent, InputGroupTextDirective
} from '@coreui/angular';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { ModuleSetting } from '../../services/models/module-setting'
import { MisterSignalrService } from '../../services/mister-signalr.service';
import { QuerygamesService } from '../../services/querygames.service';
import { ModuleHealthcheck } from '../../services/models/module-healthcheck';
import { ManagerCache } from '../../services/models/manager-cache';
import { MisterstateColorPipe } from '../../pipe/misterstate-color.pipe';
import { AuthService } from '../../services/auth.service';


@Component({
  selector: 'app-mister-settings',
  templateUrl: './mister-settings.component.html',
  styleUrl: './mister-settings.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    ImgDirective,
    Tabs2Module,
    ButtonCloseDirective,
    TooltipComponent, TooltipDirective, TooltipModule,
    BadgeComponent, CollapseDirective, FormSelectDirective, RouterLink, TableModule, IconDirective, TableDirective,
    ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent,
    BadgeModule, AlertComponent, SpinnerModule, SpinnerComponent,
    MisterstateColorPipe,
    InputGroupComponent, InputGroupTextDirective],
  providers: [IconSetService]
})
export class MisterSettingsComponent implements OnInit, OnDestroy {

  subFtpModuleSettings!: Subscription
  public FtpModuleSettings!: ModuleSetting[];
  formFtpModule!: FormGroup;
  ftpcurrenthealthcheck!: ModuleHealthcheck;

  subAuthModuleSettings!: Subscription
  public AuthModuleSettings!: ModuleSetting[];
  formAuthModule!: FormGroup;
  authcurrenthealthcheck!: ModuleHealthcheck;


  subMediaModuleSettings!: Subscription
  public MediaModuleSettings!: ModuleSetting[];
  formMediaModule!: FormGroup;
  mediacurrenthealthcheck!: ModuleHealthcheck;

  subRemoteModuleSettings!: Subscription
  public RemoteModuleSettings!: ModuleSetting[];
  formRemoteModule!: FormGroup;
  remotecurrenthealthcheck!: ModuleHealthcheck;

  subScreenScrapperModuleSettings!: Subscription
  public ScreenScrapperModuleSettings!: ModuleSetting[];
  formScreenScrapperModule!: FormGroup;
  screenscrappercurrenthealthcheck!: ModuleHealthcheck;

  subscriptionCache!: Subscription;

  currentAction: string = "";

  position = 'top-center';
  visible = signal(false);
  percentage = signal(0);
  public tooltipmodulename: string = "";
  public tooltipcolor: string = "info";
  public tooltipmsg: string = "";
  public tooltipmodulestate: string = "UNKNOW";

  activeTab: string = "remote";


  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;


  constructor(private route: ActivatedRoute,
    private misterSignalr: MisterSignalrService,
    private querygamesservice: QuerygamesService,
    public iconSet: IconSetService,
    private formBuilder: FormBuilder,
    private auth: AuthService,
    private location: Location) {
    
    iconSet.icons = {
      cilGamepad, cilLaptop, cilPowerStandby, cilSettings, cilSync, cilVolumeHigh, cilVolumeLow, cilVolumeOff, cilImage, cilSave, cilShareBoxed
    };
  }
  ngOnInit(): void {

    this.route.params.subscribe(
      params => {
        const tab = params['tab'] != undefined && params['tab'] != "settings" ? params['tab'] : "remote" ;
        this.activeTab = tab;        
      });
    

    this.location.replaceState(`/mistersettings/${this.activeTab}`);


    this.subscriptionCache = this.managercache$.subscribe((w: ManagerCache) => {

      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.remotecurrenthealthcheck = h;
        }
        if (h.name == "MisterFtp") {
          this.ftpcurrenthealthcheck = h;
        }
        if (h.name == "MisterMedia") {
          this.mediacurrenthealthcheck = h;
        }
        if (h.name == "ScreenScrapper") {
          this.screenscrappercurrenthealthcheck = h;
        }
        if (h.name == "MisterAuth") {
          this.authcurrenthealthcheck = h;
        }

      });
    });

    this.formFtpModule = this.formBuilder.group({
      formDefault: [""]
    });
    this.formMediaModule = this.formBuilder.group({
      formDefault: [""]
    });
    this.formRemoteModule = this.formBuilder.group({
      formDefault: [""]
    });
    this.formScreenScrapperModule = this.formBuilder.group({
      formDefault: [""]
    });
    this.formAuthModule = this.formBuilder.group({
      formDefault: [""]
    });

    this.subFtpModuleSettings = this.querygamesservice.GetModuleSettings("MisterFtp").subscribe((items: ModuleSetting[]) => {
      this.FtpModuleSettings = items;   
      this.formFtpModule = this.setForm(this.FtpModuleSettings);
    });
    this.subMediaModuleSettings = this.querygamesservice.GetModuleSettings("MisterMedia").subscribe((items: ModuleSetting[]) =>
    {
      this.MediaModuleSettings = items;
      this.formMediaModule = this.setForm(this.MediaModuleSettings);
    });
    this.subRemoteModuleSettings = this.querygamesservice.GetModuleSettings("MisterRemote").subscribe((items: ModuleSetting[]) =>
    {
      this.RemoteModuleSettings = items;
      this.formRemoteModule = this.setForm(this.RemoteModuleSettings);
    });
    this.subScreenScrapperModuleSettings = this.querygamesservice.GetModuleSettings("ScreenScrapper").subscribe((items: ModuleSetting[]) => {
      this.ScreenScrapperModuleSettings = items;
      this.formScreenScrapperModule = this.setForm(this.ScreenScrapperModuleSettings);
    });
    this.subAuthModuleSettings = this.querygamesservice.GetModuleSettings("MisterAuth").subscribe((items: ModuleSetting[]) => {
      this.AuthModuleSettings = items;
      this.formAuthModule = this.setForm(this.AuthModuleSettings);
    });
  }
  ngOnDestroy(): void {
    this.subFtpModuleSettings?.unsubscribe();
    this.subAuthModuleSettings?.unsubscribe();
    this.subMediaModuleSettings?.unsubscribe();
    this.subRemoteModuleSettings?.unsubscribe();
    this.subScreenScrapperModuleSettings?.unsubscribe();
    this.subscriptionCache?.unsubscribe();
  }

  public setForm(settings: ModuleSetting[]): FormGroup {
    const group: any = {};
    settings.forEach((setting: ModuleSetting) => {
      group[setting.name] = new FormControl(setting.value, Validators.required);
    });
    return this.formBuilder.group(group);
  } 

  public OnSubmitFtp(): void {
    
    this.submitForm(this.formFtpModule, this.FtpModuleSettings, )
    

  }

  public submitForm(form: FormGroup, settings: ModuleSetting[]): void {
    if (settings.length == 0) {
      return;
    }
    let modulename = settings[0].moduleName;

    this.currentAction = `save_${modulename}`;

    console.log(settings);
    settings.forEach((setting: ModuleSetting) => {
      setting.value = form.controls[setting.name].value;
    });
    this.querygamesservice.SetModuleSettings(settings).subscribe((b: Boolean) => {
      this.currentAction = ""; // RESET
      console.log(`Update result ${b}`)
    })
  }

  public checkForm(form: FormGroup, settings: ModuleSetting[]) : void {

    if (settings.length == 0) {
      return;
    }

    let modulename = settings[0].moduleName;

    this.currentAction = `check_${modulename}`;

    console.log("check");
    
    settings.forEach((setting: ModuleSetting) => {
      setting.value = form.controls[setting.name].value;
    });
    this.querygamesservice.CheckModuleSettings(settings).subscribe(
      (b: ModuleHealthcheck) => {
        this.currentAction = ""; // RESET
        (this.setToaster(b))
      }
    );
  }

  public CheckSpinner(settings: ModuleSetting[], action: string): Boolean {
    if (settings.length == 0 || this.currentAction == "" ) {
      return false;
    }  
    let actioncheck = `${action}_${settings[0].moduleName}`;
    return this.currentAction == actioncheck
  }

  public setFormMedia(): void {
    //console.log(this.MediaModuleSettings)
    const group: any = {};
    this.MediaModuleSettings.forEach((setting: ModuleSetting) => {
      group[setting.name] = new FormControl(setting.value, Validators.required);
    });
    this.formMediaModule = this.formBuilder.group(group);
  }

  public OnSubmitMedia(): void {
    console.log(this.formMediaModule)
  }

  onClosed() {
    this.visible.update((value: Boolean) => !value);
  }

  onVisibleChange($event: boolean) {
    this.visible.set($event);
    this.percentage.set(this.visible() ? this.percentage() : 0);
  }

  onTimerChange($event: number) {
    this.percentage.set($event * 25);
  }

  setToaster(healthcheck: ModuleHealthcheck) : void {
    this.visible.update((value: Boolean) => !value);
    this.tooltipcolor = healthcheck.misterState == 'OK' ? 'success' : healthcheck.misterState == 'WARNING' ? 'warning' : 'danger';
    this.tooltipmodulename = healthcheck.name;
    this.tooltipmodulestate = healthcheck.misterState;
    this.tooltipmsg = healthcheck.message ? healthcheck.message : "Success";
  }

  tabChange(event: any): void {

    this.activeTab = event as string;
    this.location.replaceState(`/mistersettings/${this.activeTab}`);

  }
}
