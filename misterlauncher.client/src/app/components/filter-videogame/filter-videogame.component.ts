import { Component, Input, OnDestroy, OnInit, signal, Output, EventEmitter, OnChanges, SimpleChanges, ElementRef } from '@angular/core';
import { Observable, Subscription, timer } from 'rxjs'
import { CommonModule, NgStyle, Location, DatePipe, AsyncPipe } from '@angular/common';
import { cilPen, cilDelete, cilNoteAdd, cilClearAll, cilListNumbered, cilFilter, cilSearch, cilFilterX } from '@coreui/icons';
import { IconSetService, IconDirective } from '@coreui/icons-angular';
import { formatDate } from '@angular/common';

import { QuerygamesService } from '../../services/querygames.service';
import { VideogameSearchRequest } from '../../services/models/videogame-search-request';

import {
  BorderDirective,
  BadgeModule, BadgeComponent,
  Tabs2Module,
  ContainerComponent,
  ButtonDirective,
  TableDirective, TableModule,
  CardBodyComponent, CardComponent, CardFooterComponent, CardGroupComponent, CardHeaderComponent, CardImgDirective, CardLinkDirective, CardSubtitleDirective, CardTextDirective, CardTitleDirective,
  ColComponent, RowComponent,
  GutterDirective,  
  ListGroupDirective, ListGroupItemDirective,
  TextColorDirective,
  CollapseDirective,
  FormSelectDirective,
  FormLabelDirective,
  FormTextDirective,
  AlertComponent,
  SpinnerComponent,
  ProgressComponent,
  ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent, ButtonCloseDirective,
  FormDirective, FormControlDirective, FormCheckComponent, FormCheckLabelDirective, FormCheckInputDirective,
  ButtonGroupComponent, ButtonModule,
  InputGroupComponent, InputGroupTextDirective,
  AlertModule
} from '@coreui/angular';
import { FilterOption } from '../../services/models/filter-option';
import { SortField } from '../../services/models/sort-field';
import { FieldSort } from '../../services/models/field-sort';
import { SystemDb } from '../../services/models/system-db';
import { ReactiveFormsModule, FormsModule, FormGroup, FormControl, FormBuilder } from '@angular/forms';
import { SystemSearchRequest } from '../../services/models/system-search-request';
import { SystemSearchResult } from '../../services/models/system-search-result';
import { ItemCount } from '../../services/models/item-count';


@Component({
  selector: 'app-filter-videogame',
  templateUrl: './filter-videogame.component.html',
  styleUrl: './filter-videogame.component.scss',
  standalone: true,
  imports: [
    BorderDirective,
    BadgeModule,
    BadgeComponent,
    Tabs2Module,
    CommonModule,
    ContainerComponent,
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
    RowComponent,
    TextColorDirective,
    IconDirective,
    CollapseDirective,
    FormSelectDirective,
    ProgressComponent,
    AlertComponent,
    AsyncPipe,
    AlertModule,
    SpinnerComponent,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent, ButtonCloseDirective,
    FormLabelDirective, FormTextDirective, FormDirective, ReactiveFormsModule, FormsModule,
    FormDirective, FormSelectDirective, FormControlDirective, FormCheckComponent, FormCheckLabelDirective, FormCheckInputDirective,
    InputGroupComponent, InputGroupTextDirective,
    ButtonGroupComponent, ButtonModule
  ],
  providers: [IconSetService]
})
export class FilterVideogameComponent implements OnInit, OnDestroy, OnChanges {

  
  @Input() ResponseFilterOption?: FilterOption;
  @Output() NewSearchRequest = new EventEmitter<VideogameSearchRequest>();
  @Output() OnReset = new EventEmitter<string>();

  @Input() searching: Boolean = false;
  debug: boolean = false;

  SearchRequest: VideogameSearchRequest = {}
  allSystems: SystemDb[] = []

  getSystemSub?: Subscription;

  avalaibleCategory: FilterValue[] = []

  avalaibleSystems: FilterValueSeleted[] = []
  avalaibleConsoles: FilterValueSeleted[] = []

  avalaibleGameType: FilterValueSeleted[] = []
  avalaiblePlaylist: FilterValue[] = []

  hasCollectionFilter: Boolean = false;
  hasDevelopperFilter: Boolean = false;
  hasEditorFilter: Boolean = false;

  avalaibleSortField: FilterValue[] = [
    { label: "Name", value: 'name' },
    { label: "Rating", value: 'rating' },
    { label: "Year", value: 'year' },
    { label: "Editor", value: 'editorname' },
    { label: "Developer", value: 'developname' },
    { label: "Played", value: 'playedhit' },
    { label: "Last Played", value: 'playedlast' },

  ];

  avalaibleLimit: FilterValue[] = [
    { label: "20", value: 20 },
    { label: "50", value: 50 },
    { label: "100", value: 100 }   
  ]

  
  
  defaultgametype: FilterValue[] = [
    { label: "Action", value: "Action" }
    , { label: "Adulte", value: "Adulte" }
    , { label: "Aventure", value: "Aventure" }
    , { label: "Beat'em All", value: "Beat'em All" }
    , { label: "Casino", value: "Casino" }
    , { label: "Casual Game", value: "Casual Game" }
    , { label: "Chasse et Peche", value: "Chasse et Peche" }
    , { label: "Combat", value: "Combat" }
    , { label: "Compilation", value: "Compilation" }
    , { label: "Course, Conduite", value: "Course, Conduite" }
    , { label: "Demo", value: "Demo" }
    , { label: "Divers", value: "Divers" }
    , { label: "Flipper", value: "Flipper" }
    , { label: "Jeu de cartes", value: "Jeu de cartes" }
    , { label: "Jeu de rôles", value: "Jeu de rôles" }
    , { label: "Jeu de societe / plateau", value: "Jeu de societe / plateau" }
    , { label: "Jeu de societe asiatique", value: "Jeu de societe asiatique" }
    , { label: "Ludo-Educatif", value: "Ludo-Educatif" }
    , { label: "Musique et Danse", value: "Musique et Danse" }
    , { label: "N/A", value: "N/A" }
    , { label: "Plateforme", value: "Plateforme" }
    , { label: "Puzzle-Game", value: "Puzzle-Game" }
    , { label: "Quiz", value: "Quiz" }
    , { label: "Réflexion", value: "Réflexion" }
    , { label: "Shoot'em Up", value: "Shoot'em Up" }
    , { label: "Simulation", value: "Simulation" }
    , { label: "Sport", value: "Sport" }
    , { label: "Sport avec animaux", value: "Sport avec animaux" }
    , { label: "Stratégie", value: "Stratégie" }
    , { label: "Tir", value: "Tir" }
    , { label: "Tir avec accessoire", value: "Tir avec accessoire" }
  ]

  avalaibleRating: FilterValue[] = [
    { label: "Top", value: "top" },
    { label: "Correct", value: "correct" },
    { label: "Bad", value: "bad" },
    { label: "Irrelevant", value: "irrelevant" }
  ]

  avalaibleSortFieldWay: FilterValueSeleted[] = [
    { label: "Asc", value: true, selected:true },
    { label: "Desc", value: false, selected:false }
  ]

  filterItems: FilterItem[] = [];
  showEditModal: Boolean = false;
  editModalTitle: string = "Add"
  editModalMode: string = "add"

  sortWayvalue: boolean = true;

  formSearchVideoGame! : FormGroup

  editListValue: EditListValue = { label:"" , options: [], items:[], category:"", selectedvalue:""  }
  multiEditListValue: MultiEditListValue = { label:"", options: [], items: [], category:"", selectedvalue: []}
  constructor(
    private querygamesservice: QuerygamesService,
    public iconSet: IconSetService,
    private _elementRef: ElementRef,
    private formBuilder: FormBuilder
    ) {
    iconSet.icons = {
      cilPen, cilDelete, cilNoteAdd, cilClearAll, cilListNumbered, cilFilter, cilSearch, cilFilterX
    };
  }
    ngOnChanges(changes: SimpleChanges): void {
      this.SearchRequest = this.querygamesservice.getCurrentVideoGameSearchRequest();
      if (this.SearchRequest.page )
      //console.log("(onchange)")
      //console.log(this.SearchRequest);
      //console.log(this.ResponseFilterOption);
      this.SetFilter();
      this.SetAvalaibleFilterFValue()

    }
  ngOnInit(): void {
    this.SearchRequest = this.querygamesservice.getCurrentVideoGameSearchRequest();
    this.allSystems = this.querygamesservice.GetAllSystemFromCache();

    //console.log("== On init ==");
    //console.log(this.SearchRequest);
    //console.log(`System load from cache : ${this.allSystems.length}`);

    if (this.searchisEmpty(this.SearchRequest)) {
      this.SearchRequest.sortFields = [{ field: "rating", isascending: false }]
      this.SearchRequest.sortFields = [{ field: "playedlast", isascending: false }]
      this.SearchRequest.pagesize = 20;
      this.SearchRequest.playedhitMin = 1;
    }

    if (this.SearchRequest?.sortFields == undefined) {
      this.SearchRequest.sortFields = [{ field: "rating", isascending: false}]
    }
    if (this.SearchRequest?.pagesize == undefined) {
      this.SearchRequest.pagesize = 20;
    }

    this.querygamesservice.refreshDefaultSearchVideoGameFilter();
    
   

    this.formSearchVideoGame = this.formBuilder.group(
      {
        search: [this.SearchRequest?.name]
      });

    this.SetFilter();
    this.SetAvalaibleFilterFValue();
    this.updateSearchRequest(false);
   
    // retrieve GameType, System, GameExcluded
  
      
    }
    ngOnDestroy(): void {
      this.getSystemSub?.unsubscribe;


  } 

  SetFilter(): void {
    if (this.SearchRequest == undefined) {
      //console.log("this Search Request is undefined")
      this.filterItems = [];
    }

    this.formSearchVideoGame.controls['search'].setValue(this.SearchRequest.name == undefined ? "" : this.SearchRequest.name);
    

    this.filterItems = [];
    if (this.SearchRequest?.systemCategory != undefined) {
      let item: FilterItem = { label: "Category", color: "secondary", value: { label: this.SearchRequest.systemCategory, value: this.SearchRequest.systemCategory }, category: "systemCategory" };     
      this.filterItems.push(item);
    }    
    if (this.SearchRequest?.year != undefined) {
      let item: FilterItem = { label: "Year", color: "secondary", value: { label: this.SearchRequest.year.toString(), value: this.SearchRequest.year }, category: "year" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.yearMin != undefined) {
      let item: FilterItem = { label: "Year", color: "secondary", value: { label: `>=${this.SearchRequest.yearMin}`, value: this.SearchRequest.yearMin }, category: "yearMin" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.yearMax != undefined) {
      let item: FilterItem = { label: "Year", color: "secondary", value: { label: `<=${this.SearchRequest.yearMax}`, value: this.SearchRequest.yearMax }, category: "yearMax" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.allowUnknowYear != undefined) {
      let item: FilterItem = { label: "Allow unknow year", color: "secondary", value: { label: this.SearchRequest?.allowUnknowYear ? "yes" : "no", value: this.SearchRequest.allowUnknowYear }, category: "allowUnknowYear" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.allowUnRated != undefined) {
      let item: FilterItem = { label: "Allow unknow Rating", color: "secondary", value: { label: this.SearchRequest?.allowUnRated ? "yes" : "no", value: this.SearchRequest.allowUnRated }, category: "allowUnRated" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.playlist != undefined) {
      let item: FilterItem = { label: "Playlist", color: "secondary", value: { label: this.SearchRequest?.playlist, value: this.SearchRequest.playlist }, category: "playlist" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.minRating != undefined) {
      let item: FilterItem = { label: "Rating", color: "secondary", value: { label: `>=${this.SearchRequest?.minRating}`, value: this.SearchRequest.minRating }, category: "minRating" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.maxRating != undefined) {
      let item: FilterItem = { label: "Rating", color: "secondary", value: { label: `<=${this.SearchRequest?.maxRating}`, value: this.SearchRequest.maxRating }, category: "maxRating" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.editor != undefined) {
      let item: FilterItem = { label: "Editor", color: "secondary", value: { label: this.SearchRequest?.editor, value: this.SearchRequest.editor }, category: "editor" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.developname != undefined) {
      let item: FilterItem = { label: "Developer", color: "secondary", value: { label: this.SearchRequest?.developname, value: this.SearchRequest.developname }, category: "developname" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.gameType != undefined) {
      let item: FilterItem = { label: "Type", color: "secondary", value: { label: this.SearchRequest?.gameType.join(','), value: this.SearchRequest?.gameType }, category: "gameType" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.gameTypeExcluded != undefined) {
      let item: FilterItem = { label: "Excluded Type", color: "secondary", value: { label: this.SearchRequest?.gameTypeExcluded.join(','), value: this.SearchRequest?.gameTypeExcluded }, category: "gameTypeExcluded" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.collection != undefined) {
      let item: FilterItem = { label: "Collection", color: "secondary", value: { label: this.SearchRequest?.collection, value: this.SearchRequest?.collection }, category: "collection" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.core != undefined) {
      let item: FilterItem = { label: "Core", color: "secondary", value: { label: this.SearchRequest?.core, value: this.SearchRequest?.core }, category: "core" };
      this.filterItems.push(item);
    }
   
    if (this.SearchRequest?.systems != undefined) {
      let arcadesystemvalue = "";
      let consolesystemvalue = "";

      this.SearchRequest?.systems.forEach((s: string) => {
        if (this.allSystems.length > 0) {
          let system = this.allSystems.find((sys: SystemDb) => sys._id == s);
          if (system != undefined) {
            switch (system.category) {
              case "Arcade":
                arcadesystemvalue += (arcadesystemvalue == "" ? "" : ",") + system.name;
                break;
              case "Console":
                consolesystemvalue += (consolesystemvalue == "" ? "" : ",") + system.name;
            }
          }
        }
        else {
          //console.log("All system is empty...");
        }
      });
      if (arcadesystemvalue != "") {
        let itemarcade: FilterItem = { label: "Arcade", color: "secondary", value: { label: arcadesystemvalue, value: arcadesystemvalue }, category: "systems" };
        this.filterItems.push(itemarcade);
      }
      if (consolesystemvalue != "") {
        let itemconsole: FilterItem = { label: "Console", color: "secondary", value: { label: consolesystemvalue, value: consolesystemvalue }, category: "systems" };
        this.filterItems.push(itemconsole);
      }
      //console.log(`(SetFilter) Console:${consolesystemvalue} - Arcade: ${arcadesystemvalue}`);

    }

    if (this.SearchRequest?.systemsExcluded != undefined) {
      let arcadesystemvalue = "";
      let consolesystemvalue = "";
      this.SearchRequest?.systemsExcluded.forEach((s: string) => {
        if (this.allSystems.length > 0) {
          let system = this.allSystems.find((sys: SystemDb) => sys._id == s);
          if (system != undefined) {
            switch (system.category) {
              case "Arcade":
                arcadesystemvalue += (arcadesystemvalue == "" ? "" : ",") + system.name;
                break;
              case "Console":
                consolesystemvalue += (consolesystemvalue == "" ? "" : ",") + system.name;
            }
          }
        }
      });
      if (arcadesystemvalue != "") {
        let item: FilterItem = { label: "!Arcade", color: "secondary", value: { label: arcadesystemvalue, value: arcadesystemvalue }, category: "systemsExcluded" };
        this.filterItems.push(item);
      }
      if (consolesystemvalue != "") {
        let item: FilterItem = { label: "!Console", color: "secondary", value: { label: consolesystemvalue, value: consolesystemvalue }, category: "systemsExcluded" };
        this.filterItems.push(item);
      }
    }

    if (this.SearchRequest?.playedhitMin != undefined || this.SearchRequest?.playedhitMax != undefined) {;
      let label = "";
      if (this.SearchRequest?.playedhitMin == undefined)
      {
        if (this.SearchRequest?.playedhitMax == 0) {
          label = "never";
        }
        if (this.SearchRequest?.playedhitMax == 1) {
          label = "once";
        }
      }
      if (this.SearchRequest?.playedhitMax == undefined) {
        if (this.SearchRequest?.playedhitMin == 1) {
          label = "least one";
        }
        if (this.SearchRequest?.playedhitMin == 10) {
          label = "+ 10 times";
        }
      }
      if (label == "") {

        if (this.SearchRequest?.playedhitMin == undefined) {
          label = "<=" + this.SearchRequest?.playedhitMax?.toString();
        }
        if (this.SearchRequest?.playedhitMax == undefined) {
          label = ">=" + this.SearchRequest?.playedhitMin?.toString();
        }
        if (this.SearchRequest?.playedhitMin != undefined && this.SearchRequest?.playedhitMax != undefined) {
          label = `[${this.SearchRequest?.playedhitMin}..${this.SearchRequest?.playedhitMax}]`;
        }
      }

      let item: FilterItem = { label: "Play times", color: "secondary", value: { label:label, value: label }, category: "playedhit" };
      this.filterItems.push(item);
    }
    if (this.SearchRequest?.playedlastMin != undefined || this.SearchRequest?.playedlastMax != undefined) {
      
      let label = "";
      if (this.SearchRequest?.playedhitMin == undefined && this.SearchRequest.playedlastMax !=undefined) {
        label = "before " + `${this.SearchRequest.playedlastMax.getDate()}/${this.SearchRequest.playedlastMax.getMonth()+1}/${this.SearchRequest.playedlastMax.getFullYear() }`;
        this.SearchRequest?.playedhitMax?.toString();
      }
      if (this.SearchRequest?.playedhitMax == undefined && this.SearchRequest.playedlastMin != undefined) {
        label = "from " + `${this.SearchRequest.playedlastMin.getDate()} /${this.SearchRequest.playedlastMin.getMonth() +1 }/${this.SearchRequest.playedlastMin.getFullYear()}`;
      }
      if (this.SearchRequest?.playedlastMin != undefined && this.SearchRequest?.playedlastMax != undefined) {
        label = `between ${this.SearchRequest.playedlastMin.getDate()} /${this.SearchRequest.playedlastMin.getMonth() + 1}/${this.SearchRequest.playedlastMin.getFullYear()} and ${this.SearchRequest.playedlastMax.getDate()}/${this.SearchRequest.playedlastMax.getMonth()+1}/${this.SearchRequest.playedlastMax.getFullYear() }`;
      }

      let item: FilterItem = { label: "Played", color: "secondary", value: { label: label, value: label }, category: "playedlast" };
      this.filterItems.push(item);
    }

   

    if (this.SearchRequest?.pagesize != undefined) {
      let item: FilterItem = { label: "Limit", color: "success", value: { label: this.SearchRequest?.pagesize.toString(), value: this.SearchRequest?.pagesize }, category: "limit" };
      this.filterItems.push(item);
    }

    if (this.SearchRequest?.sortFields != undefined) {
      this.SearchRequest?.sortFields.forEach((s: FieldSort) => {
        let item: FilterItem = { label: s.field, color: "warning", value: { label: s.isascending ? "Asc" : "Desc", value: s.isascending }, category: "sortfield_" + s.field  };
        this.filterItems.push(item);
      });     
    }    
    this.querygamesservice.setCurrentVideoGameSearchRequest(this.SearchRequest);


    
    
  }

  onSubmitGame() {
    this.SearchRequest.name = this.formSearchVideoGame.value.search;    
    if (!this.updateSearchRequest()) {
      this.OnReset.emit("clear");
    }
  }

  SetAvalaibleFilterFValue() {

    // CHECK if all systems are loaded
   // console.log(`(update avalaible filter) allsystems.count = ${this.allSystems.length}`)
    if (this.allSystems.length == 0) {
      this.allSystems = this.querygamesservice.GetAllSystemFromCache();      
    }
    
    // SYSTEMCATEGORY
    this.avalaibleCategory = []
    this.avalaibleCategory.push({ label:"Arcade", value:"Arcade" });
    this.avalaibleCategory.push({ label:"Console", value:"Console" });
    // SYSTEMS
    this.avalaibleSystems = []
    this.avalaibleConsoles = []

    if ((this.ResponseFilterOption != undefined) && (this.ResponseFilterOption.systemDbs.length > 0)) {
      
      
      this.ResponseFilterOption.systemDbs.forEach((s: SystemDb) => {
          if (s.category == "Arcade") {
            this.avalaibleSystems.push({ label: s.name, value: s._id, selected: this.SearchRequest?.systems?.find((f: string) => f == s._id) != undefined });
          }
          else {
            this.avalaibleConsoles.push({ label: s.name, value: s._id, selected: this.SearchRequest?.systems?.find((f: string) => f == s._id) != undefined });
          }
        });
      }
      else {
        //console.log ("Load Console & system")
        this.allSystems.forEach((s: SystemDb) => {
          if (s.category == "Arcade") {
            this.avalaibleSystems.push({ label: s.name, value: s._id, selected: this.SearchRequest?.systems?.find((f: string) => f == s._id) != undefined });
          }
          else {
            this.avalaibleConsoles.push({ label: s.name, value: s._id, selected: this.SearchRequest?.systems?.find((f: string) => f == s._id) != undefined });
          }
        });
      }
      
    
    this.avalaibleGameType = []
    let selectedGametype: string[] = []
    if (this.SearchRequest?.gameType != undefined) {
      this.SearchRequest.gameType.forEach((s: string) => selectedGametype.push(s));
    }
    if (this.SearchRequest?.gameTypeExcluded != undefined) {
      this.SearchRequest.gameTypeExcluded.forEach((s: string) => selectedGametype.push(s));
    }

    if (this.ResponseFilterOption != undefined) {
      this.ResponseFilterOption.gameTypes.forEach((s: string) => {
        this.avalaibleGameType.push({ label: s, value: s, selected: selectedGametype.includes(s) });
      });
    }
    else {      
      this.defaultgametype.forEach((f: FilterValue) => {
        this.avalaibleGameType.push({ label: f.label, value: f.value, selected: selectedGametype.includes(f.value) });
      });
      
    }

    this.avalaiblePlaylist = [{ label: "Favorite", value: "favorite" }, { label: "Play later", value: "playlater" }, { label: "Issue", value: "issue" }];

    this.hasDevelopperFilter = this.ResponseFilterOption?.developers != undefined && this.ResponseFilterOption.developers.length > 0;
    this.hasCollectionFilter = this.ResponseFilterOption?.collections != undefined && this.ResponseFilterOption.collections.length > 0;
    this.hasEditorFilter = this.ResponseFilterOption?.editors != undefined && this.ResponseFilterOption.editors.length > 0;    

  }



  DeleteFilter(category: string, refresh: boolean = true) : void {
    if (this.SearchRequest == undefined) {
      return;

    }   

    switch (category) {
      case "systemCategory":
        this.SearchRequest.systemCategory = undefined;        
        break;
      case "year":
        this.SearchRequest.year = undefined;
        break;
      case "yearMin":
        this.SearchRequest.yearMin = undefined;
        break;
      case "yearMax":
        this.SearchRequest.yearMax = undefined;
        break;
      case "allowUnknowYear":
        this.SearchRequest.allowUnknowYear = undefined;
        break;
      case "allowUnRated":
        this.SearchRequest.allowUnRated = undefined;
        break;
      case "playlist":
        this.SearchRequest.playlist = undefined;
        break;
      case "minRating":
        this.SearchRequest.minRating = undefined;
        break;
      case "maxRating":
        this.SearchRequest.maxRating = undefined;
        break;
      case "editor":
        this.SearchRequest.editor = undefined;
        break;
      case "developname":
        this.SearchRequest.developname = undefined;
        break;
      case "gameType":
        this.SearchRequest.gameType = undefined;
        break;
      case "gameTypeExcluded":
        this.SearchRequest.gameTypeExcluded = undefined;
        break;
      case "collection":
        this.SearchRequest.collection = undefined;
        break;
      case "core":
        this.SearchRequest.core = undefined;
        break;
      case "systems":
        this.SearchRequest.systems = undefined;
        break;
      case "gamesExcluded":
        this.SearchRequest.gamesExcluded = undefined;
        break;
      case "systemsExcluded":
        this.SearchRequest.systemsExcluded = undefined;
        break;
      case "limit":
        this.SearchRequest.pagesize = undefined;
        break;
      case "playedhitMin":
        this.SearchRequest.playedhitMin = undefined;
        break;
      case "playedhitMax":
        this.SearchRequest.playedhitMax = undefined;
        break;
      case "playedlastMax":
        this.SearchRequest.playedlastMax = undefined;
        break;
      case "playedlastMin":
        this.SearchRequest.playedlastMin = undefined;
        break;
      case "playedhit":
        this.SearchRequest.playedhitMin = undefined;
        this.SearchRequest.playedhitMax = undefined;
        break;
      case "playedlast":
        this.SearchRequest.playedlastMin = undefined;
        this.SearchRequest.playedlastMax = undefined;
        break;


    }

    if (category.startsWith("sortfield_")) {
      let fieldname = category.substring(10);
      if (this.SearchRequest.sortFields != undefined && this.SearchRequest.sortFields.length > 0) {
        this.SearchRequest.sortFields = this.SearchRequest.sortFields.filter((s: FieldSort) => s.field != fieldname);
      }
    }
    this.SetFilter();
    if (refresh) {
      if (!this.updateSearchRequest()) {
        this.OnReset.emit("clear");
      }
    }

  }

  EditFilter(category: string): void {
    if (this.SearchRequest == undefined) {
      return;

    }



    switch (category) {
      case "systemCategory":
        //this.SearchRequest.systemCategory = undefined;
        break;
      case "year":
        //this.SearchRequest.year = undefined;
        break;
      case "yearMin":
        //this.SearchRequest.yearMin = undefined;
        break;
      case "yearMax":
        //this.SearchRequest.yearMax = undefined;
        break;
      case "allowUnknowYear":
        //this.SearchRequest.allowUnknowYear = undefined;
        break;
      case "allowUnRated":
        //this.SearchRequest.allowUnRated = undefined;
        break;
      case "playlist":
        //this.SearchRequest.playlist = undefined;
        break;
      case "minRating":
        //this.SearchRequest.minRating = undefined;
        break;
      case "maxRating":
        //this.SearchRequest.maxRating = undefined;
        break;
      case "editor":
        //this.SearchRequest.editor = undefined;
        break;
      case "developname":
        //this.SearchRequest.developname = undefined;
        break;
      case "gameType":
        //this.SearchRequest.gameType = undefined;
        break;
      case "gameTypeExcluded":
        //this.SearchRequest.gameTypeExcluded = undefined;
        break;
      case "collection":
        //this.SearchRequest.collection = undefined;
        break;
      case "core":
        //this.SearchRequest.core = undefined;
        break;
      case "systemId":
        //this.SearchRequest.systemId = undefined;
        break;
      case "gamesExcluded":
        //this.SearchRequest.gamesExcluded = undefined;
        break;
      case "systemsExcluded":
        //this.SearchRequest.systemsExcluded = undefined;
        break;
      case "limit":
        //this.SearchRequest.limit = undefined;
        break;

    }

    if (category.startsWith("sortfield_")) {
      let fieldname = category.substring(10);
      //if (this.SearchRequest.sortFields != undefined && this.SearchRequest.sortFields.length > 0) {
      //  this.SearchRequest.sortFields = this.SearchRequest.sortFields.filter((s: FieldSort) => s.field != fieldname);
      //}
      //this.EditModalMode('sort');
    }

    

   
    this.SetFilter();
    //this.NewSearchRequest.emit(this.SearchRequest);

  }

  ClearFilter(): void {
    this.SearchRequest = {};
    if (this.SearchRequest?.sortFields == undefined) {
      this.SearchRequest.sortFields = [{ field: "rating", isascending: false }]
    }
    if (this.SearchRequest?.pagesize == undefined) {
      this.SearchRequest.pagesize = 20;
    }

    this.SetFilter();
    this.OnReset.emit("clear");
    

  }

  AddFilterString(category: string, value: string): void {
    if (this.SearchRequest == undefined) {
      return;
    }   
    switch (category) {
      case "systemCategory":
        this.SearchRequest.systemCategory = value;
        break;
      case "playlist":
        this.SearchRequest.playlist = value;
        break;
      case "editor":
        this.SearchRequest.editor = value;
        break;
      case "developname":
        this.SearchRequest.developname = value;
        break;      
      case "collection":
        this.SearchRequest.collection = value;
        break;
      case "core":
        this.SearchRequest.core = value;
        break;      
    }

    this.SetFilter();
  }

  AddFilterStringArray(category: string, value: string[]): void {
    if (this.SearchRequest == undefined) {
      return;
    }
    switch (category) {
      case "gameType":
        this.SearchRequest.gameType = value;
        break;
      case "gameTypeExcluded":
        this.SearchRequest.gameTypeExcluded = value;
        break;
      case "systems":
        this.SearchRequest.systems = value;
        break;
      case "systemsExcluded":
        this.SearchRequest.systemsExcluded = value;
        break;
    }

    this.SetFilter();
  }

  AddFilterNumber(category: string, value: number): void {
    if (this.SearchRequest == undefined) {
      return;
    }
    switch (category) {
      case "year":
        this.SearchRequest.year = value;
        break;
      case "yearMin":
        this.SearchRequest.yearMin = value;
        break;
      case "yearMax":
        this.SearchRequest.yearMax = value;
        break;           
      case "minRating":
        this.SearchRequest.minRating = value;
        break;
      case "maxRating":
        this.SearchRequest.maxRating = value;
        break;      
      case "limit":
        this.SearchRequest.pagesize = value;
        break;
      case "playedhitMin":
        this.SearchRequest.playedhitMin = value;
        break;
      case "playedhitMax":
        this.SearchRequest.playedhitMax = value;
        break;
    }

    this.SetFilter();
  }

  AddFilterBoolean(category: string, value: boolean): void {
    if (this.SearchRequest == undefined) {
      return;
    }
    switch (category) {
      case "allowUnknowYear":
        this.SearchRequest.allowUnknowYear = value;
        break;
      case "allowUnRated":
        this.SearchRequest.allowUnRated = value;
        break;
    }

    this.SetFilter();
  }

  AddFilterDate(category: string, value: Date): void {
    if (this.SearchRequest == undefined) {
      return;
    }
    switch (category) {
      case "playedlastMin":
        this.SearchRequest.playedlastMin = value;
        break;
      case "playedlastMax":
        this.SearchRequest.playedlastMax = value;
        break;
    }

    this.SetFilter();
  }

  toggleVisibility() {
    this.showEditModal = !this.showEditModal;
  }

  handleVisibilityChange(event: any) {
    this.showEditModal = event;
  }

  EditModalAdd(): void {
    this.showEditModal = true;
    this.editModalMode = "add";
    this.editModalTitle = "Add Option";
  }

  EditModalMode(modename: string): void {
    if (modename == "sort") {
      this.showEditModal = true;
      this.editModalMode = "sort";
      this.editModalTitle = "Add Sort";
      this.sortWayvalue = this.avalaibleSortFieldWay.find((i : FilterValueSeleted) => i.selected)?.value;

    }
    if (modename == "limit") {
      this.showEditModal = true;
      this.editModalMode = "limit";
      this.editModalTitle = "Add Limit";
    }
    if (modename == "selectfilter") {
      this.showEditModal = true;
      this.editModalMode = "selectfilter";
      this.editModalTitle = "Add Filter";
      if (this.allSystems.length == 0) {
        this.allSystems = this.querygamesservice.GetAllSystemFromCache();
        if (this.allSystems.length > 0) {
          this.SetAvalaibleFilterFValue();
        }
      }
    }
  } 

  changeSortWay(value: boolean) {
    this.sortWayvalue = value;
  }

  addSort(event: any): void {
    let domFieldElement = this._elementRef.nativeElement.querySelector(`#sortInputField`);
    if (this.SearchRequest.sortFields == undefined || this.SearchRequest.sortFields.length == 0) {
      this.SearchRequest.sortFields = [];
      this.SearchRequest.sortFields.push({ field: domFieldElement.value, isascending: this.sortWayvalue });
    }
    else {
      let found = false;
      this.SearchRequest.sortFields.forEach((s: FieldSort) => {
        if (s.field == domFieldElement.value) {
          s.isascending =this.sortWayvalue;
          found = true
        }
      });
      if (found == false) {
        this.SearchRequest.sortFields.push({ field: domFieldElement.value, isascending: this.sortWayvalue });
      }
    }
    this.SetFilter();
    //this.querygamesservice.setCurrentVideoGameSearchRequest(this.SearchRequest);
    //console.log(this.SearchRequest);

    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";
    
    this.updateSearchRequest();  

  }

  addLimit(event: any): void {
    let domLimitElement = this._elementRef.nativeElement.querySelector(`#sortInputLimit`);
    this.SearchRequest.pagesize = domLimitElement.value;      
    this.SetFilter();
    //this.querygamesservice.setCurrentVideoGameSearchRequest(this.SearchRequest);
    //console.log(this.SearchRequest);

    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";

    this.updateSearchRequest();   

    //console.log(domLimitElement.value);
    //console.log(domFieldWayElement)

  }

  editfilter(category: string, filtername: string): void {
    //console.log(`--editFilter-- category : ${category} filter : ${filtername}`)
    this.editListValue.selectedvalue = "";
    switch (category) {
      case "playlist":
        this.editListValue.label = "Playlist";
        this.editListValue.category = "playlist"
        this.editListValue.items = this.avalaiblePlaylist;
        if (this.SearchRequest?.playlist != undefined) {
          this.editListValue.selectedvalue = this.SearchRequest?.playlist;
        }
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        break;
      case "editor":
        this.editListValue.label = "Editor";
        this.editListValue.category = "editor"
        this.editListValue.items = []
        this.ResponseFilterOption?.editors.forEach((e: string) => this.editListValue.items.push({ label: e, value: e }));

        if (this.SearchRequest?.editor != undefined) {
          this.editListValue.selectedvalue = this.SearchRequest?.editor;
        }
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        break;
      case "developname":
        this.editListValue.label = "Studio";
        this.editListValue.category = "developname"
        this.editListValue.items = []
        this.ResponseFilterOption?.developers.forEach((e: string) => this.editListValue.items.push({ label: e, value: e }));

        if (this.SearchRequest?.developname != undefined) {
          this.editListValue.selectedvalue = this.SearchRequest?.developname;
        }
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        break;
      case "collection":
        this.editListValue.label = "Collection";
        this.editListValue.category = "collection"
        this.editListValue.items = []
        this.ResponseFilterOption?.collections.forEach((e: string) => this.editListValue.items.push({ label: e, value: e}));

        if (this.SearchRequest?.collection != undefined) {
          this.editListValue.selectedvalue = this.SearchRequest?.collection;
        }
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        break;
   
      case "systems":
        this.multiEditListValue.label = "Arcade System";
        this.multiEditListValue.category = "systems"
        this.multiEditListValue.items = this.avalaibleSystems;
        this.multiEditListValue.options = [{ label: "Included", value: "inc", selected: this.SearchRequest?.systems != undefined }, { label: "Excluded", value: "exc", selected: this.SearchRequest?.systemsExcluded != undefined }];        
        this.editModalTitle = "Add Filter";
        this.multiEditListValue.selectedvalue = [];
        this.multiEditListValue.items.forEach((item: FilterValueSeleted) => {
          if (item.selected) {
            this.multiEditListValue.selectedvalue.push(item.value)
          }
        });        
        break;
      case "consoles":
        this.multiEditListValue.label = "Console";
        this.multiEditListValue.category = "consoles"
        this.multiEditListValue.items = this.avalaibleConsoles;
        this.multiEditListValue.options = [{ label: "Included", value: "inc", selected: this.SearchRequest?.systems != undefined }, { label: "Excluded", value: "exc", selected: this.SearchRequest?.systemsExcluded != undefined }];
        this.multiEditListValue.selectedvalue = [];
        this.multiEditListValue.items.forEach((item: FilterValueSeleted) => {
          if (item.selected) {
            this.multiEditListValue.selectedvalue.push(item.value)
          }
        });
        this.editModalTitle = "Add Filter";
        break;
      case "year":
        this.editModalTitle = "Year filter";
        break;
      case "played":
        this.editModalTitle = "Launch";
        break

      case "category":
        this.editListValue.label = "Category";
        this.editListValue.category = "category"
        this.editListValue.items = this.avalaibleCategory;
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        this.multiEditListValue.selectedvalue = [];
        if (this.SearchRequest?.systemCategory != undefined) {
          this.editListValue.selectedvalue = this.SearchRequest?.systemCategory;
        }
        else {
          this.editListValue.selectedvalue = this.editListValue.items[0].value;
        }
        break;
      case "gametype":
        this.multiEditListValue.label = "Game Type";
        this.multiEditListValue.category = "gametype";
        this.multiEditListValue.items = this.avalaibleGameType;
        this.multiEditListValue.options = [{ label: "Included", value: "inc", selected: (this.SearchRequest?.gameType != undefined) }, { label: "Excluded", value: "exc", selected: (this.SearchRequest?.gameTypeExcluded != undefined) }];
        this.multiEditListValue.selectedvalue = [];
        this.multiEditListValue.items.forEach((item: FilterValueSeleted) => {
          if (item.selected) {
            this.multiEditListValue.selectedvalue.push(item.value)
          }
        });
        this.editModalTitle = "Add Filter";
        break;
      case "rating":
        this.editListValue.label = "Rating";
        this.editListValue.category = "rating"
        this.editListValue.items = this.avalaibleRating;
        this.editListValue.selectedvalue = "";
        this.editListValue.options = [];
        this.editModalTitle = "Add Filter";
        let min = this.SearchRequest?.minRating == undefined ? 0 : this.SearchRequest.minRating;
        let max = this.SearchRequest?.maxRating == undefined ? 0 : this.SearchRequest.maxRating;

        this.avalaibleRating.forEach((f: FilterValue) => {
          
            switch (f.value) {
              case "top":
                this.editListValue.selectedvalue = (min >= 16) ? "top" : this.editListValue.selectedvalue;
                break;
              case "correct":
                this.editListValue.selectedvalue = (min == 11 && max == 15) ? "correct" : this.editListValue.selectedvalue;                
                break;
              case "bad":
                this.editListValue.selectedvalue = (min == 6 && max == 10) ? "bad" : this.editListValue.selectedvalue;                
                break;
              case "irrevelant":
                this.editListValue.selectedvalue = (max <= 5) ? "bad" : this.editListValue.selectedvalue;         
                break;
            }                    
        });

    }

    if (filtername == "dropdownlist") {
      this.editModalMode = "editFilterDropdownList";      
    }
    if (filtername == "filteryear") {
      this.editModalMode = "editFilterYear";
    }
    if (filtername == "filterplayed") {
      this.editModalMode = "editFilterPlayed";
    }
    if (filtername == "multiselect") {
      this.editModalMode = "editFilterMultiSelect";
    }

   // console.log(`--editFilter-- editModalMode : ${this.editModalMode} label : ${this.editListValue.label}`)
   // console.log(this.editListValue.items)
  }

  onCheckboxCheck(value: string) {
    //console.log(`(Checkbox) ${value}`)
    if (this.multiEditListValue.selectedvalue.includes(value)) {
      this.multiEditListValue.selectedvalue = this.multiEditListValue.selectedvalue.filter((s: string) => s != value);
    }
    else {
      this.multiEditListValue.selectedvalue.push(value);
    }
    //console.log(this.multiEditListValue.selectedvalue)
  }

  onRadioboxCheck(value: string) {
    this.editListValue.selectedvalue = value
  }

  addfilter(category: string, formOptionId: string) {
    
    let domFielOptionElement = this._elementRef.nativeElement.querySelector(`#${formOptionId}`);

    switch (category) {
      case "playlist":
        this.AddFilterString(category, this.editListValue.selectedvalue);// domFieldElement.value);
        break;     
      case "category":
        this.AddFilterString("systemCategory", this.editListValue.selectedvalue); //domFieldElement.value);
        this.DeleteFilter("systemId", false);
        break;
      case "editor":
        this.AddFilterString("editor", this.editListValue.selectedvalue); //domFieldElement.value);
        break;
      case "collection":
        this.AddFilterString("collection", this.editListValue.selectedvalue); //domFieldElement.value);;
        break;
      case "developname" :
        this.AddFilterString("developname", this.editListValue.selectedvalue); //domFieldElement.value);;
        break;
      case "rating":
        switch (this.editListValue.selectedvalue) {
          case "top":
            this.AddFilterNumber("minRating", 16);
            this.DeleteFilter("maxRating", false);
            break;
          case "correct":
            this.AddFilterNumber("minRating", 11);
            this.AddFilterNumber("maxRating", 15);
            break;
          case "bad":
            this.AddFilterNumber("minRating", 6);
            this.AddFilterNumber("maxRating", 10);
            break;
          case "irrelevant":
            this.DeleteFilter("minRating",false);
            this.AddFilterNumber("maxRating", 5);
            break;
        }
        break;
    }
  

    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";
    this.updateSearchRequest();
  }

  addmultifilter(category: string, formOptionId: string) {
    let domFielOptionElement = this._elementRef.nativeElement.querySelector(`#${formOptionId}`);
    switch (category) {
      case "gametype":
        if (domFielOptionElement.value != undefined) {
          if (domFielOptionElement.value == "inc") {
            this.DeleteFilter("gameTypeExcluded", false)
            this.AddFilterStringArray("gameType", this.multiEditListValue.selectedvalue);
          }
          if (domFielOptionElement.value == "exc") {
            this.DeleteFilter("gameType", false)
            this.AddFilterStringArray("gameTypeExcluded", this.multiEditListValue.selectedvalue);
          }
        }
        break;
      case "systems":
      case "consoles":
        if (domFielOptionElement.value != undefined) {
          //console.log(this.multiEditListValue.selectedvalue);
          if (domFielOptionElement.value == "inc") {
            this.DeleteFilter("systemsExcluded", false)
            this.AddFilterStringArray("systems", this.multiEditListValue.selectedvalue);
          }
          if (domFielOptionElement.value == "exc") {
            this.DeleteFilter("systems", false)
            this.AddFilterStringArray("systemsExcluded", this.multiEditListValue.selectedvalue);
          }
        }
        break;      
    }

    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";
    this.updateSearchRequest();
  }

  addYear(formYearId: string, formMinYearId: string, formMaxYearId: string) {
    let domFielYeardElement = this._elementRef.nativeElement.querySelector(`#${formYearId}`);
    let domFielMinYearElement = this._elementRef.nativeElement.querySelector(`#${formMinYearId}`);
    let domFielMaxYearElement = this._elementRef.nativeElement.querySelector(`#${formMaxYearId}`);

    let found = false;

    if (domFielYeardElement.value != "") {
      this.AddFilterNumber("year", +domFielYeardElement.value)
      this.DeleteFilter("yearMin", false);
      this.DeleteFilter("yearMax", false);

      found = true;
    }
    if (domFielMinYearElement.value != "") {
      this.DeleteFilter("year", false);
      this.AddFilterNumber("yearMin", +domFielMinYearElement.value)
      found = true;
    }
    if (domFielMaxYearElement.value != "") {
      this.DeleteFilter("year", false);
      this.AddFilterNumber("yearMax", +domFielMaxYearElement.value)
      found = true;
    }

    if (found) {

    
    this.showEditModal = false;
    this.editModalMode = "";
      this.editModalTitle = "";
      this.updateSearchRequest();
      
    }

  }

  addHitRangePlayed(forminput: string) {
    let domFieldElement = this._elementRef.nativeElement.querySelector(`#${forminput}`);
    
    switch (domFieldElement.value) {
      case "never":
        this.AddFilterNumber("playedhitMax", 0);
        this.DeleteFilter("playedhitMin", false);
        break;
      case "once":
        this.AddFilterNumber("playedhitMax", 1);
        this.AddFilterNumber("playedhitMin", 1);
        break;
      case "lonce":
        this.AddFilterNumber("playedhitMin", 1);
        this.DeleteFilter("playedhitMax");
        break;
      case "10time":
        this.AddFilterNumber("playedhitMin", 10);
        this.DeleteFilter("playedhitMax", false);
        break;
    }

    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";
    this.updateSearchRequest();
  }

  addDays(date: Date, days: number): Date {
  let result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

  addPeriodPlayed(forminput: string) {
    let domFieldElement = this._elementRef.nativeElement.querySelector(`#${forminput}`);
    
    let now = new Date();
    let currentmonth = new Date(now.getFullYear(), now.getMonth(), 1);
    let currenyear = new Date(now.getFullYear(), 0, 1);
    //console.log(`AddPeriodPlayed ${domFieldElement.value}`);
    //console.log(now)
    //console.log(this.addDays(new Date(), -7))
    //console.log(this.addDays(new Date(), -30))

    //console.log(currentmonth);
    //console.log(currenyear);



    switch (domFieldElement.value) {
      case "last7d":
        this.AddFilterDate("playedlastMin", this.addDays(new Date(), -7));
        this.DeleteFilter("playedlastMax", false);
        break;
      case "last30d":
        this.AddFilterDate("playedlastMin", this.addDays(new Date(), -30));
        this.DeleteFilter("playedlastMax", false);
        break;
      case "month":
        this.AddFilterDate("playedlastMin", currentmonth);
        this.DeleteFilter("playedlastMax", false);
        break;
      case "year":
        this.AddFilterDate("playedlastMin", currenyear);
        this.DeleteFilter("playedlastMax", false);
        break;

    }
    this.showEditModal = false;
    this.editModalMode = "";
    this.editModalTitle = "";
    this.updateSearchRequest();
  }

  getSystemFilterItem(systemid: string): FilterItem {
    var result: FilterItem = { label: "", color:'secondary', category:'', value: { label: '', value:'' } };

    this.allSystems.filter((s: SystemDb) => s._id == systemid).forEach((s: SystemDb) => {
      result.category = "systemId";
      result.label = s.category;
      result.value.label = s.name;
      result.value.value = s._id;
    });
    return result;
  }


  searchisEmpty(search: VideogameSearchRequest): Boolean {
    return (search.name == undefined || search.name == "")
      && (search.sortFields == undefined)
      && (search.limit == undefined)
      && (search.systemCategory == undefined)
      && (search.systems == undefined)
      && (search.systemsExcluded == undefined)
      && (search.gameType == undefined)
      && (search.gameTypeExcluded == undefined)
      && (search.gamesExcluded == undefined)
      && (search.maxRating == undefined)
      && (search.minRating == undefined)
      && (search.year == undefined)
      && (search.yearMin == undefined)
      && (search.yearMax == undefined)
      && (search.allowUnknowYear == undefined)
      && (search.allowUnRated == undefined)
      && (search.playedhitMax == undefined)
      && (search.playedhitMin == undefined)
      && (search.playedlastMax == undefined)
      && (search.playedlastMin == undefined)
      && (search.core == undefined)
      && (search.collection == undefined)
      && (search.developname == undefined)
      && (search.editor == undefined);
      

  }

  updateSearchRequest(resetpage : boolean = true) : Boolean {
    let countfilter: number = 0;
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.name != undefined && this.SearchRequest.name != "") {
      countfilter += (7 + this.SearchRequest.name.length);
    }
   
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.systems != undefined && this.SearchRequest.systems.length>0) {
      let systemcount = this.SearchRequest.systems.length > 10 ? 10 : this.SearchRequest.systems.length;
      countfilter += (10 - (systemcount - 1));
    }
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.systemsExcluded != undefined && this.SearchRequest.systemsExcluded.length > 0) {
      let systemcount = this.SearchRequest.systemsExcluded.length > 5 ? 5 : this.SearchRequest.systemsExcluded.length;
      countfilter += systemcount;
    }
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.gameType != undefined && this.SearchRequest.gameType.length > 0) {
      let gametypecount = this.SearchRequest.gameType.length > 5 ? 5 : this.SearchRequest.gameType.length;
      countfilter += (10 - (gametypecount - 1));
    }

    let periode = 9;
    if (this.SearchRequest.year != undefined) {
      periode = 0;
    }
    else {
      let min = this.SearchRequest.yearMin != undefined && this.SearchRequest.yearMin < 1970  ? this.SearchRequest.yearMin : 1970;
      let max = this.SearchRequest.yearMax != undefined && this.SearchRequest.yearMax > 2015 ? this.SearchRequest.yearMax : 2015;
      periode = Math.round( Math.max(Math.min(max - min, 45), 1) / 5); // max 9
    }

    countfilter += 10 - periode;
    //console.log(`CountFilter : ${countfilter}`)
    
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.collection != undefined) {
      countfilter += 20
    }
    //console.log(`CountFilter : ${countfilter}`)

    if (this.SearchRequest.playlist != undefined) {
      countfilter += 20
    }
    //console.log(`CountFilter : ${countfilter}`)

    if (this.SearchRequest.developname != undefined) {
      countfilter += 20
    }
    //console.log(`CountFilter : ${countfilter}`)
    if (this.SearchRequest.editor != undefined) {
      countfilter += 20
    }

    if (this.SearchRequest.systemCategory != undefined) {
      countfilter += (this.SearchRequest.systemCategory=="Arcade" ? 10 : 3)
    }

    if (this.SearchRequest.playedhitMin != undefined || this.SearchRequest.playedhitMax != undefined) {
      let sumhit = (this.SearchRequest.playedhitMin != undefined ? this.SearchRequest.playedhitMin : 0) + (this.SearchRequest.playedhitMax != undefined ? this.SearchRequest.playedhitMax : 0);
      countfilter += (sumhit > 0 ? 10 : 4);
    }

    if (this.SearchRequest.playedlastMin != undefined || this.SearchRequest.playedlastMax != undefined) {
      countfilter += (this.SearchRequest.playedlastMin != undefined ? 10 : 2);
    }

    


    //console.log(`CountFilter : ${countfilter}`)

    let rating = 20;
    let min = this.SearchRequest.minRating != undefined && this.SearchRequest.minRating >= 0 ? Math.min(this.SearchRequest.minRating, 20) : 0;
    let max = this.SearchRequest.maxRating != undefined && this.SearchRequest.maxRating <= 20 ? Math.max(this.SearchRequest.maxRating, 0) : 20;
    rating = Math.max(max - min, 1)
   
    if (rating < 20) {
      countfilter += 5 - Math.round(rating / 5);
      
    }
    //console.log(`CountFilter : rating ${rating} - ${countfilter}`)
    //console.log(`CountFilter : ${countfilter}`)
    if (countfilter >= 10) {
      if (resetpage) {
        this.SearchRequest.page = 1;
      }
      this.querygamesservice.setCurrentVideoGameSearchRequest(this.SearchRequest);
      //console.log("== Save search == ")
      //console.debug(this.SearchRequest)

      this.NewSearchRequest.emit(this.SearchRequest);
      return true;
    }

    return false;
  }  

}



export interface FilterItem {
  label: string;
  category: string;
  value: FilterValue;
  color: string;
}

export interface FilterValue {
  label: string;
  value: any;
}

export interface EditListValue {
  label: string;
  category: string;
  selectedvalue: string;
  items: FilterValue[];
  options: FilterValue[];
}

export interface MultiEditListValue {
  label: string;
  category: string;
  selectedvalue: string[];
  items: FilterValueSeleted[];
  options: FilterValueSeleted[];
}

export interface FilterValueSeleted {
  label: string;
  value: any;
  selected: boolean;
}
