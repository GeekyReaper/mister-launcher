import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule } from '@angular/common';
import { FooterModule } from '@coreui/angular';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { brandSet, flagSet, freeSet, cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay } from '@coreui/icons';
import { QuerygamesService } from '../../services/querygames.service';
import { GameSearch } from '../../services/models/game-search';
import { GameSearchResult } from '../../services/models/game-search-result';
import { ActivatedRoute, RouterLink, RouterLinkActive } from '@angular/router';

import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import { NgStyle } from '@angular/common';
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
  FormSelectDirective
} from '@coreui/angular';
import { GameAction } from '../../services/models/game-action';
import { StateService } from '../../services/state.service';


@Component({
  selector: 'app-test',
  standalone: true,
  imports: [RowComponent, CommonModule, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, IconDirective],
  templateUrl: './games.component.html',
  styleUrl: './games.component.scss',
  providers: [QuerygamesService, IconSetService]
})
export class GamesComponent implements OnInit, OnDestroy {
  public test!: number;
  public gamesresult: GameSearchResult | undefined;
  public systemlist: Array<string> = new Array<string>();
  subQueryGames!: Subscription;
  formSearchGame!: FormGroup;
  submitted = false;
  formoptionVisible = false;
  state: any;


  constructor(private querygamesservice: QuerygamesService, private formBuilder: FormBuilder, private stateService: StateService, public iconSet: IconSetService) {
    iconSet.icons = { cilCloudDownload, cilChevronCircleLeftAlt, cilHeart, cilLibraryAdd, cilMediaPlay, ...brandSet };
}

  ngOnInit(): void {

    this.state = this.stateService.state$.getValue() || {};
    if (this.state && this.state.formSearchGame) {
      this.formSearchGame = this.state.formSearchGame;
      this.formoptionVisible = this.state.formoptionVisible
      this.onSubmit()
    }
    else {

      this.formSearchGame = this.formBuilder.group(
        {
          search: [''], //, [Validators.required, Validators.minLength(3)]],
          selectSystemName: [''],
          selectSystemType: [''],
          selectGameType: ['']
        }
      );
      // save forms
      this.state.formSearchGame = this.FormIsEmpty() ? null : this.formSearchGame;
      this.state.formoptionVisible = this.formoptionVisible;
      this.stateService.state$.next(this.state);
    }


    this.formSearchGame.valueChanges.subscribe((formValues: any) => { // si le formulaire est modifié (quel que soit le champ)
      this.state.formSearchGame = this.FormIsEmpty() ? null : this.formSearchGame;
      this.state.formoptionVisible = this.formoptionVisible;
      this.stateService.state$.next(this.state);
      console.log(formValues);
    });

    

  }

  FormIsEmpty(): boolean {
    return  (this.formSearchGame.value.search == '' && this.formSearchGame.value.selectSystemType == '' && this.formSearchGame.value.selectSystemName == '')
  }



  onChange(): void {
    this.onSubmit();
  }

  onSubmit(): void {                        // à la soumission du formulaire, clique sur le bouton: "s'enregistrer"
    this.submitted = true;


    // on arrête ici si le formulaire est invalide
    if (this.formSearchGame.invalid) {      // s’il y a une erreur, le formulaire est invalide
      alert('Fail!! :-)');
      return;
    }

    let gamesearch: GameSearch =
    {
      name: this.formSearchGame.value.search,
      limit: 50,
      allowUnknowYear: false,
      allowUnRated: false
    }

    if (this.formSearchGame.value.selectSystemType && this.formSearchGame.value.selectSystemType != "") {
      gamesearch.systemCategory = this.formSearchGame.value.selectSystemType;
    }

    if (this.formSearchGame.value.selectSystemName && this.formSearchGame.value.selectSystemName != "") {
      gamesearch.systemId = this.formSearchGame.value.selectSystemName;
    }






    this.subQueryGames = this.querygamesservice.getGames(gamesearch).pipe().subscribe((gs: GameSearchResult) => {
      this.gamesresult = gs;
      this.systemlist.splice(0);
      gs.filterOption.systemDbs.forEach(item => {

        this.systemlist.push(item._id);
      });
      console.log(this.systemlist);
    });

  }
  onReset(): void {                 // clique sur le bouton: "annuler"
    this.submitted = false;
    this.gamesresult = undefined;

    Object.keys(this.formSearchGame.controls).forEach(key => {
      this.formSearchGame.get(key)?.reset("");
    });
    this.state.formSearchGame = this.FormIsEmpty() ? null : this.formSearchGame;
    this.state.formoptionVisible = this.formoptionVisible;
    this.stateService.state$.next(this.state);





  }
  ngOnDestroy(): void {
    this.state.formSearchGame = this.FormIsEmpty() ? null : this.formSearchGame;
    this.state.formoptionVisible = this.formoptionVisible;
    this.stateService.state$.next(this.state);
    this.subQueryGames?.unsubscribe();
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

  toggleCollapse(): void {
    //if (this.gamesresult != undefined) {
    this.formoptionVisible = !this.formoptionVisible;
    this.state.formSearchGame = this.FormIsEmpty() ? null : this.formSearchGame;
    this.state.formoptionVisible = this.formoptionVisible;
    this.stateService.state$.next(this.state);
    //}
    //alert(JSON.stringify(this.formSearchVideoGame.value));
  }

}
