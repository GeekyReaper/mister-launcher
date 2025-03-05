import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NgScrollbar } from 'ngx-scrollbar';
import { IconDirective, IconModule, IconSetService } from '@coreui/icons-angular';
import {
  brandSet, cilMenu, cilGamepad, cilFeaturedPlaylist,
  cilSpeedometer, cilDrop, cilDescription, cilPencil, cilPuzzle, cilNotes, cilCursor, cilStar, cilChartPie, cilCalculator, cilLaptop,
  cilSun, cilMoon, cilContrast, cilShieldAlt, cilVideogame, cilCast, cilSave, cilSync, cilBell,
  cilList, cilMobile, cilImage, cilMovie, cilMagnifyingGlass, cilFile, cilSettings, cilAccountLogout,
  cilRouter, cilZoom,
  cilSend,
  cilTerminal, cilLink
} from '@coreui/icons';

import {
  ContainerComponent,
  INavData,
  ShadowOnScrollDirective,
  SidebarBrandComponent,
  SidebarComponent,
  SidebarFooterComponent,
  SidebarHeaderComponent,
  SidebarNavComponent,
  SidebarToggleDirective,
  SidebarTogglerDirective
} from '@coreui/angular';

import { DefaultFooterComponent, DefaultHeaderComponent } from './';
import { navItems } from './_nav';
import { AuthService } from '../../services/auth.service';

function isOverflown(element: HTMLElement) {
  return (
    element.scrollHeight > element.clientHeight ||
    element.scrollWidth > element.clientWidth
  );
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './default-layout.component.html',
  styleUrls: ['./default-layout.component.scss'],
  standalone: true,
  imports: [
    SidebarComponent,
    SidebarHeaderComponent,
    SidebarBrandComponent,
    RouterLink,
    IconDirective,
    NgScrollbar,
    SidebarNavComponent,
    SidebarFooterComponent,
    SidebarToggleDirective,
    SidebarTogglerDirective,
    DefaultHeaderComponent,
    ShadowOnScrollDirective,
    ContainerComponent,
    RouterOutlet,
    DefaultFooterComponent
  ],
  providers: [IconSetService]
})
export class DefaultLayoutComponent {
  public navItems = navItems;

  constructor(public iconSet: IconSetService, private auth : AuthService) {

    iconSet.icons = {
      cilSpeedometer, cilDrop, cilFeaturedPlaylist,
      cilDescription, cilPencil, cilPuzzle, cilNotes, cilCursor, cilStar, cilChartPie,
      cilCalculator, cilGamepad, cilMenu, cilShieldAlt, cilVideogame, cilLaptop, cilSun,
      cilMoon, cilContrast, cilCast, cilSave, cilSync, cilBell, cilList,
      cilMobile, cilImage, cilMovie, cilMagnifyingGlass, cilFile, cilRouter, cilSettings, cilAccountLogout, cilSend, cilTerminal, cilZoom, cilLink, ...brandSet
    };

    
    console.log (`usertype ${auth.usertype}`)
    this.navItems = this.navItems.filter((item: INavData) => {
      return (item.attributes == undefined) || (auth.usertype == item.attributes['role'])
    })
  }

  onScrollbarUpdate($event: any) {
    // if ($event.verticalUsed) {
    // console.log('verticalUsed', $event.verticalUsed);
    // }
  }
}
