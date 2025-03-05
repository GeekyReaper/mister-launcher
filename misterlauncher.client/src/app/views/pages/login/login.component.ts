import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule, NgStyle, Location } from '@angular/common';
import { IconDirective } from '@coreui/icons-angular';
import { FormBuilder, FormControl, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormGroupDirective } from '@angular/forms';
import { Observable, Subscription, interval } from 'rxjs';
import { AuthService } from '../../../services/auth.service';

import {
  ContainerComponent, RowComponent, ColComponent, CardGroupComponent, TextColorDirective,
  BadgeComponent, BadgeModule,
  CardComponent, CardBodyComponent, InputGroupComponent, InputGroupTextDirective,
  ButtonDirective,
  FormDirective, FormLabelDirective, FormControlDirective,
  AlertComponent, AlertModule,
  SpinnerComponent, SpinnerModule
} from '@coreui/angular';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { LoginResult } from '../../../services/models/login-result';
import { HttpErrorResponse } from '@angular/common/http';
import { MisterSignalrService } from '../../../services/mister-signalr.service';
import { ManagerCache } from '../../../services/models/manager-cache';
import { GuestAccess } from '../../../services/models/guest-access';


@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss'],
    standalone: true,
  imports: [ContainerComponent, RowComponent, BadgeComponent, BadgeModule, ColComponent, CardGroupComponent, TextColorDirective, CardComponent, CardBodyComponent,
    ReactiveFormsModule, FormsModule, FormDirective, FormLabelDirective, FormControlDirective,
    InputGroupComponent, InputGroupTextDirective, IconDirective, ButtonDirective, NgStyle,
    AlertComponent, AlertModule, SpinnerComponent, SpinnerModule,
    CommonModule]
})
export class LoginComponent implements OnInit, OnDestroy {

  subLogin?: Subscription;
  subLoginFail?: Subscription;
  subRequestAccess?: Subscription;
  subCheckRequestAccess?: Subscription;
  subGuestAccessApprouved?: Subscription;
  subGuestAccessConsumed?: Subscription;
  subManagercacheUpdate?: Subscription;
  subRequesteDenied?: Subscription;

  formLogin!: FormGroup;
  loginError: Boolean = false;

  requestAccess?: GuestAccess;


  public managercache2$: Observable<ManagerCache> = this.mistersignalr.managerCacheRefresh$

  constructor(private formBuilder: FormBuilder, private auth: AuthService, private router: Router, private mistersignalr: MisterSignalrService, private route: ActivatedRoute) { }
    ngOnDestroy(): void {
      this.subManagercacheUpdate?.unsubscribe();
      this.subLogin?.unsubscribe();
      this.subRequestAccess?.unsubscribe();
      this.subLoginFail?.unsubscribe();
      this.subCheckRequestAccess?.unsubscribe();
      this.subRequesteDenied?.unsubscribe();
    }


  ngOnInit(): void {

    this.route.params.subscribe(
      params => {
        const cmd = params['cmd'];
        console.log(`login parameters ${cmd}`)
        if (cmd == "out") {

          this.auth.Logout();
          this.router.navigateByUrl('/login');
        }        
        });
       

    this.formLogin = this.formBuilder.group(
      {
        password: ['', [Validators.required, Validators.minLength(1)]]
      }     
    );
    this.subManagercacheUpdate = this.managercache2$.subscribe((w: ManagerCache) => {
      console.log("##cache update")
      console.log(w);
      if (w.health.misterState == "ERROR") {
        this.subManagercacheUpdate?.unsubscribe();
        this.router.navigateByUrl('/500');
      }
    });

    // check if authentication is setup
    this.subLogin = this.auth.api_loginwithoutauthentication().subscribe((result: LoginResult) => {
      if (result.token != "") {
        console.debug("authentication is not necessary");
        this.auth.SaveToken(result);
        console.log("**** redirect to videogamesearch");
        this.router.navigateByUrl("/videogames/search");
      }
    });



    //console.log(`Current token ${this.auth.GetToken()}`)
  }

  login() {

    this.subLogin = this.auth.api_login(this.formLogin?.value.password).subscribe((result: LoginResult) => {
      console.debug(`== login result ==`)
      console.debug(result)
      this.formLogin?.reset();
      if (result.token != "") {
        console.debug("redirect");
        this.auth.SaveToken(result);
        this.loginError = false;
        this.ngOnDestroy();
        console.log("**** redirect to videogamesearch");
        this.router.navigateByUrl("/videogames/search");
        
      }
      else {
        this.loginError = true;
        this.subLoginFail = interval(5000).subscribe(() => {
          console.log("loginerror reset")
          this.loginError = false;
          this.subLoginFail?.unsubscribe();
        });

      }
      this.subLogin?.unsubscribe();

    }, (error: HttpErrorResponse): void => {
      console.debug("== login error ==")
      console.debug(error);
      if (error.status == 401) {

        this.loginError = true;
        this.subLoginFail = interval(5000).subscribe(() => {
          console.log("loginerror reset")
          this.loginError = false;
          this.subLoginFail?.unsubscribe();
        });
        this.formLogin?.reset();
      }
      this.subLogin?.unsubscribe();
    });
    

    
    

  }

  request_access() {
    var key = this.auth.generatekey(10);
    this.subRequestAccess = this.auth.api_requestguestaccess(key).subscribe((ga: GuestAccess) => {
      ga.clientkey = key;      
      this.requestAccess = ga;
      console.log("-- Request Access");
      console.log(this.requestAccess);
      this.subRequestAccess?.unsubscribe();
      this.checkrequestAccessStates();
    })

  }

  

  checkrequestAccessStates() {

    this.subCheckRequestAccess = interval(5000).subscribe(() => {

      console.log("-- CHECK request access")
      if (this.requestAccess == undefined) {
        console.log("STOP pooling from checkrequestAccessStates")
        this.subCheckRequestAccess?.unsubscribe() // Stop pooling
        return;
      }

      this.subGuestAccessApprouved = this.auth.api_guestaccessstate(this.requestAccess.code).subscribe((state: string) => {
        console.log(`GusetAccessState : ${state} for request ${this.requestAccess?.code})`);
        if (state == "NOTFOUND") {
          this.requestAccess = undefined;          
          return;
        }
        if (this.requestAccess != undefined && this.requestAccess.clientkey != undefined) {
          this.requestAccess.state = state;
          if (state == "APPROUVED") {
            this.consumeRequestAccess();
          }
          if (state == "DENIED" || state == "BLOCK") {
            this.subRequesteDenied = interval(5000).subscribe(() => {
              this.requestAccess = undefined;
              this.subRequesteDenied?.unsubscribe();
            })
          }
        }
        this.subGuestAccessApprouved?.unsubscribe();
      });

    });
  }

  consumeRequestAccess() {

    if (this.requestAccess != undefined && this.requestAccess.clientkey != undefined && this.requestAccess.state == "APPROUVED") {
        this.subGuestAccessConsumed = this.auth.api_guestaccessconsumed(this.requestAccess.code, this.requestAccess.clientkey).subscribe((result: LoginResult) => {
        if (result.token != "") {
          this.auth.SaveToken(result);
          console.log("**** redirect to videogamesearch");
          this.router.navigateByUrl("/videogames/search");
          console.log("STOP pooling from consumeRequestAccess")
          this.subCheckRequestAccess?.unsubscribe(); // stop pooling
        }
        this.subGuestAccessConsumed?.unsubscribe();

      });
    }
  }

  guestAccessBadgeColor(state: string): string {
    switch (state) {
      case "PENDING":
        return "info";
      case "DENIED":
        return "danger";
      case "BLOCK":
        return "warning";
      case "APPROUVED":
        return "warning";
      case "CONSUMED":
        return "success";
    }
    return "info";
  }

}
