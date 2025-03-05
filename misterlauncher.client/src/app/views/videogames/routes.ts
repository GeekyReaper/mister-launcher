import { Routes } from '@angular/router';

import { VideogamesComponent } from './videogames.component';
import { VideogameDetailComponent } from './videogame-detail/videogame-detail.component'
import { VideogamePlaylistComponent } from './videogame-playlist/videogame-playlist.component'

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'search',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'VideoGames'
    },
    children: [
      {
        path: 'search',
        component: VideogamesComponent,
        pathMatch: 'full',
        data: {
          title: 'Search'
        }
      },
      {
        path: 'id/:id',
        component: VideogameDetailComponent,
        data: {
          title: 'Details'
        }
      },     
      {
        path: 'playlist',
        component: VideogamePlaylistComponent,
        data: {
          title: 'Playlist'
        }
      }
    ]
  }  
];
