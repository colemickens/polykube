import { provideRouter, RouterConfig } from '@angular/router';

import { AboutRoutes } from './about/about.routes';
import { CountersRoutes } from './counters/counters.routes';
import { GuestbookRoutes } from './guestbook/guestbook.routes';

export const routes: RouterConfig = [
  ...CountersRoutes,
  ...AboutRoutes,
  ...GuestbookRoutes,
  {
    path: '',
    redirectTo: '/about',
    pathMatch: 'full'
  }
];

export const APP_ROUTER_PROVIDERS = [
  provideRouter(routes)
];
