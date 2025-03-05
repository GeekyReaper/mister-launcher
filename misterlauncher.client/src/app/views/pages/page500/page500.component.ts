import { Component, OnDestroy, OnInit } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import { ContainerComponent, AlertComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, FormControlDirective, ButtonDirective, SpinnerComponent, SpinnerModule  } from '@coreui/angular';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';
import { ManagerCache } from '../../../services/models/manager-cache';

@Component({
    selector: 'app-page500',
    templateUrl: './page500.component.html',
    styleUrls: ['./page500.component.scss'],
    standalone: true,
  imports: [ContainerComponent, AlertComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective, SpinnerComponent, SpinnerModule]
})
export class Page500Component implements OnInit, OnDestroy {
  public managercache2$: Observable<ManagerCache> = this.mistersignalr.managerCacheRefresh$;
  submanagercache? : Subscription;
  constructor(private mistersignalr: MisterSignalrService, private router: Router) { }
    ngOnDestroy(): void {
      this.submanagercache?.unsubscribe();
    }

  ngOnInit(): void {
    this.submanagercache = this.managercache2$.subscribe((w: ManagerCache) => {
      if (w.health.misterState != "ERROR") {
        this.router.navigateByUrl('/login')
      }
    });
    }
}
