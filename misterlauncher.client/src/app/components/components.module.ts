import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { FooterComponent } from './footer/footer.component';
import { NavbarComponent } from './navbar/navbar.component';
import { SidebarComponent } from './sidebar/sidebar.component';
import { ListRomsComponent } from './list-roms/list-roms.component';
import { ModaleditSystemComponent } from './modaledit-system/modaledit-system.component';
import { PartVideogameCategoriesComponent } from './part-videogame-categories/part-videogame-categories.component';
import { PartVideogameLaunchbuttonComponent } from './part-videogame-launchbutton/part-videogame-launchbutton.component';
import { ModaleditVideogameComponent } from './modaledit-videogame/modaledit-videogame.component';
import { SelectRomComponent } from './select-rom/select-rom.component';
import { ViewJobComponent } from './view-job/view-job.component';
import { ModalConfirmationComponent } from './modal-confirmation/modal-confirmation.component';
import { FilterVideogameComponent } from './filter-videogame/filter-videogame.component';
import { PaginationVideogameComponent } from './pagination-videogame/pagination-videogame.component';

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
  ],
  declarations: [
    FooterComponent,
    NavbarComponent,
    SidebarComponent,
    ListRomsComponent,
    ModaleditSystemComponent,
    PartVideogameCategoriesComponent,
    PartVideogameLaunchbuttonComponent,
    ModaleditVideogameComponent,
    SelectRomComponent,
    ViewJobComponent,
    ModalConfirmationComponent,
    FilterVideogameComponent,
    PaginationVideogameComponent
  ],
  exports: [
    FooterComponent,
    NavbarComponent,
    SidebarComponent
  ]
})
export class ComponentsModule { }
