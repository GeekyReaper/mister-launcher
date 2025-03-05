import { Routes } from '@angular/router';

import { TestComponent } from './test.component';

export const routes: Routes = [
  {
    path: '',
    component: TestComponent,
    data: {
      title: 'Test'
    }
  }
];
