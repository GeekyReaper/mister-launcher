import { Routes } from '@angular/router';

import { MisterSettingsComponent } from './mister-settings.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'settings',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Settings'
    },
    children: [
      //{
      //  path: 'settings',
      //  component: MisterSettingsComponent,
      //  pathMatch: 'full',
      //  data: {
      //    title: 'Settings'
      //  }
      //},
      {
        path: ':tab',
        component: MisterSettingsComponent,
        pathMatch: 'full',
        data: {
          title: 'Settings'
        }
      }
    ]
  }  
];
