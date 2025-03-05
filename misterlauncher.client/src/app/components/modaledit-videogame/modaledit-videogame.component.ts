import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild, viewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { cilSave, cilActionUndo, cilDelete, cilTrash, cilVideogame } from '@coreui/icons';
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
  InputGroupComponent, InputGroupTextDirective
} from '@coreui/angular';
import { Subscription } from 'rxjs';

import { QuerygamesService } from '../../services/querygames.service';
import { VideogameDb } from '../../services/models/videogame-db';

import { ModalConfirmationComponent, ConfirmationOption } from '../modal-confirmation/modal-confirmation.component';



@Component({
  selector: 'app-modaledit-videogame',
  templateUrl: './modaledit-videogame.component.html',
  styleUrl: './modaledit-videogame.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormModule, FormsModule, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, ContainerComponent, ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent, CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    ReactiveFormsModule, ButtonCloseDirective,
    TooltipComponent, TooltipDirective, TooltipModule,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    Tabs2Module,
    InputGroupComponent, InputGroupTextDirective,
    IconDirective,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent,
    ModalConfirmationComponent],
  providers: [QuerygamesService, IconSetService]
})
export class ModaleditVideogameComponent implements OnInit, OnDestroy {

  formEdit!: FormGroup;

  @Input() videogame!: VideogameDb;
  @Input() visible: Boolean = false;

  @Output() NeedRefresh = new EventEmitter<string>();
  

  subscribeUpdate!: Subscription
  confirmationShow: boolean = false;

  confirmationModalOptionsForDelete: ConfirmationOption[] = [
    { label: "Yes", value: "yes", color: "warning", icon: "cilThumbUp" },
    { label: "No", value: "no", color: "secondary", icon: "cilThumbDown" }];


  constructor(private querygamesservice: QuerygamesService, private formBuilder: FormBuilder,
    public iconSet: IconSetService, private location: Location,) {
    iconSet.icons = { cilSave, cilActionUndo, cilDelete, cilTrash, cilVideogame };
  }



  ngOnInit(): void {

    this.formEdit = this.formBuilder.group(
      {
        formName: [this.videogame.name],
        formYear: [this.videogame.year],
        formCollection: [this.videogame.collection],
        formCollectionId: [this.videogame.collectionId],
        formEditoName: [this.videogame.editorname],
        formDevelopname: [this.videogame.developname],
        formnbPlayers: [this.videogame.nbplayers],
        formRating: [this.videogame.rating]
      });

   


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

  onDelete(): void {
    this.subscribeUpdate = this.querygamesservice.DeleteVideogame(this.videogame._id).subscribe((b: Boolean) => {
      if (b) {
        this.location.back();
      }
    });
  }

  onSubmit(): void {
    console.log("Submit videogame form");
    console.log(this.formEdit.invalid);

    if (this.formEdit.invalid) {
      return;
    }   

    this.videogame.name = this.formEdit.value.formName;
    this.videogame.collection = this.formEdit.value.formCollection;
    this.videogame.collectionId = this.formEdit.value.formCollectionId;
    this.videogame.editorname = this.formEdit.value.formEditoName;
    this.videogame.developname = this.formEdit.value.formDevelopname;
    this.videogame.nbplayers = this.formEdit.value.formnbPlayers;
    this.videogame.rating = this.formEdit.value.formRating;
    this.videogame.year = this.formEdit.value.formYear;

    

    this.subscribeUpdate = this.querygamesservice.UpdateVideogameSettings(this.videogame).subscribe((s: VideogameDb) => {
      this.toggleVisibility();
      this.NeedRefresh.emit("reload videogame");
    });
  }

  showConfirmationDelete(): void {
    this.confirmationShow = true;
  }

  DeleteVideogame(value: string): void {
    if (value == "yes") {
      this.subscribeUpdate = this.querygamesservice.DeleteVideogame(this.videogame._id).subscribe((b: Boolean) => {
        if (b) {
          this.location.back();
        }
      });
    }
    else {
      this.confirmationShow = false;
    }

    
  }
}
