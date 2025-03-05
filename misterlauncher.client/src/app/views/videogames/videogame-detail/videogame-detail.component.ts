import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, flagSet, freeSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLink, cilLaptop, cilBug, cilSettings, cilPen } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MisterDatePipe } from "../../../pipe/misterdate.pipe";
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
  AccordionButtonDirective,
  AccordionComponent,
  AccordionItemComponent,
  TemplateIdDirective,
  AlertComponent
} from '@coreui/angular';
import { Observable, Subscription } from 'rxjs';

import { QuerygamesService } from '../../../services/querygames.service';
import { VideogameDb } from '../../../services/models/videogame-db';
import { VideogameSearchResult } from '../../../services/models/videogame-search-result';
import { VideogameSearchRequest } from '../../../services/models/videogame-search-request';
import { ItemlistVideogameComponent } from '../../../components/itemlist-videogame/itemlist-videogame.component'
import { ListRomsComponent } from '../../../components/list-roms/list-roms.component';
import { PartVideogameCategoriesComponent } from '../../../components/part-videogame-categories/part-videogame-categories.component';
import { PartVideogameLaunchbuttonComponent } from '../../../components/part-videogame-launchbutton/part-videogame-launchbutton.component'
import { ModaleditVideogameComponent } from '../../../components/modaledit-videogame/modaledit-videogame.component'
import { MediaurlPipe  } from '../../../pipe/mediaurl.pipe'
import { ModuleHealthcheck } from '../../../services/models/module-healthcheck';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { ManagerCache } from '../../../services/models/manager-cache';


@Component({
  selector: 'app-videogame-detail',
  templateUrl: './videogame-detail.component.html',
  styleUrl: './videogame-detail.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent,
    CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective,
    ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective,
    ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent,
    NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent,
    ItemlistVideogameComponent, CollapseDirective, FormSelectDirective, ContainerComponent,
    ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent,
    CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    Tabs2Module,
    AccordionButtonDirective,
    AccordionComponent,
    AccordionItemComponent,
    AlertComponent,
    TemplateIdDirective,
    MisterDatePipe, MediaurlPipe,
    ListRomsComponent, PartVideogameCategoriesComponent, PartVideogameLaunchbuttonComponent, ModaleditVideogameComponent],
  providers: [IconSetService]
})

export class VideogameDetailComponent implements OnInit, OnDestroy {
  id: string = '';
  subQueryVideoGames!: Subscription;  
  subQueryVideoGamePlaylist!: Subscription;

  subUpdateCache !: Subscription;
  public managercache$: Observable<ManagerCache> = this.misterSignalr.managerCacheRefresh$;
  canLaunch: Boolean = true;

  subQueryGameSameCollection!: Subscription
  public gamesSameCollection!: VideogameSearchResult;

  subQueryGameSameCategory!: Subscription
  public gamesSameCategory!: VideogameSearchResult;

  subQueryGameSameYear!: Subscription
  public gamesSameYear!: VideogameSearchResult;

  subQueryGameSameEditor!: Subscription
  public gamesSameEditor!: VideogameSearchResult;

  subQueryVideoGameLaunch!: Subscription;

  public modalSettingsVisible = false;

  nbgames: number = 15;

  videoGame!: VideogameDb;
  videoPlayer!: HTMLVideoElement;
  isFavorite: boolean = false;
  isPlaylater: boolean = false;
  isIssue: boolean = false;
  constructor(private route: ActivatedRoute, private router: Router, private querygamesservice: QuerygamesService, private location: Location,
    private misterSignalr: MisterSignalrService,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLaptop, cilBug, cilSettings, cilLink, cilPen, ...brandSet };
  }

  ngOnDestroy(): void {
    this.subQueryVideoGames?.unsubscribe();
    this.subQueryVideoGamePlaylist?.unsubscribe();
    this.subQueryGameSameCategory?.unsubscribe();
    this.subQueryGameSameYear?.unsubscribe();
    this.subQueryGameSameEditor?.unsubscribe();
    this.subQueryVideoGameLaunch?.unsubscribe();
    this.subQueryGameSameCollection?.unsubscribe();
    this.subUpdateCache?.unsubscribe();
  }

  ngOnInit(): void {
    //this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.route.params.subscribe(
      params => {
        const id = params['id'];
        this.id = id
        console.log(this.id);
        this.subQueryVideoGames = this.querygamesservice.getVideoGameDetail(this.id).pipe().subscribe((g: VideogameDb) => {
          this.videoGame = g;
          this.UpdatePlaylistState(this.videoGame);

          this.SearchGameSameCategory(g);
          this.SearchGameSameYear(g);
          this.SearchGameSameEditor(g);
          this.SearchGameSameCollection(g);


          console.log(this.videoGame);
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

  private SearchGameSameCategory(game: VideogameDb): void {
    let search: VideogameSearchRequest =
    {
      systems: [game.systemid ],      
      limit: this.nbgames,
      pagesize: this.nbgames,
      gameType: game.gametype,
      gamesExcluded: [game._id],
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryGameSameCategory = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.gamesSameCategory = gsr;
      //console.log(this.gamesSameCategory);
    });
  }
  private SearchGameSameYear(game: VideogameDb): void {
    let search: VideogameSearchRequest =
    {
      systems: [game.systemid],
      pagesize: this.nbgames,
      limit: this.nbgames,
      gamesExcluded: [game._id],
      year: game.year,     
      gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryGameSameYear = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.gamesSameYear = gsr;
      //console.log(this.gamesSameCategory);
    });
  }
  private SearchGameSameEditor(game: VideogameDb): void {
    let search: VideogameSearchRequest =
    {
      systems: [game.systemid],
      pagesize: this.nbgames,
      limit: this.nbgames,
      gamesExcluded: [game._id],
      developname: game.developname,
      gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
      sortFields: [{
        field: "rating",
        isascending: false
      }]

    }
    this.subQueryGameSameEditor = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
      this.gamesSameEditor = gsr;
      //console.log(this.gamesSameCategory);
    });
  }

  private SearchGameSameCollection(game: VideogameDb): void {
    if (game.collectionId != null && game.collectionId > 0) {


      let search: VideogameSearchRequest =
      {
        pagesize: this.nbgames,
        gamesExcluded: [game._id],
        limit: this.nbgames,
        collection: game.collection,
        gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
        sortFields: [{
          field: "rating",
          isascending: false
        }],

      }
      this.subQueryGameSameCollection = this.querygamesservice.getVideoGames(search).pipe().subscribe((gsr: VideogameSearchResult) => {
        this.gamesSameCollection = gsr;
        console.log("collection result");
        console.log(this.gamesSameCollection);
      });
    }
    
  }


  private UpdatePlaylistState(g: VideogameDb): void {
    if (!g.playlist) {
      this.isFavorite = false;
      this.isPlaylater = false;
      this; this.isIssue = false;
    }
    else {
      this.isFavorite = g.playlist.indexOf('favorite') > -1;
      this.isPlaylater = g.playlist.indexOf('playlater') > -1;
      this.isIssue = g.playlist.indexOf('issue') > -1;

      //console.log("IsFavorite : " + g.gameDb.playlist.indexOf('favorite'));
      console.log("playlist : " + g.playlist);

    }
  }


  setPlaylist(playlist: string, add: boolean): void {
    this.subQueryVideoGamePlaylist = this.querygamesservice.SetVideoGamePlaylist(this.id, playlist, add).pipe().subscribe((g: VideogameDb) => {
      this.videoGame = g;
      this.UpdatePlaylistState(this.videoGame);

      console.log(`Update Playlist : ${this.videoGame.playlist}`);
    });
  }

  @ViewChild('videoPlayer')
  set mainVideoEl(el: ElementRef) {
   
      this.videoPlayer = el?.nativeElement;
    
  }

  toggleVideo(event: any) {
    this.videoPlayer.play();
  }
  

  Refresh(event: any) {
    console.log(`Receive event ${event}`);
    this.modalSettingsVisible = false;
    this.subQueryVideoGames = this.querygamesservice.getVideoGameDetail(this.id).pipe().subscribe((g: VideogameDb) => {     
      this.videoGame = g;
      this.UpdatePlaylistState(this.videoGame);

      this.SearchGameSameCategory(g);
      this.SearchGameSameYear(g);
      this.SearchGameSameEditor(g);
      this.SearchGameSameCollection(g);


      console.log(this.videoGame);
    });

  }

  toggleLiveDemo() {
    this.modalSettingsVisible = !this.modalSettingsVisible;
  }

  goSearch(filter: string) {
    var request: VideogameSearchRequest | undefined = undefined
    switch (filter) {
      case "collection":
        request = {          
          pagesize: this.nbgames,          
          collection: this.videoGame.collection,
          sortFields: [{
          field: "rating",
          isascending: false
            }]
        }
        break;
      case "editor":
        request = {
          systems: [this.videoGame.systemid],
          pagesize: this.nbgames,
          developname: this.videoGame.developname,
          sortFields: [{
            field: "rating",
            isascending: false
          }]
        }
        break;
      case "year":
        request = {
          systems: [this.videoGame.systemid],
          pagesize: this.nbgames,
          year: this.videoGame.year,
          gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
          sortFields: [{
            field: "rating",
            isascending: false
          }],
        };
        break;
      case "category":
        request = {
          systems: [this.videoGame.systemid],
          pagesize: this.nbgames,
          gameType: this.videoGame.gametype,
          gamesExcluded: [this.videoGame._id],
          sortFields: [{
            field: "rating",
            isascending: false
          }],
        };
        break;
    }
    if (request != undefined) {
      this.querygamesservice.setCurrentVideoGameSearchRequest(request);
      console.debug(request);
      this.router.navigateByUrl("/videogames/search");
    }



  }

}

