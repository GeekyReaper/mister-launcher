import { Component, OnInit, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs'
import { CommonModule, NgStyle, Location } from '@angular/common';
import { cilInfo, cilLaptop, cilCheck, cilChevronCircleLeftAlt, cilLinkBroken, cilLink, cilSearch, cilTrash } from '@coreui/icons';
import { FooterModule } from '@coreui/angular';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { IconDirective, IconSetService } from '@coreui/icons-angular';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import { QuerygamesService } from '../../../services/querygames.service';


import {
  AccordionButtonDirective,
  AccordionComponent,
  AccordionItemComponent,
  TemplateIdDirective,
  BorderDirective,
  BadgeModule,
  Tabs2Module,
  ContainerComponent,
  SpinnerModule,
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
  CarouselComponent,
  CarouselControlComponent,
  CarouselInnerComponent,
  CarouselItemComponent,
  CarouselIndicatorsComponent,
  ColComponent,
  GutterDirective,
  ListGroupDirective,
  ListGroupItemDirective,
  ImgDirective,
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  ProgressComponent,
  InputGroupComponent, InputGroupTextDirective
} from '@coreui/angular';

import { RomInfo } from '../../../services/models/rom-info';
import { VideogameDb } from '../../../services/models/videogame-db'
import { PartVideogameCategoriesComponent } from '../../../components/part-videogame-categories/part-videogame-categories.component';
import { ModalConfirmationComponent, ConfirmationOption } from '../../../components/modal-confirmation/modal-confirmation.component';

import { SelectRomComponent } from '../../../components/select-rom/select-rom.component'
import { FilesizePipe } from "../../../pipe/filesize.pipe";
import { MisterDatePipe } from "../../../pipe/misterdate.pipe";

@Component({
  selector: 'app-rom-link',
  templateUrl: './rom-link.component.html',
  styleUrl: './rom-link.component.scss',
  standalone: true,
  imports: [RowComponent, CommonModule, ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective,
    ImgDirective, Tabs2Module, SpinnerModule,
    BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, TableModule, TableDirective,
    CarouselComponent, CarouselControlComponent,  CarouselInnerComponent,   CarouselItemComponent,   CarouselIndicatorsComponent,
    BadgeModule, ProgressComponent,
    PartVideogameCategoriesComponent, ModalConfirmationComponent,
    AccordionButtonDirective, AccordionComponent, AccordionItemComponent, TemplateIdDirective, 
    FilesizePipe, MisterDatePipe, IconDirective,
    SelectRomComponent,
    InputGroupComponent, InputGroupTextDirective
  ],
  providers: [IconSetService]
  
})
export class RomLinkComponent implements OnInit, OnDestroy {

  subSelectRom?: Subscription;
  subSearchVideoGame?: Subscription;
  subSelectVideogame?: Subscription;
  subVideogameMatch?: Subscription;
  subUnlinkRom?: Subscription;
  subDeleteRom?: Subscription;
  romid: string = "";
  public romInfo?: RomInfo;
  public videogames?: VideogameDb[];
  public videogamematch?: VideogameDb;

  confirmationModalOptionsForDelete: ConfirmationOption[] = [
    { label: "With file", value: "file", color: "danger", icon: "" },
    { label: "Rom only", value: "rom", color: "warning", icon: "" }];
  confirmationShow: Boolean = false;
  formSearchVideoGame!: FormGroup;
  isSubmited: Boolean = false;
  spinnerIsSelected: Boolean = false;
  selectedroms : string[] = []

  constructor(private route: ActivatedRoute, private querygamesservice: QuerygamesService, private formBuilder: FormBuilder,
    public iconSet: IconSetService,
    private location: Location) {
    iconSet.icons = { cilInfo, cilLaptop, cilCheck, cilChevronCircleLeftAlt, cilLinkBroken, cilLink, cilSearch, cilTrash };
  }
    ngOnInit(): void {

      // LoadRom
      this.route.params.subscribe(
        params => {
          const id = params['id'];
          this.romid = id
          console.log(this.romid);
          this.subSelectRom = this.querygamesservice.GetRomFromId(this.romid).pipe().subscribe((r: RomInfo) => {
            this.romInfo = r;
            if (r.isMatch) {
              this.subVideogameMatch = this.querygamesservice.SearchVideogameFromRomId(r._id).subscribe((vg: VideogameDb) => {
                this.videogamematch = vg;
              })
            }
            console.log(this.romInfo);
          });
        });
      // Initialize Form
      this.formSearchVideoGame = this.formBuilder.group(
        {
          search: ['', [Validators.required, Validators.minLength(3)]]       
        }
      );




  }

  SubmitSearch(): void {
    if (this.isSubmited) {
      console.log("Search already process");
      return;
    }
    if (!this.formSearchVideoGame.valid && this.romInfo) {
      console.log("InvalidForms");
      return;
    }

    var systemid = this.romInfo?.systemCategory == "Arcade" ? "Arcade" : this.romInfo?.systemid;

    this.isSubmited = true;
    this.subSearchVideoGame = this.querygamesservice.SearchVideogameFromScrapper(this.formSearchVideoGame.value.search, systemid).subscribe((vgl: VideogameDb[]) => {
      this.videogames = vgl;
      console.log(vgl);
      this.isSubmited = false;
    });



  }


  SelectVideoGame(videogame?: number): void {
    if (videogame != undefined) {
      this.spinnerIsSelected = true;
    
      this.subSelectVideogame = this.querygamesservice.LinkRomToScrapperVideogame(this.romid, videogame, this.selectedroms).subscribe((b: Boolean) => {
        if (b) {
          this.location.back();
          console.log("[rom-link] Success link")
        }
        this.spinnerIsSelected = false;
      });
    }
  }



  ngOnDestroy(): void {
    this.subSelectRom?.unsubscribe();
    this.subSearchVideoGame?.unsubscribe();
    this.subSelectVideogame?.unsubscribe();
    this.subUnlinkRom?.unsubscribe();
    this.subDeleteRom?.unsubscribe();
  }

  goBack() {
    this.location.back();
  }

  unlinkRom() {
    if (this.videogamematch != undefined && this.romInfo != undefined) {

    
      this.subUnlinkRom = this.querygamesservice.UnlinkRomForVideogame(this.videogamematch._id, this.romInfo._id).subscribe((vg: VideogameDb) => {
        //reload
        this.ngOnInit();
      });
    }
  }

  selectedromupdated(roms: string[]) {
    //console.log(roms);
    this.selectedroms = roms;
  }

  showConfirmationDelete(): void {
    this.confirmationShow = true;
  }

  deleteRom(value: string): void {
    console.log(`Delete rom confirm with action ${value}`);
    let deletefile = value == "file";
    console.log(`Delete file ${deletefile}`);
    if (this.romInfo != undefined) {
      this.subDeleteRom = this.querygamesservice.DeleteRom(this.romInfo._id, deletefile).subscribe((success: Boolean) => {
        console.log(`delete file result ${success}`)
        if (success)
          this.goBack();
      })
    }
    this.confirmationShow = false;
  }
}
