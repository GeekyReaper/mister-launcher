import { inject } from "@angular/core";
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from "./services/auth.service";
import { MisterSignalrService } from "./services/mister-signalr.service";

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  var urlrequested = state.url;
  
  console.log("--GUARD--");
  console.log(route)
  console.log(`Url requested : ${state.url}`)


  auth.CheckTokenStored();
  console.log(`AUTH usertype = ${auth.usertype}`)

  let isallow = (auth.usertype != "unknow")

  switch (urlrequested) {
    case "/mistersettings/settings":
    case "/guestaccess/guestaccess":
    case "/jobscan/scan/rom":
    case "/jobscan/matchingrom":
      console.log("admin only");
      isallow = (auth.usertype == "admin");
      break;
  }

  if (!isallow) {
    router.navigateByUrl('/login')
  }
  return isallow
  
};
