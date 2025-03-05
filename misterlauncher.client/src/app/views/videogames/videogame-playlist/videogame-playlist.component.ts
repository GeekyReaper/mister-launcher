import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, flagSet, freeSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLaptop, cilBug } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgStyle } from '@angular/common';
import { Tabs2Module } from '@coreui/angular';
import {
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
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  ContainerComponent,
  ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
  CarouselComponent,
  CarouselControlComponent,
  CarouselInnerComponent,
  CarouselItemComponent,
  CarouselIndicatorsComponent,
  ThemeDirective,
  ImgModule
} from '@coreui/angular';
import { Observable, Subscription } from 'rxjs';

import { QuerygamesService } from '../../../services/querygames.service';
import { VideogameDb } from '../../../services/models/videogame-db';
import { VideogameSearchResult } from '../../../services/models/videogame-search-result';
import { VideogameSearchRequest } from '../../../services/models/videogame-search-request';
import { ItemlistVideogameComponent } from '../../../components/itemlist-videogame/itemlist-videogame.component'
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { ManagerCache } from '../../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../../services/models/module-healthcheck';

@Component({
  selector: 'app-videogame-playlist',
  templateUrl: './videogame-playlist.component.html',
  styleUrl: './videogame-playlist.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    ItemlistVideogameComponent, BadgeComponent, CollapseDirective, FormSelectDirective, ContainerComponent, ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent, CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    Tabs2Module],
  providers: [QuerygamesService, IconSetService]
})
export class VideogamePlaylistComponent implements OnInit, OnDestroy {

  subUpdateCache !: Subscription;
  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  canLaunch: Boolean = true;

  subQueryVideoGameFavorite!: Subscription
  public videogamesFavorite!: VideogameSearchResult;
  subQueryVideoGamePlaylater!: Subscription
  public videogamesPlaylater!: VideogameSearchResult;
  subQueryVideoGameIssue!: Subscription
  public videogamesIssue!: VideogameSearchResult;  

  constructor(private route: ActivatedRoute, private router: Router, private querygamesservice: QuerygamesService, private location: Location,
    private misterSignalr: MisterSignalrService,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLaptop, cilBug, ...brandSet };
  }

  ngOnDestroy(): void {
    this.subQueryVideoGameFavorite?.unsubscribe();
    this.subQueryVideoGamePlaylater?.unsubscribe();
    this.subQueryVideoGameIssue?.unsubscribe();
    this.subUpdateCache?.unsubscribe();
  }

  ngOnInit(): void {
    this.SearchVideoGameFavorite();
    this.SearchVideoGamePlaylater();
    this.SearchVideoGameIssue();
    this.subUpdateCache = this.managercache$.subscribe(w => {

      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.canLaunch = h.misterState == "OK";
        }
      });
    });
  }

  private SearchVideoGameFavorite(): void {
    let search: VideogameSearchRequest =
    {
      playlist : 'favorite',
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryVideoGameFavorite = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.videogamesFavorite = gsr;
    });
  }
  private SearchVideoGamePlaylater(): void {
    let search: VideogameSearchRequest =
    {
      playlist: 'playlater',
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryVideoGamePlaylater = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.videogamesPlaylater = gsr;
    });
  }
  private SearchVideoGameIssue(): void {
    let search: VideogameSearchRequest =
    {
      playlist: 'issue',
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryVideoGameIssue = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.videogamesIssue = gsr;
    });
  }
 
}
