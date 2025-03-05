import { Component, computed, DestroyRef, inject, Input, OnDestroy, OnInit } from '@angular/core';
import { QuerygamesService } from '../../../services/querygames.service';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { PlayingVideogame } from "../../../services/models/playing-videogame";
import {
  AvatarComponent,
  BadgeComponent,
  BreadcrumbRouterComponent,
  ColorModeService,
  ContainerComponent,
  DropdownComponent,
  DropdownDividerDirective,
  DropdownHeaderDirective,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  HeaderComponent,
  HeaderNavComponent,
  HeaderTogglerDirective,
  NavItemComponent,
  NavLinkDirective,
  ProgressBarDirective,
  ProgressComponent,
  SidebarToggleDirective,
  TextColorDirective,
  ThemeDirective
} from '@coreui/angular';
import { NgStyle, NgTemplateOutlet } from '@angular/common';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { IconDirective, IconModule, IconSetService } from '@coreui/icons-angular';
import {
  brandSet, cilMenu, cilGamepad,
  cilFeaturedPlaylist, cilLaptop, cilSun, cilMoon, cilContrast,
  cilShieldAlt, cilVideogame, cilCast, cilSave,
  cilSync, cilBell, cilList,
  cilFile, cilBook, cilMagnifyingGlass, cilImage, cilMobile, cilAccountLogout, cilLockLocked
} from '@coreui/icons';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { delay, filter, map, tap } from 'rxjs/operators';
import { ManagerCache } from '../../../services/models/manager-cache';
import { ManagerHealth } from '../../../services/models/manager-health';
import { BehaviorSubject, Observable, Subscription, timer, interval } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-default-header',
  templateUrl: './default-header.component.html',
  standalone: true,
  imports: [ContainerComponent, HeaderTogglerDirective, SidebarToggleDirective, IconDirective, HeaderNavComponent, NavItemComponent, NavLinkDirective, RouterLink, RouterLinkActive, NgTemplateOutlet, BreadcrumbRouterComponent, ThemeDirective, DropdownComponent, DropdownToggleDirective, TextColorDirective, AvatarComponent, DropdownMenuDirective, DropdownHeaderDirective, DropdownItemDirective, BadgeComponent, DropdownDividerDirective, ProgressBarDirective, ProgressComponent, NgStyle, CommonModule],
  providers: [IconSetService]
})
export class DefaultHeaderComponent extends HeaderComponent implements OnInit, OnDestroy  {

  readonly #activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  readonly #colorModeService = inject(ColorModeService);
  readonly colorMode = this.#colorModeService.colorMode;
  readonly #destroyRef: DestroyRef = inject(DestroyRef);

  //public iconslib = { cilGamepad, cilShieldAlt, cilVideogame, cilLaptop, cilCast, cilSave, cilSync, ...brandSet };

  public loadcache : boolean = false
  public systemlist: Array<string> = new Array<string>();
  public playinggame: string = ""

  public managercache2$: Observable<ManagerCache> = this.mistersignalr.managerCacheRefresh$ //this.querygamesservice.cacheo
  subQueryGames!: Subscription;
  subManagerCache!: Subscription;
  
  readonly colorModes = [
    { name: 'light', text: 'Light', icon: 'cilSun' },
    { name: 'dark', text: 'Dark', icon: 'cilMoon' },
    { name: 'auto', text: 'Auto', icon: 'cilContrast' }
  ];

  readonly icons = computed(() => {
    const currentMode = this.colorMode();
    return this.colorModes.find(mode=> mode.name === currentMode)?.icon ?? 'cilSun';
  });

  constructor(private mistersignalr: MisterSignalrService, public iconSet: IconSetService, private auth : AuthService, private router : Router) { //private querygamesservice: QuerygamesService,
    super();

    iconSet.icons = {
      cilGamepad, cilFeaturedPlaylist, cilMenu, cilShieldAlt,
      cilVideogame, cilLaptop, cilSun, cilMoon, cilContrast, cilCast, cilSave, cilSync, cilBell, cilList,
      cilFile, cilBook, cilMagnifyingGlass, cilImage, cilMobile, cilAccountLogout, cilLockLocked, ...brandSet
    };
    this.#colorModeService.localStorageItemName.set('coreui-free-angular-admin-template-theme-default');
    this.#colorModeService.eventName.set('ColorSchemeChange');



    this.#activatedRoute.queryParams
      .pipe(
        delay(1),
        map(params => <string>params['theme']?.match(/^[A-Za-z0-9\s]+/)?.[0]),
        filter(theme => ['dark', 'light', 'auto'].includes(theme)),
        tap(theme => {
          this.colorMode.set(theme);
        }),
        takeUntilDestroyed(this.#destroyRef)
      )
      .subscribe();
  }
    ngOnInit(): void {
      
      this.subManagerCache = this.managercache2$.subscribe((w : ManagerCache) => {
        console.log("[DefaultHeaderComponent] - Update Cache Event")
        if (w.playingVideoGame.currentVideoGame && w.playingVideoGame.currentVideoGame.isPlaying) {
          this.playinggame = (w.playingVideoGame.currentVideoGame.currentVideogame ? w.playingVideoGame.currentVideoGame.currentVideogame.name : "");
        }
        else {
          this.playinggame = ""
        }
      });

      
    }

  

    ngOnDestroy(): void {
      this.subQueryGames?.unsubscribe();
      this.subManagerCache?.unsubscribe();
    }

  logout(): void {
    this.auth.Logout();
  }

  @Input() sidebarId: string = 'sidebar1';

  public newMessages = [
    {
      id: 0,
      from: 'Jessica Williams',
      avatar: '7.jpg',
      status: 'success',
      title: 'Urgent: System Maintenance Tonight',
      time: 'Just now',
      link: 'apps/email/inbox/message',
      message: 'Attention team, we\'ll be conducting critical system maintenance tonight from 10 PM to 2 AM. Plan accordingly...'
    },
    {
      id: 1,
      from: 'Richard Johnson',
      avatar: '6.jpg',
      status: 'warning',
      title: 'Project Update: Milestone Achieved',
      time: '5 minutes ago',
      link: 'apps/email/inbox/message',
      message: 'Kudos on hitting sales targets last quarter! Let\'s keep the momentum. New goals, new victories ahead...'
    },
    {
      id: 2,
      from: 'Angela Rodriguez',
      avatar: '5.jpg',
      status: 'danger',
      title: 'Social Media Campaign Launch',
      time: '1:52 PM',
      link: 'apps/email/inbox/message',
      message: 'Exciting news! Our new social media campaign goes live tomorrow. Brace yourselves for engagement...'
    },
    {
      id: 3,
      from: 'Jane Lewis',
      avatar: '4.jpg',
      status: 'info',
      title: 'Inventory Checkpoint',
      time: '4:03 AM',
      link: 'apps/email/inbox/message',
      message: 'Team, it\'s time for our monthly inventory check. Accurate counts ensure smooth operations. Let\'s nail it...'
    },
    {
      id: 3,
      from: 'Ryan Miller',
      avatar: '4.jpg',
      status: 'info',
      title: 'Customer Feedback Results',
      time: '3 days ago',
      link: 'apps/email/inbox/message',
      message: 'Our latest customer feedback is in. Let\'s analyze and discuss improvements for an even better service...'
    }
  ];

  public newNotifications = [
    { id: 0, title: 'New user registered', icon: 'cilUserFollow', color: 'success' },
    { id: 1, title: 'User deleted', icon: 'cilUserUnfollow', color: 'danger' },
    { id: 2, title: 'Sales report is ready', icon: 'cilChartPie', color: 'info' },
    { id: 3, title: 'New client', icon: 'cilBasket', color: 'primary' },
    { id: 4, title: 'Server overloaded', icon: 'cilSpeedometer', color: 'warning' }
  ];

  public newStatus = [
    { id: 0, title: 'CPU Usage', value: 25, color: 'info', details: '348 Processes. 1/4 Cores.' },
    { id: 1, title: 'Memory Usage', value: 70, color: 'warning', details: '11444GB/16384MB' },
    { id: 2, title: 'SSD 1 Usage', value: 90, color: 'danger', details: '243GB/256GB' }
  ];

  public newTasks = [
    { id: 0, title: 'Upgrade NPM', value: 0, color: 'info' },
    { id: 1, title: 'ReactJS Version', value: 25, color: 'danger' },
    { id: 2, title: 'VueJS Version', value: 50, color: 'warning' },
    { id: 3, title: 'Add new layouts', value: 75, color: 'info' },
    { id: 4, title: 'Angular Version', value: 100, color: 'success' }
  ];

}
