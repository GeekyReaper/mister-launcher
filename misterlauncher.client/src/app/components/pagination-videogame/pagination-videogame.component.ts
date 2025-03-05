import { Component, Input, OnInit, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { cilChevronDoubleLeft, cilChevronDoubleRight } from '@coreui/icons';
import { IconSetService, IconDirective } from '@coreui/icons-angular';

import { CommonModule} from '@angular/common';

import {
  BorderDirective,
  ContainerComponent,
  ButtonDirective,
  ColComponent, RowComponent,
  GutterDirective,
  TextColorDirective,
  AlertComponent,
  ButtonGroupComponent, ButtonModule  
} from '@coreui/angular';

@Component({
  selector: 'app-pagination-videogame',
  templateUrl: './pagination-videogame.component.html',
  styleUrl: './pagination-videogame.component.scss',
  standalone: true,
  imports: [
    CommonModule,
    IconDirective,
    BorderDirective,
    ContainerComponent,
    ButtonDirective,
    ColComponent, RowComponent,
    GutterDirective,
    TextColorDirective,
    AlertComponent,
    ButtonGroupComponent, ButtonModule  
  ],
  providers : [IconSetService]
})



export class PaginationVideogameComponent implements OnInit, OnChanges {

  constructor(
    public iconSet: IconSetService) {
    iconSet.icons = {
      cilChevronDoubleLeft, cilChevronDoubleRight
    };
  } 
 
  @Input() CurrentPage?: number;
  @Input() ResultCount?: number;
  @Input() PageSize?: number;

  @Output() ChangePage = new EventEmitter<number>();

  pagination : PageInfo[] = []

  ngOnInit(): void {
    this.refresh();
  }

  ngOnChanges(changes: SimpleChanges): void {
    //console.log("PAGINATION ON CHANGE")
    this.refresh();
  }

  refresh(): void {

    //console.log("new pagination");
    this.pagination = [];

    if (this.CurrentPage == undefined || this.ResultCount == undefined || this.PageSize == undefined) {
      return;
    }

    if (this.ResultCount == undefined || this.ResultCount == 0 || this.PageSize == undefined) {
      return;
    }

    let pagenumber = Math.floor(this.ResultCount / this.PageSize) + (this.ResultCount % this.PageSize == 0 ? 0 : 1);
    if (pagenumber == 1) {
      return;
    }
    if (pagenumber <= 5) {
      for (var i = 1; i <= pagenumber; i++) {
        this.pagination.push({ label: i.toString(), value: i, selected: this.CurrentPage == i, icon:"" })
      }
    }
    else {
      let start = 1;
      let end = pagenumber;

      start = this.CurrentPage - 2
      end = this.CurrentPage + 2

      if (start <= 1) {
        start = 1;
        end = 5;
      }
      if (end > pagenumber) {
        start = pagenumber - 5;
        end = pagenumber;
      }

      if (start > 1) {
        this.pagination.push({ label: "<<", value: 1, selected: false, icon : "cilChevronDoubleLeft" })
      }

      for (var i = start; i <= end; i++) {
        this.pagination.push({ label: i.toString(), value: i, selected: this.CurrentPage == i, icon : "" })
      }

      if (end < pagenumber) {
        this.pagination.push({ label: ">>", value: pagenumber, selected: false, icon: "cilChevronDoubleRight" })
      }

    }

    //console.log(this.pagination);
    

  }



  OnPageChange(pagenumber: number) {
    this.ChangePage.emit(pagenumber)
  }

}

export interface PageInfo {
  selected: boolean;
  label: string;
  icon: string;
  value: number;
}
