import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { RomDb } from '../../services/models/rom-db';
import { CommonModule } from '@angular/common';
import { cilGamepad, cilMediaPlay, cilApplications , cilInfo } from '@coreui/icons';
import { ActivatedRoute, RouterLink, RouterLinkActive, Router } from '@angular/router';
import {
  BorderDirective,
  ButtonCloseDirective,
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
  TableModule,
  NavComponent,
  NavItemComponent,
  NavLinkDirective,
  RowComponent,
  TextColorDirective,
  FormDirective, FormLabelDirective, FormControlDirective,
  BadgeComponent,
  CollapseDirective,
  FormSelectDirective,
  FooterModule,
  ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent
} from '@coreui/angular';
import { QuerygamesService } from '../../services/querygames.service';
import { Subscription } from 'rxjs';
import { VideogameDb } from '../../services/models/videogame-db';
import { PartVideogameLaunchbuttonComponent} from '../part-videogame-launchbutton/part-videogame-launchbutton.component'

interface Dictionary<T> {
  [Key: string]: T;
}

@Component({
  selector: 'app-list-roms',
  templateUrl: './list-roms.component.html',
  styleUrl: './list-roms.component.scss',
  standalone : true,
  imports: [RowComponent, ButtonCloseDirective, CommonModule, TableModule,ContainerComponent, ColComponent, TextColorDirective, CardComponent, CardHeaderComponent, CardBodyComponent, CardTitleDirective, CardTextDirective, ButtonDirective, CardSubtitleDirective, CardLinkDirective, ListGroupDirective, ListGroupItemDirective, CardFooterComponent, NavComponent, NavItemComponent, NavLinkDirective, BorderDirective, CardGroupComponent, GutterDirective, CardImgDirective,
    FormDirective, FormLabelDirective, FormControlDirective, ButtonDirective, BadgeComponent, CollapseDirective, FormSelectDirective, FooterModule, RouterLink, IconDirective,
    ModalComponent, ModalBodyComponent, ModalHeaderComponent, ModalFooterComponent,
    PartVideogameLaunchbuttonComponent],
  providers: [QuerygamesService, IconSetService]
})
export class ListRomsComponent implements OnInit {
  constructor(private querygamesservice: QuerygamesService, public iconSet: IconSetService) {
    iconSet.icons = { cilGamepad, cilMediaPlay, cilApplications, cilInfo };
  }
  ngOnInit(): void {
    this.roms.forEach((r: RomDb) => { this.modalvisible[r.romid] = false; })
    }

  @Input() roms!: RomDb[];
  @Input() videogame_id!: string;
  @Input() videogame!: VideogameDb;

  @Output() NeedRefresh = new EventEmitter<string>();  

  public modalvisible: Dictionary<Boolean> = {};

  LaunchRom(romid?: string): void {
    this.querygamesservice.LaunchVideoGame(this.videogame_id, romid).subscribe( (v: VideogameDb) => console.log (v.name));
  }

  toggleModal(romid: string) {
    this.modalvisible[romid] = !this.modalvisible[romid] 
   }

  handleModalChange(romid: string, event: any) {
    this.modalvisible[romid] = event;
  }

  SetPrimary(romid: string) {
    this.querygamesservice.SetPrimaryRomForVideogame(this.videogame_id, romid).subscribe(vg => {
      if (vg != null) {
        this.NeedRefresh.emit("Set primary " + romid);
        this.modalvisible[romid] = false;
      }
    });
  }
  UnlinkRom(romid: string) {
    this.querygamesservice.UnlinkRomForVideogame (this.videogame_id, romid).subscribe(vg => {
        if (vg != null) {
          this.NeedRefresh.emit("Unlink " + romid);
          this.modalvisible[romid] = false;
        }
      });

  }

  

}
