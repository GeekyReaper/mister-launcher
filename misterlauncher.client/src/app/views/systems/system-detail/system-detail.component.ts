import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilSettings, cilSearch, cilPen } from '@coreui/icons';
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
  ImgModule,
  ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent
} from '@coreui/angular';
import { Observable, Subscription } from 'rxjs';

import { QuerygamesService } from '../../../services/querygames.service';
import { SystemInfo } from '../../../services/models/system-info';
import { VideogameSearchResult } from '../../../services/models/videogame-search-result';
import { VideogameSearchRequest } from '../../../services/models/videogame-search-request';
import { ItemlistVideogameComponent } from '../../../components/itemlist-videogame/itemlist-videogame.component'
import { ModaleditSystemComponent } from '../../../components/modaledit-system/modaledit-system.component'
import { MediaurlPipe } from '../../../pipe/mediaurl.pipe'
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { ManagerCache } from '../../../services/models/manager-cache';
import { ModuleHealthcheck } from '../../../services/models/module-healthcheck';


@Component({
  selector: 'app-system-detail',
  templateUrl: './system-detail.component.html',
  styleUrl: './system-detail.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, ContainerComponent, ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent, CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    Tabs2Module, ItemlistVideogameComponent, ModaleditSystemComponent, MediaurlPipe,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent],
  providers: [IconSetService]
})
export class SystemDetailComponent implements OnInit, OnDestroy {
  id: string = '';
  subQuery!: Subscription;
  subQueryGameTop10!: Subscription;
  subQueryFirst!: Subscription;
  subQueryFavorite!: Subscription;
  subQueryPlaylater!: Subscription;

  subUpdateCache !: Subscription;
  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  canLaunch: Boolean = true;

  //subQueryVideoGameLaunch!: Subscription;

  public system!: SystemInfo
  public gamesresultTop10!: VideogameSearchResult;
  public gamesresultFirst!: VideogameSearchResult;
  public gamesresultFavorite!: VideogameSearchResult;
  public gamesresultPlaylater!: VideogameSearchResult;
  videoPlayer!: HTMLVideoElement;
  nbgames: number = 15;

  public modalSettingsVisible = false;

  constructor(private route: ActivatedRoute, private router: Router, private querygamesservice: QuerygamesService, private location: Location,
    private misterSignalr: MisterSignalrService,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilSettings, cilSearch, cilPen };
  }

  ngOnDestroy(): void {
    this.subQuery?.unsubscribe();
    this.subQueryGameTop10?.unsubscribe();
    this.subQueryFirst?.unsubscribe();
    this.subQueryFavorite?.unsubscribe();
    this.subQueryPlaylater?.unsubscribe();
    this.subUpdateCache?.unsubscribe();
  }

  ngOnInit(): void {
    //this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.route.params.subscribe(
      params => {
        const id = params['id'];
        this.id = id;
        console.log(this.id);
        // Get system Info
        this.subQuery = this.querygamesservice.getSystemDetail(this.id).pipe().subscribe((s: SystemInfo) => {
          this.system = s;
          console.log(this.system);

          // Get First
          let searchFirst: VideogameSearchRequest =
          {
            systems: [this.id],
            yearMax: this.system.startyear + 5,
            minRating: 5,
            limit: this.nbgames,
            pagesize: this.nbgames,
            gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
            sortFields: [{
              field: "year",
              isascending: true
            }, {
              field: "rating",
              isascending: false
            }]
          }
          this.subQuery = this.querygamesservice.getVideoGames(searchFirst).pipe().subscribe((gsr: VideogameSearchResult) => {
            this.gamesresultFirst = gsr;
            console.log(this.gamesresultFirst);
          });

        });
        // Get top 10 Games
        let searchTop10: VideogameSearchRequest =
        {
          systems: [this.id],
          pagesize: this.nbgames,
          limit: this.nbgames,
          gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
          minRating: 15,
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        }
        this.subQuery = this.querygamesservice.getVideoGames(searchTop10).pipe().subscribe((gsr: VideogameSearchResult) => {
          this.gamesresultTop10 = gsr;
          console.log(this.gamesresultTop10);


        });

        let searchFavorite: VideogameSearchRequest =
        {
          systems: [this.id],
          playlist: 'favorite',
          limit: this.nbgames,
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        };
        this.subQueryFavorite = this.querygamesservice.getVideoGames(searchFavorite).pipe().subscribe((gsr: VideogameSearchResult) => {
          this.gamesresultFavorite = gsr;
          console.log(this.gamesresultFavorite);

        });

        let searchPlaylater: VideogameSearchRequest =
        {
          systems: [this.id],
          playlist: 'playlater',
          limit: this.nbgames,
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        };
        this.subQueryPlaylater = this.querygamesservice.getVideoGames(searchPlaylater).pipe().subscribe((gsr: VideogameSearchResult) => {
          this.gamesresultPlaylater = gsr;
          console.log(this.gamesresultPlaylater = gsr);
        });
      });
    
    this.subUpdateCache = this.managercache$.subscribe(w => {

      w.health.moduleHealthchecks.forEach((h: ModuleHealthcheck) => {

        if (h.name == "MisterRemote") {
          this.canLaunch = h.misterState == "OK";
        }
      });
    });
   

  }
  goBack() {
    this.location.back();
  }

  goSearch(filter: string) {
    var request: VideogameSearchRequest | undefined = undefined
    switch (filter) {
      case "top":
        request = {
          systems: [this.id],
          pagesize: this.nbgames,
          gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
          minRating : 15,
          sortFields: [{
            field: "rating",
            isascending: false
            }]
        }
        break;
      case "first":
        request = {
          systems: [this.id],
          yearMax: this.system.startyear + 5,
          minRating: 5,
          pagesize: this.nbgames,
          gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
          sortFields: [{
            field: "year",
            isascending: true
          }, {
            field: "rating",
            isascending: false
          }]
        }
        break;
      case "favorite":
        request = {
          systems: [this.id],
          playlist: 'favorite',
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        };
        break;
      case "playlater":
        request = {
          systems: [this.id],
          playlist: 'playlater',
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        };
        break;     
    }
    if (request != undefined) {
      this.querygamesservice.setCurrentVideoGameSearchRequest(request);
      this.router.navigateByUrl("/videogames/search");
    }

    
   
  }

  toggleLiveDemo() {
    this.modalSettingsVisible = !this.modalSettingsVisible;
  }

  handleLiveDemoChange(event: any) {
    this.modalSettingsVisible = event;
  }

  Refresh(event: any) {
    console.log(`SystemEvent - Receive event ${event}`);
    this.modalSettingsVisible = false;
    this.ngOnInit();
  }
 
}
