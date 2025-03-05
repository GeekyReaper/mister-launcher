import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilSave, cilActionUndo } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';

import { NgStyle } from '@angular/common';
import { Tabs2Module } from '@coreui/angular';

import {
  BorderDirective,
  ButtonDirective,
  ButtonCloseDirective,
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
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective, FormSelectDirective, FormModule,
  BadgeComponent,
  CollapseDirective,  
  ContainerComponent,
  ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
  CarouselComponent,
  CarouselControlComponent,
  CarouselInnerComponent,
  CarouselItemComponent,
  CarouselIndicatorsComponent,
  TooltipComponent, TooltipDirective, TooltipModule,
  ThemeDirective,
  ImgModule,
  ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
  InputGroupComponent, InputGroupTextDirective,
  ButtonGroupComponent, ButtonModule  
} from '@coreui/angular';
import { Subscription } from 'rxjs';

import { QuerygamesService } from '../../services/querygames.service';
import { SystemInfo } from '../../services/models/system-info';



@Component({
  selector: 'app-modaledit-system',
  templateUrl: './modaledit-system.component.html',
  styleUrl: './modaledit-system.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormModule, FormsModule, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, ContainerComponent, ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent, CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    ReactiveFormsModule, ButtonCloseDirective,
    TooltipComponent, TooltipDirective, TooltipModule,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    Tabs2Module, IconDirective,
    InputGroupComponent, InputGroupTextDirective,
    ButtonGroupComponent, ButtonModule,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent],
  providers: [QuerygamesService, IconSetService]
})
export class ModaleditSystemComponent implements OnInit, OnDestroy {

  formEditSystem!: FormGroup;

  @Input() systeminfo!: SystemInfo;
  @Input() visible: Boolean = false;

  @Output() NeedRefresh = new EventEmitter<string>();

  allowSaveState: Boolean = false;
  allowSaveMemory: Boolean = false;

  subscribeUpdate! : Subscription
  

  constructor(private querygamesservice: QuerygamesService, private formBuilder: FormBuilder,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilSave, cilActionUndo };
  }



  ngOnInit(): void {
    
    this.formEditSystem = this.formBuilder.group(
      {
        formName: [this.systeminfo.name],
        formCore: [this.systeminfo.core],
        formCompany: [this.systeminfo.company],
        formExtensions: [this.systeminfo.extensions, [Validators.required, Validators.minLength(3)]],
        formExcludeRomPatterns: [this.systeminfo.excluderompaterns],
        formUnofficialPathRomPatterns: [this.systeminfo.unofficalpathrompaterns],
        formSupporttype: [this.systeminfo.supporttype],
        formEndyear: [this.systeminfo.startyear],// [Validators.required, Validators.pattern("\d{4}")]],
        formStartyear: [this.systeminfo.endyear],// [Validators.required, Validators.pattern("\d{4}")]],
        formGamepath: [this.systeminfo.gamepath, [Validators.required, Validators.minLength(3)]],
        formAllowsSaveStates: [this.systeminfo.allowSaveStates],
        formAllowsSaveMemory: [this.systeminfo.allowSaveMemory]
      });

    this.allowSaveState = this.systeminfo.allowSaveStates == undefined ? false : this.systeminfo.allowSaveStates;
    this.allowSaveMemory = this.systeminfo.allowSaveMemory == undefined ? false : this.systeminfo.allowSaveMemory;

        
    }
    ngOnDestroy(): void {
      this.subscribeUpdate?.unsubscribe();
  }

  toggleVisibility() {
    this.visible = !this.visible;
  }

  handleVisibilityChange(event: any) {
    this.visible = event;
  }

  onSubmit(): void {
    console.log("Submit system form");
    console.log(this.formEditSystem.invalid);

    if (this.formEditSystem.invalid) {
      return;
    }

    this.systeminfo.name = this.formEditSystem.value.formName;
    this.systeminfo.core = this.formEditSystem.value.formCore;      
    this.systeminfo.company = this.formEditSystem.value.formCompany;
    this.systeminfo.extensions = this.formEditSystem.value.formExtensions
    this.systeminfo.supporttype = this.formEditSystem.value.formSupporttype;
    this.systeminfo.startyear = this.formEditSystem.value.formEndyear;
    this.systeminfo.endyear = this.formEditSystem.value.formStartyear;
    this.systeminfo.gamepath = this.formEditSystem.value.formGamepath;
    this.systeminfo.allowSaveStates = this.formEditSystem.value.formAllowsSaveStates;
    this.systeminfo.allowSaveMemory = this.formEditSystem.value.formAllowsSaveMemory;
    this.systeminfo.excluderompaterns = this.formEditSystem.value.formExcludeRomPatterns;
    this.systeminfo.unofficalpathrompaterns = this.formEditSystem.value.formUnofficialPathRomPatterns;



    this.subscribeUpdate = this.querygamesservice.UpdateSystemSettings(this.systeminfo).subscribe((s: SystemInfo) => {
      this.toggleVisibility();
      console.log('Refresh emit');
      this.NeedRefresh.emit("reload system")
    });
  }
}
