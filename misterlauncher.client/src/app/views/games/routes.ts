import { Routes } from '@angular/router';

import { GamesComponent } from './games.component';
import { GameDetailComponent } from './game-detail/game-detail.component'

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'search',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Games'
    },
    children: [
      {
        path: 'search',
        component: GamesComponent,
        pathMatch: 'full',
        data: {
          title: 'Search'
        }
      },
      {
        path: 'id/:id',
        component: GameDetailComponent,
        data: {
          title: 'Details'
        }
      }
    ]
  }  
];
