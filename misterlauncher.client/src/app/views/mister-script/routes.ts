import { Routes } from '@angular/router';

import { MisterScriptComponent } from './mister-script.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'script',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Script'
    },
    children: [
      {
        path: 'script',
        component: MisterScriptComponent,
        pathMatch: 'full',
        data: {
          title: 'Script'
        }
      }
    ]
  }  
];
