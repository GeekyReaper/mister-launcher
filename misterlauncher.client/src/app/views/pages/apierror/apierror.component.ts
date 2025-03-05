import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { IconDirective } from '@coreui/icons-angular';
import { ContainerComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, FormControlDirective, ButtonDirective } from '@coreui/angular';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { ManagerCache } from '../../../services/models/manager-cache';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-apierror',
  templateUrl: './apierror.component.html',
  styleUrl: './apierror.component.scss',
  standalone: true,
  imports: [ContainerComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective]
})
export class ApierrorComponent { 
//implements OnInit {

  //public managercache2$: Observable<ManagerCache> = this.mistersignalr.managerCacheRefresh$
  //constructor(private mistersignalr: MisterSignalrService, private router : Router) { }

  constructor() { }

  //ngOnInit(): void {
    //this.managercache2$.subscribe((w: ManagerCache)  => {
    //  //console.log("[DefaultHeaderComponent] - Update Cache Event")
    //  if (w.health.misterState != "ERROR") {
    //    console.debug("-- misterState ok redirect to search")
    //    this.router.navigateByUrl('/videogames/search')
    //  }
    //});
    //}
}
