import { Routes } from '@angular/router';

import { JobScanComponent } from './job-scan.component';
import { JobScanRomComponent } from './job-scan-rom/job-scan-rom.component';
import { RomLinkComponent } from './rom-link/rom-link.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'matchingrom/automatic',
    pathMatch: 'full'
  },
  {
    path: '',
    data: {
      title: 'Job'
    },
    children: [
      {
        path: 'matchingrom/:tab',
        component: JobScanComponent,
        pathMatch: 'full',
        data: {
          title: 'Matching Rom'
        }
      },
      {
        path: 'scan/:tab',
        component: JobScanRomComponent,
        pathMatch: 'full',
        data: {
          title: 'Scan Rom'
        }
      },
      {
        path: 'scan/:tab/:system',
        component: JobScanRomComponent,
        pathMatch: 'full',
        data: {
          title: 'Scan Rom'
        }
      },
      {
        path: 'matchingrom/:tab/:system',
        component: JobScanComponent,
        pathMatch: 'full',
        data: {
          title: 'Matching Rom'
        }
      },
      {
        path: 'matchingrom',
        component: JobScanComponent,
        pathMatch: 'full',
        data: {
          title: 'Matching Rom'
        }
      },
      {
        path: 'rom/:id',
        component: RomLinkComponent,
        pathMatch: 'full',
        data: {
          title: 'Rom'
        }
      }

    ]
  }  
];
