import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription, interval } from 'rxjs'
import { CommonModule, NgStyle } from '@angular/common';
import { cilGamepad, cilPowerStandby, cilSettings, cilSync, cilVolumeHigh, cilLaptop, cilVolumeLow, cilVolumeOff, cilImage, cilSave, cilShareBoxed } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';

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
import { AuthService } from '../../services/auth.service';
import { GuestAccess } from '../../services/models/guest-access';
import { TimeofdayPipe } from '../../pipe/timeofday.pipe';

@Component({
  selector: 'app-guest-access',
  templateUrl: './guest-access.component.html',
  styleUrl: './guest-access.component.scss',
  standalone: true,
  imports: [BorderDirective, CommonModule,
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
    BadgeModule,
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
    FormSelectDirective,
    IconModule, IconDirective,
    TimeofdayPipe],
  providers: [IconSetService]

})
export class GuestAccessComponent implements OnInit, OnDestroy {

  subGetGuestAccessPool?: Subscription;
  subRequestAction?: Subscription;
  subGetGuestAccess?: Subscription;
  currentGuestAccess : GuestAccess[] = []

  constructor(public iconSet: IconSetService, private auth : AuthService) {
    iconSet.icons = {
      cilGamepad, cilLaptop, cilPowerStandby, cilSettings, cilSync, cilVolumeHigh, cilVolumeLow, cilVolumeOff, cilImage, cilSave, cilShareBoxed
    };
  }
  ngOnInit(): void {
    this.loadrequest();

    this.subGetGuestAccessPool = interval(5000).subscribe(() => {
      this.loadrequest();     
    })
        
    }
    ngOnDestroy(): void {
      this.subGetGuestAccessPool?.unsubscribe();
      this.subGetGuestAccess?.unsubscribe();
  }

  loadrequest() {
    this.subGetGuestAccess = this.auth.api_guestaccesscurrent().subscribe((cga: GuestAccess[]) => {
      this.currentGuestAccess = cga;
      console.log(cga);
    });
  }

  requestaction(approuve: Boolean, code: string): void {
    this.subRequestAction = this.auth.api_guestaccessaction(code, approuve).subscribe((response: Boolean) => {
      if (response) {
        this.loadrequest();
      }
    });
  }

  guestAccessBadgeColor(state: string): string {
    switch (state) {
      case "PENDING":
        return "info";
      case "DENIED":
        return "danger";
      case "BLOCK":
        return "warning";
      case "APPROUVED":
        return "warning";
      case "CONSUMED":
        return "success";
    }
    return "info";
  }

}
