import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule } from '@angular/common';
import { FooterModule } from '@coreui/angular';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, flagSet, freeSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilDelete, cilPen, cilNoteAdd } from '@coreui/icons';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import { NgStyle } from '@angular/common';
import {
  ContainerComponent,
  BorderDirective,
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
  FormDirective, FormLabelDirective, FormControlDirective, FormSelectDirective,
  BadgeComponent,
  CollapseDirective  
} from '@coreui/angular';


import { StateService } from '../../services/state.service';
import { QuerygamesService } from '../../services/querygames.service';
import { VideogameSearchRequest } from '../../services/models/videogame-search-request';
import { VideogameSearchResult } from '../../services/models/videogame-search-result';
import { VideogameDb } from '../../services/models/videogame-db';
import { ItemlistVideogameComponent } from '../../components/itemlist-videogame/itemlist-videogame.component'
import { FilterVideogameComponent } from '../../components/filter-videogame/filter-videogame.component';
import { PaginationVideogameComponent } from '../../components/pagination-videogame/pagination-videogame.component'
import { MisterSignalrService } from '../../services/mister-signalr.service';
import { ManagerCache } from '../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../services/models/module-healthcheck';

@Component({
  selector: 'app-videogames',
  standalone: true,
  imports: [ContainerComponent, RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FilterVideogameComponent, PaginationVideogameComponent,
    ItemlistVideogameComponent, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, IconDirective],
  providers: [IconSetService],
  templateUrl: './videogames.component.html',
  styleUrl: './videogames.component.scss',

})
export class VideogamesComponent implements OnInit, OnDestroy {

  public videogamesresult: VideogameSearchResult | undefined;
  subQueryVideoGames!: Subscription;
  subUpdateCache !: Subscription;
  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  canLaunch: Boolean = true;
  searching: Boolean = false;

  constructor(
    private querygamesservice: QuerygamesService,
    private misterSignalr: MisterSignalrService,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilDelete, cilPen, cilNoteAdd, ...brandSet };
  }

  ngOnInit(): void {

    this.subUpdateCache = this.managercache$.subscribe(w => {
      
      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.canLaunch = h.misterState == "OK";
        }
      });
    });
  }


  OnPageChange(page: number) {
    let searchrequest = this.querygamesservice.getCurrentVideoGameSearchRequest();
    searchrequest.page = page;
    this.querygamesservice.setCurrentVideoGameSearchRequest(searchrequest);
    this.NewSearchRequest(searchrequest);
  }

  ResetSearch(option: string): void {
    this.videogamesresult = undefined;
  }

  ngOnDestroy(): void {
    this.subQueryVideoGames?.unsubscribe();
    this.subUpdateCache?.unsubscribe();
  }

  
  NewSearchRequest(searchRequest: VideogameSearchRequest): void {
    this.searching = true;
    this.subQueryVideoGames = this.querygamesservice.getVideoGames(searchRequest).subscribe((gs: VideogameSearchResult) => {
      this.videogamesresult = gs;
      this.searching = false;
        window.scroll(0, 0);
    });    
  }

}
