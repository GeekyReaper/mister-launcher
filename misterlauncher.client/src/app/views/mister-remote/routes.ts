import { Routes } from '@angular/router';

import { MisterRemoteComponent } from './mister-remote.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'remote',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Remote'
    },
    children: [
      {
        path: 'remote',
        component: MisterRemoteComponent,
        pathMatch: 'full',
        data: {
          title: 'Remote'
        }
      }
    ]
  }  
];
