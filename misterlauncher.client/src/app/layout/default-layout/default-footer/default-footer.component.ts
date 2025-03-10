import { Component, OnInit } from '@angular/core';
import { FooterComponent } from '@coreui/angular';
import versionJson from '../../../version.json';


@Component({
    selector: 'app-default-footer',
    templateUrl: './default-footer.component.html',
    styleUrls: ['./default-footer.component.scss'],
    standalone: true,
})
export class DefaultFooterComponent extends FooterComponent implements OnInit{
  version: string ="";
  constructor() {
    
    super();
  }
    ngOnInit(): void {
      this.version = versionJson.version;
    }

}
