
import { Routes } from '@angular/router';
import { CardListComponent } from './components/card-list/card-list.component';
import { CardFormComponent } from './components/card-form/card-form.component';

export const APP_ROUTES: Routes = [
  {
    path: 'cards',
    component: CardListComponent,
    title: 'Business Cards'
  },
  {
    path: 'cards/new',
    component: CardFormComponent,
    title: 'Add New Card'
  },
  {
    path: '',
    redirectTo: '/cards',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/cards'
  }
];
