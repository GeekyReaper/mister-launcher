import { Component, Input, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IconModule, IconDirective, IconSetService } from '@coreui/icons-angular';
import { cilMediaPlay } from '@coreui/icons';
import {  RouterLink } from '@angular/router';
import {
  ButtonDirective, ButtonCloseDirective, GridModule, BadgeComponent, BadgeModule, AlertComponent, SpinnerComponent,
  ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent
  
} from '@coreui/angular';
import { QuerygamesService } from '../../services/querygames.service';
import { Subscription } from 'rxjs';
import { VideogameDb } from '../../services/models/videogame-db';
import { RomDb } from '../../services/models/rom-db';

@Component({
  selector: 'app-part-videogame-launchbutton',
  templateUrl: './part-videogame-launchbutton.component.html',
  styleUrl: './part-videogame-launchbutton.component.scss',
  standalone: true,
  imports: [CommonModule, SpinnerComponent,
    RouterLink, GridModule, BadgeComponent, BadgeModule, AlertComponent,
    IconDirective, IconModule,
    ButtonDirective, ButtonCloseDirective,
    ToastComponent, ToasterComponent, ToastHeaderComponent, ToastBodyComponent, ProgressComponent],
  providers: [IconSetService]
})
export class PartVideogameLaunchbuttonComponent implements OnDestroy {
  @Input() videogame!: VideogameDb;
  @Input() color: string = "secondary";
  @Input() label: string = ""
  @Input() btnclass: string = "m-2"
  @Input() canLaunch: Boolean = true;

  @Input() rom!: RomDb;
  @Input() haslabel: boolean = true;

  toastcolor: string = "success";
  toastbadge: string = "ok"
  subQueryVideoGameLaunch!: Subscription;

  launching: Boolean = false;

  constructor(private querygamesservice: QuerygamesService, public iconSet: IconSetService) {
    iconSet.icons = { cilMediaPlay };
  }

  ngOnDestroy(): void {
    this.subQueryVideoGameLaunch?.unsubscribe();
  }
  position = 'top-center';
  visible = signal(false);
  percentage = signal(0);


  LaunchRom(videogameId: string, romId?: string): void {

    this.launching = true;

    this.subQueryVideoGameLaunch = this.querygamesservice.LaunchVideoGame(videogameId, romId).subscribe(
      (value: VideogameDb) => {
        console.log("-- launch Game");
        console.log(value);
        this.launching = false;
        this.toastcolor = "success";
        this.toastbadge = "ok"
        this.visible.update((value: Boolean) => !value);
      },
      (error: any) => {
        console.log(error);
        this.launching = false;
        this.toastcolor = "danger";
        this.toastbadge = "fail";
        this.visible.update((value: Boolean) => !value);
      }
    );

  }

  onClosed() {
    this.visible.update((value: Boolean) => !value);
  }

  onVisibleChange($event: boolean) {
    this.visible.set($event);
    this.percentage.set(this.visible() ? this.percentage() : 0);
  }

  onTimerChange($event: number) {
    this.percentage.set($event * 25);    
  }
}
