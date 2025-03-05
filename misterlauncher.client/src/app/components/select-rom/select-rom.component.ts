import { Component, Input, OnDestroy, OnInit, signal, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { cilBadge } from '@coreui/icons';
import { RouterLink } from '@angular/router';
import {
  AccordionButtonDirective, AccordionComponent, AccordionItemComponent, TemplateIdDirective,
  BadgeComponent,
  ButtonDirective, ButtonCloseDirective,
  TableDirective, TableModule,
  ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent,
  FormControlDirective, FormCheckComponent, FormCheckInputDirective

} from '@coreui/angular';
import { QuerygamesService } from '../../services/querygames.service';
import { Subscription } from 'rxjs';
import { RomInfo } from '../../services/models/rom-info';
import { FilesizePipe } from "../../pipe/filesize.pipe";

@Component({
  selector: 'app-select-rom',
  templateUrl: './select-rom.component.html',
  styleUrl: './select-rom.component.scss',
  standalone: true,
  imports: [CommonModule,
    RouterLink,
    BadgeComponent,
    IconDirective, IconModule,
    AccordionButtonDirective, AccordionComponent, AccordionItemComponent, TemplateIdDirective,
    ButtonDirective, ButtonCloseDirective,
    ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent,
    TableDirective, TableModule,
    FormControlDirective, FormCheckComponent, FormCheckInputDirective,
    FilesizePipe],
  providers: [IconSetService]
})
export class SelectRomComponent implements OnInit, OnDestroy {

  subQueryRom!: Subscription;

  Roms: RomInfo[] = []

  @Input() parentrom!: string;
  @Input() systemid!: string;
  selectedRoms: string[] = [];

  @Output() UpdateSelectedRom = new EventEmitter<string[]>();

  constructor(private querygamesservice: QuerygamesService, public iconSet: IconSetService) {
    iconSet.icons = { cilBadge };
  }
  ngOnInit(): void {
    console.log(`[select-rom] (oninit) systemid : ${this.systemid}, parentrom= ${this.parentrom}`);
    if (this.systemid == "Arcade") {
      this.subQueryRom = this.querygamesservice.GetUnmatchRoms("Arcade", "").subscribe((r: RomInfo[]) => {
        this.Roms = r.filter(s => s._id != this.parentrom);
        console.log(this.Roms);
      });
    }
    else {
      this.subQueryRom = this.querygamesservice.GetUnmatchRoms("Console", this.systemid).subscribe((r: RomInfo[]) => {
        this.Roms = r.filter(s => s._id != this.parentrom);
      });
    }

  }

  ngOnDestroy(): void {
    this.subQueryRom?.unsubscribe;
  }

  romcheckbox($event: any, romid: string): void {
    let isChecked = $event.srcElement.checked;      
    if (isChecked) {
      this.selectedRoms.push(romid);
    }
    else {
      this.selectedRoms = this.selectedRoms.filter(s => s != romid)
    }
    this.UpdateSelectedRom.emit(this.selectedRoms);
    


  }
}
