import { Component, Input, OnDestroy, signal } from '@angular/core';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { VideogameDb } from '../../services/models/videogame-db';
import { CommonModule } from '@angular/common';
import { cilGamepad, cilMediaPlay, cilApplications, cilRouter} from '@coreui/icons';
import { ActivatedRoute, RouterLink, RouterLinkActive, Router } from '@angular/router';
import {
  BorderDirective,
  ContainerComponent,
  ButtonDirective,
  CardBodyComponent,
  ButtonCloseDirective,
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
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  FooterModule,
  ToastBodyComponent,
  ToastComponent,
  ToasterComponent,
  ToastHeaderComponent
} from '@coreui/angular';
import { QuerygamesService } from '../../services/querygames.service';
import { PartVideogameCategoriesComponent } from '../part-videogame-categories/part-videogame-categories.component';
import { PartVideogameLaunchbuttonComponent } from '../part-videogame-launchbutton/part-videogame-launchbutton.component';
import { Subscription } from 'rxjs';
import { MediaurlPipe  } from '../../pipe/mediaurl.pipe'

@Component({
  selector: 'app-itemlist-videogame',
  templateUrl: './itemlist-videogame.component.html',
  styleUrl: './itemlist-videogame.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective,
    FormSelectDirective, ButtonCloseDirective, FooterModule, RouterLink, IconDirective,
    ToastBodyComponent,
    ToastComponent,
    ToasterComponent,
    ToastHeaderComponent,
    PartVideogameCategoriesComponent, PartVideogameLaunchbuttonComponent, MediaurlPipe],
  providers: [QuerygamesService, IconSetService]
})
export class ItemlistVideogameComponent {
  constructor(private querygamesservice: QuerygamesService, public iconSet: IconSetService) {
    iconSet.icons = { cilGamepad, cilMediaPlay, cilApplications  };
  }    

  @Input() videogame!: VideogameDb;
  @Input() canLaunch: Boolean = true;
  
}
