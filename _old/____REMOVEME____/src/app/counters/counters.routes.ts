import { RouterConfig }          from '@angular/router';
import { CountersComponent }     from './counters.component';

export const CountersRoutes: RouterConfig = [
  { path: 'counters',  component: CountersComponent },
  //{ path: 'counters/:id', component: CounterDetailComponent }
];
