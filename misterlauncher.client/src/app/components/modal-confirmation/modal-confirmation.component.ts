import { Component, ElementRef, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { cilThumbUp, cilThumbDown } from '@coreui/icons';
import { NgStyle } from '@angular/common';
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
  BadgeComponent,
  CollapseDirective,
  ContainerComponent,
  TableModule, UtilitiesModule, GridModule,
  TooltipComponent, TooltipDirective, TooltipModule,
  Tabs2Module,
  ThemeDirective,
  ImgModule,
  AlertComponent,
  ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent,
  FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective
} from '@coreui/angular';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-modal-confirmation',
  templateUrl: './modal-confirmation.component.html',
  styleUrl: './modal-confirmation.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ButtonDirective, BadgeComponent, CollapseDirective, ContainerComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, ImgModule, IconDirective,
    ButtonCloseDirective,
    TooltipComponent, TooltipDirective, TooltipModule,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    Tabs2Module,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent,
    AlertComponent],
    providers :[ IconSetService]
})
export class ModalConfirmationComponent {



  @Input() title: string = "Confirmation";
  @Input() message: string = "";
  @Input() color : string = "light"
  @Input() options: ConfirmationOption[] = []
  @Input() visible: Boolean = false;
  @Output() onSelectedOption = new EventEmitter<string>();

  constructor(
    public iconSet: IconSetService) {
    iconSet.icons = { cilThumbUp, cilThumbDown };
  }

  toggleVisibility() {
    this.visible = !this.visible;
  }

  handleVisibilityChange(event: any) {
    this.visible = event;
  }

  onclick(value: string) {
    this.onSelectedOption.emit(value);    
  }


}

export interface ConfirmationOption {
  label: string
  value: string
  icon: string
  color: string
}
