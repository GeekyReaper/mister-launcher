import { Routes } from '@angular/router';

import { SystemsComponent } from './systems.component';
import { SystemDetailComponent } from './system-detail/system-detail.component'

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'list',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Systems'
    },
    children: [
      {
        path: 'list',
        component: SystemsComponent,
        pathMatch: 'full',
        data: {
          title: 'List'
        }
      },
      {
        path: 'console',
        component: SystemsComponent,
        pathMatch: 'full',
        data: {
          title: 'console'
        }
      },
      {
        path: 'arcade',
        component: SystemsComponent,
        pathMatch: 'full',
        data: {
          title: 'arcade'
        }
      },
      {
        path: 'sys/:id',
        component: SystemDetailComponent,
        data: {
          title: 'Details'
        }
      },
      {
        path: 'arcade/:id',
        component: SystemDetailComponent,
        data: {
          title: 'Details'
        }
      },      
      {
        path: 'console/:id',
        component: SystemDetailComponent,
        data: {
          title: 'Details'
        }
      }
    ]
  }  
];
