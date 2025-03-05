import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common'
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, flagSet, freeSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLaptop, cilBug } from '@coreui/icons';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { QuerygamesService } from '../../../services/querygames.service';
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
import { Subscription } from 'rxjs';
import { GameResult } from '../../../services/models/game-result';
import { GameAction } from '../../../services/models/game-action';
import { GameSearchResult } from '../../../services/models/game-search-result';
import { GameSearch } from '../../../services/models/game-search';

@Component({
  selector: 'app-game-detail',
  templateUrl: './game-detail.component.html',
  styleUrl: './game-detail.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, ContainerComponent, ProgressComponent, ProgressBarComponent, TableModule, UtilitiesModule, GridModule,
    ThemeDirective, CarouselComponent, CarouselInnerComponent, CarouselItemComponent, CarouselControlComponent, RouterLink, CarouselIndicatorsComponent, ImgModule, IconDirective,
    Tabs2Module],
  providers: [QuerygamesService, IconSetService]
})
export class GameDetailComponent implements OnInit, OnDestroy {
  id: string = '';
  subQueryGames!: Subscription;
  subQueryPlaylist!: Subscription;

  subQueryGameSameCategory!: Subscription
  public gamesSameCategory!: GameSearchResult;

  subQueryGameSameYear!: Subscription
  public gamesSameYear!: GameSearchResult;

  subQueryGameSameEditor!: Subscription
  public gamesSameEditor!: GameSearchResult;

  nbgames: number = 15;

  game!: GameResult;
  videoPlayer!: HTMLVideoElement;
  isFavorite: boolean = false;
  isPlaylater: boolean = false;
  isIssue: boolean = false;

 
  constructor(private route: ActivatedRoute, private router: Router, private querygamesservice: QuerygamesService, private location: Location,
    public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, cilLaptop, cilBug,...brandSet }; 
  }

    ngOnDestroy(): void {
      this.subQueryGames?.unsubscribe();
      this.subQueryPlaylist?.unsubscribe();
      this.subQueryGameSameCategory?.unsubscribe();
      this.subQueryGameSameYear?.unsubscribe();
      this.subQueryGameSameEditor?.unsubscribe();
    }

  ngOnInit(): void {
    this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    this.route.queryParams.subscribe(params => {

      const id = this.route.snapshot.params['id'];
      this.id = id
      console.log(this.id);
      this.subQueryGames = this.querygamesservice.getGameDetail(this.id).pipe().subscribe((g: GameResult) => {
        this.game = g;
        this.UpdatePlaylistState(this.game);

        this.SearchGameSameCategory(g);
        this.SearchGameSameYear(g);
        this.SearchGameSameEditor(g);


        console.log(this.game);
      });

    });
  }
    goBack() {
      this.location.back();
  }

  private SearchGameSameCategory(game: GameResult): void {
    let search: GameSearch =
    {
      systemId: game.gameDb.systemid,
      matchscreenscraper: true,
      limit: this.nbgames,
      gameType: [game.gameDb.gametype[0]],
      gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
      gamesExcluded: [game.gameDb._id],
      sortFields: [{
        field: "rating",
        isascending: false
      }],
      
    }
    this.subQueryGameSameCategory = this.querygamesservice.getGames(search).pipe().subscribe((gsr: GameSearchResult) => {
      this.gamesSameCategory = gsr;
      //console.log(this.gamesSameCategory);
    });
  }
  private SearchGameSameYear(game: GameResult): void {
    let search: GameSearch =
    {
      systemId: game.gameDb.systemid,
      matchscreenscraper: true,
      limit: this.nbgames,
      gamesExcluded: [game.gameDb._id],
      yearMin: game.gameDb.year,
      yearMax: game.gameDb.year,
      gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryGameSameYear = this.querygamesservice.getGames(search).pipe().subscribe((gsr: GameSearchResult) => {
      this.gamesSameYear = gsr;
      //console.log(this.gamesSameCategory);
    });
  }
  private SearchGameSameEditor(game: GameResult): void {
    let search: GameSearch =
    {
      systemId: game.gameDb.systemid,
      matchscreenscraper: true,
      limit: this.nbgames,
      gamesExcluded: [game.gameDb._id],
      developname: game.gameDb.developname,
      gameTypeExcluded: ["Sport / Golf", "Sport / Baseball", "Sport / Football Américain", "Sport / Hockey"],
      sortFields: [{
        field: "rating",
        isascending: false
      }],

    }
    this.subQueryGameSameEditor = this.querygamesservice.getGames(search).pipe().subscribe((gsr: GameSearchResult) => {
      this.gamesSameEditor = gsr;
      //console.log(this.gamesSameCategory);
    });
  }


  private UpdatePlaylistState(g: GameResult): void {
    if (!g.gameDb.playlist) {
      this.isFavorite = false;
      this.isPlaylater = false;
      this; this.isIssue = false;
    }
    else {
      this.isFavorite = g.gameDb.playlist.indexOf('favorite') > -1;
      this.isPlaylater = g.gameDb.playlist.indexOf('playlater') > -1;
      this.isIssue = g.gameDb.playlist.indexOf('issue') > -1;

      //console.log("IsFavorite : " + g.gameDb.playlist.indexOf('favorite'));
      console.log("playlist : " + g.gameDb.playlist);

    }
  }


  execAction(action: GameAction): void {
    //alert(`Action  ${action.category}/${action.name}\r\n${action.path}\r\n${action.parameters["Path"]}`);
    var isok = this.querygamesservice.checkAction(action);

    //alert(JSON.stringify(isok, null, 4))

    if (!isok) {
      alert(`Launch game ${action.parameters["Path"]} failed`);
    }
    else {
      alert(`Launch game ${action.parameters["Path"]} Succeed`);
    }
  }

  setPlaylist(playlist: string, add: boolean): void {
    this.subQueryPlaylist = this.querygamesservice.SetPlaylist(this.id, playlist, add).pipe().subscribe((g: GameResult) => {
      this.game = g;
      this.UpdatePlaylistState(this.game);

      console.log(`Update Playlist : ${this.game.gameDb.playlist}`);
    });
  }

  @ViewChild('videoPlayer')
  set mainVideoEl(el: ElementRef) {
    this.videoPlayer = el.nativeElement;
  }

  toggleVideo(event: any) {
    this.videoPlayer.play();
  }

    }


