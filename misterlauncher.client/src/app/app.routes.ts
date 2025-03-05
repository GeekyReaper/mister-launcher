import { authGuard } from './auth.guard';
import { Routes } from '@angular/router';
import { DefaultLayoutComponent } from './layout';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: '',
    component: DefaultLayoutComponent,
    data: {
      title: 'Home'
    },
    children: [
      //{
      //  path: 'dashboard',
      //  loadChildren: () => import('./views/dashboard/routes').then((m) => m.routes)
      //},     
      {
        path: 'videogames',
        loadChildren: () => import('./views/videogames/routes').then((m) => m.routes),
        canActivate : [authGuard]
      },
      {
        path: 'systems',
        loadChildren: () => import('./views/systems/routes').then((m) => m.routes),
        canActivate: [authGuard]

      },
      {
        path: 'misterremote',
        loadChildren: () => import('./views/mister-remote/routes').then((m) => m.routes),
        canActivate: [authGuard]
      },
      {
        path: 'guestaccess',
        loadChildren: () => import('./views/guest-access/routes').then((m) => m.routes),
        canActivate: [authGuard]
      },
      {
        path: 'script',
        loadChildren: () => import('./views/mister-script/routes').then((m) => m.routes),
        canActivate: [authGuard]
      },
      {
        path: 'mistersettings',
        loadChildren: () => import('./views/mister-settings/routes').then((m) => m.routes),
        canActivate: [authGuard]
      },
      {
        path: 'jobscan',
        loadChildren: () => import('./views/job-scan/routes').then((m) => m.routes),
        canActivate: [authGuard]
      },     
      {
        path: 'pages',
        loadChildren: () => import('./views/pages/routes').then((m) => m.routes)
      }
    ]
  },
  {
    path: '404',
    loadComponent: () => import('./views/pages/page404/page404.component').then(m => m.Page404Component),
    data: {
      title: 'Page 404'
    }
  },
  {
    path: '500',
    loadComponent: () => import('./views/pages/page500/page500.component').then(m => m.Page500Component),
    data: {
      title: 'Page 500'
    }
  },
  {
    path: 'apierror',
    loadComponent: () => import('./views/pages/apierror/apierror.component').then(m => m.ApierrorComponent),
    data: {
      title: 'Api Unavailable'
    }
  },
  {
    path: 'login',
    loadComponent: () => import('./views/pages/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Login Page'
    }
  },
  {
    path: 'login/:cmd',
    loadComponent: () => import('./views/pages/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Login Page'
    }
  },
  {
    path: 'register',
    loadComponent: () => import('./views/pages/register/register.component').then(m => m.RegisterComponent),
    data: {
      title: 'Register Page'
    }
  },
  { path: '**', redirectTo: 'login' }
];
