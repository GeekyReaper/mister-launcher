import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle, Location } from '@angular/common';
import {  cilGamepad } from '@coreui/icons';
import { FooterModule } from '@coreui/angular';
import { QuerygamesService } from '../../services/querygames.service';
import { SystemSearchRequest } from '../../services/models/system-search-request';
import { SystemSearchResult } from '../../services/models/system-search-result';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import {
  BorderDirective,
  ContainerComponent,
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
  Tabs2Module,
  FormSelectDirective
} from '@coreui/angular';
import { StateService } from '../../services/state.service';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { ItemlistSystemComponent } from '../../components/itemlist-system/itemlist-system.component'

@Component({
  selector: 'app-systems',
  templateUrl: './systems.component.html',
  styleUrl: './systems.component.scss',
  standalone : true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, IconDirective, ItemlistSystemComponent,
    Tabs2Module],
  providers: [QuerygamesService, IconSetService]
})
export class SystemsComponent implements OnInit, OnDestroy {
  public systemsresult: SystemSearchResult | undefined;
  subQuery!: Subscription;
  formSearch!: FormGroup;


  public systemsresultArcade: SystemSearchResult | undefined;
  public systemsresultConsole: SystemSearchResult | undefined;

  subQueryArcade!: Subscription;
  subQueryConsole!: Subscription;

  formSearchArcade!: FormGroup;
  formSearchConsole!: FormGroup;

  submitted = false;
  formoptionVisible = false;

  submittedArcade = false;
  submittedConsole = false;
  formoptionVisibleArcade = false;
  formoptionVisibleConsole = false;

  state: any;
  public systemcategory : string = "console"

  constructor(private querygamesservice: QuerygamesService, private route: ActivatedRoute,
    private formBuilder: FormBuilder, public iconSet: IconSetService,
    private location: Location) {
    iconSet.icons = { cilGamepad };

  }

  ngOnInit(): void {
    
   
    if (this.route.snapshot.url[0].path.toLowerCase() == "console") {
      this.systemcategory = "console";
    }
    if (this.route.snapshot.url[0].path.toLowerCase() == "arcade") {
      this.systemcategory = "arcade";
    }

    console.log(this.route.snapshot.url[0].path.toLowerCase())
    console.log(this.systemcategory)

    this.formSearchArcade = this.formBuilder.group(
      {
        search: [''], //, [Validators.required, Validators.minLength(3)]],
        selectSystemCategory: ["Arcade"],
        selectCompany: ['']
      });
    this.formSearchConsole = this.formBuilder.group(
      {
        search: [''], //, [Validators.required, Validators.minLength(3)]],
        selectSystemCategory: ["Console"],
        selectCompany: ['']
      });
    

    //this.formSearch = this.formBuilder.group(
    //  {
    //    search: [''], //, [Validators.required, Validators.minLength(3)]],
    //    selectSystemCategory: [this.systemcategory],
    //    selectCompany:['']
    //  });

    //this.onSubmit();
    this.onSubmitArcade();
    this.onSubmitConsole();
  }

  ngOnDestroy(): void {   
    this.subQuery?.unsubscribe();
    this.subQueryArcade?.unsubscribe();
    this.subQueryConsole?.unsubscribe();
  }

  onReset(): void {                 // clique sur le bouton: "annuler"
    this.submitted = false;
    this.systemsresult = undefined;

    Object.keys(this.formSearch.controls).forEach(key => {
      this.formSearch.get(key)?.reset("");
    });


  }

  onResetArcade(): void {                 // clique sur le bouton: "annuler"
    this.submittedArcade = false;
    this.systemsresultArcade = undefined;

    Object.keys(this.formSearchArcade.controls).forEach(key => {
      this.formSearchArcade.get(key)?.reset("");
    });


  }

  onResetConsole(): void {                 // clique sur le bouton: "annuler"
    this.submittedConsole = false;
    this.systemsresultConsole = undefined;

    Object.keys(this.formSearchConsole.controls).forEach(key => {
      this.formSearchConsole.get(key)?.reset("");
    });


  }

  toggleCollapse(): void {
    //if (this.gamesresult != undefined) {
    this.formoptionVisible = !this.formoptionVisible;
   
  }

  onChange(): void {
    this.onSubmit();
  }

  onChangeArcade(): void {
    this.onSubmitArcade();
  }

  onChangeConsole(): void {
    this.onSubmitConsole();
  }
  onSubmit(): void {                        // à la soumission du formulaire, clique sur le bouton: "s'enregistrer"
    this.submitted = true;    

    let systemsearch: SystemSearchRequest =
    {
      name: this.formSearch.value.search,
      sortFields: [{
        field:'name',
        isAscending:true
      }],
      limit: 200,
      allowUnknowYear: false      
    }

    if (this.formSearch.value.selectSystemCategory && this.formSearch.value.selectSystemCategory != "") {
      systemsearch.category = this.formSearch.value.selectSystemCategory;
    }
    if (this.formSearch.value.selectCompany && this.formSearch.value.selectCompany != "") {
      systemsearch.company = this.formSearch.value.selectCompany;
    }

    this.subQuery = this.querygamesservice.getSystems(systemsearch).pipe().subscribe((gs: SystemSearchResult) => {
      this.systemsresult = gs;           
    });

  }

  onSubmitArcade(): void {                        // à la soumission du formulaire, clique sur le bouton: "s'enregistrer"
    this.submittedArcade = true;

    let systemsearch: SystemSearchRequest =
    {
      name: this.formSearchArcade.value.search,
      sortFields: [{
        field: 'name',
        isAscending: true
      }],
      limit: 200,
      allowUnknowYear: false
    }

    if (this.formSearchArcade.value.selectSystemCategory && this.formSearchArcade.value.selectSystemCategory != "") {
      systemsearch.category = this.formSearchArcade.value.selectSystemCategory;
    }
    if (this.formSearchArcade.value.selectCompany && this.formSearchArcade.value.selectCompany != "") {
      systemsearch.company = this.formSearchArcade.value.selectCompany;
    }

    this.subQueryArcade = this.querygamesservice.getSystems(systemsearch).subscribe((gs: SystemSearchResult) => {
      this.systemsresultArcade = gs;
      this.submittedArcade = false;
    });

  }

  onSubmitConsole(): void {                        // à la soumission du formulaire, clique sur le bouton: "s'enregistrer"
    this.submittedConsole = true;

    let systemsearch: SystemSearchRequest =
    {
      name: this.formSearchConsole.value.search,
      sortFields: [{
        field: 'name',
        isAscending: true
      }],
      limit: 200,
      allowUnknowYear: false
    }

    if (this.formSearchConsole.value.selectSystemCategory && this.formSearchConsole.value.selectSystemCategory != "") {
      systemsearch.category = this.formSearchConsole.value.selectSystemCategory;
    }
    if (this.formSearchConsole.value.selectCompany && this.formSearchConsole.value.selectCompany != "") {
      systemsearch.company = this.formSearchConsole.value.selectCompany;
    }

    this.subQueryConsole = this.querygamesservice.getSystems(systemsearch).subscribe((gs: SystemSearchResult) => {
      this.systemsresultConsole = gs;
      this.submittedConsole = false;
    });

  }

  tabChange(event: any): void {
    //console.log(event);

    this.systemcategory = event as string;
    this.location.replaceState("/systems/" + this.systemcategory)

  }

}
