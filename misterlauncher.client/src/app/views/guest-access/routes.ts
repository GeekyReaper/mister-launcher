import { Routes } from '@angular/router';

import { GuestAccessComponent } from './guest-access.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'guestaccess',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Guest Access'
    },
    children: [
      {
        path: 'guestaccess',
        component: GuestAccessComponent,
        pathMatch: 'full',
        data: {
          title: 'Guest Access'
        }
      }
    ]
  }  
];
