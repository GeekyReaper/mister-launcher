import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {  
  BadgeComponent,
 } from '@coreui/angular';

@Component({
  selector: 'app-part-videogame-categories',
  templateUrl: './part-videogame-categories.component.html',
  styleUrl: './part-videogame-categories.component.scss',
  standalone: true,
  imports: [BadgeComponent, CommonModule]
})


export class PartVideogameCategoriesComponent implements OnInit {
  
  @Input() categories!: string[];

  public categoriesUi!: CategoryUI[];

  ngOnInit(): void {
    this.categoriesUi = new Array<CategoryUI>();

    //console.log(this.categories);
    this.categories.forEach((v: string) => {
      var l = v.split("/").at(-1);

      let category: CategoryUI = {
        name: v,
        label: l!=undefined ? l.trim() : "",
          levelcolor: '',
          level: v.split("/").length - 1
      }
      category.levelcolor = category.level == 0 ? "info" : category.level == 1 ? "secondary" : "light";

      this.categoriesUi.push(category);

      }
    );

    this.categoriesUi = this.categoriesUi.sort((a, b) => (a.level < b.level ? -1 : 1));
   //console.log(this.categoriesUi);
  }

}

export interface CategoryUI {
  label: string;
  levelcolor: string;
  level: number;
  name: string;
};


