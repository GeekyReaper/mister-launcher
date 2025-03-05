import { Component, Input } from '@angular/core';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { SystemInfo } from '../../services/models/system-info';
import { CommonModule } from '@angular/common';
import { cilGamepad } from '@coreui/icons';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { MediaurlPipe } from '../../pipe/mediaurl.pipe';
import {
  BorderDirective,
  ContainerComponent,
  ImgModule,
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
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  FooterModule
} from '@coreui/angular';

@Component({
  selector: 'app-itemlist-system',
  templateUrl: './itemlist-system.component.html',
  styleUrl: './itemlist-system.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, ImgModule, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, IconDirective, MediaurlPipe],
  providers: [IconSetService]
})
export class ItemlistSystemComponent {
  constructor(public iconSet: IconSetService) {
    iconSet.icons = { cilGamepad };
  }

  @Input() system!: SystemInfo;
}
