import { Component } from '@angular/core';
import { ROUTER_DIRECTIVES } from '@angular/router';
import { HTTP_PROVIDERS } from '@angular/http';

import { CountersComponent } from './counters';
import { CounterService } from './counters/counter.service';

import {MdToolbar} from '@angular2-material/toolbar';
import {MdButton} from '@angular2-material/button';
import {MD_SIDENAV_DIRECTIVES} from '@angular2-material/sidenav';
import {MD_LIST_DIRECTIVES} from '@angular2-material/list';
import {MD_CARD_DIRECTIVES} from '@angular2-material/card';
import {MdInput} from '@angular2-material/input';
import {MdCheckbox} from '@angular2-material/checkbox';
import {MdRadioButton, MdRadioGroup} from '@angular2-material/radio';
import {MdIcon, MdIconRegistry} from '@angular2-material/icon';

@Component({
  moduleId: module.id,
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.css'],
  directives: [
    MD_SIDENAV_DIRECTIVES,
    MD_LIST_DIRECTIVES,
    MD_CARD_DIRECTIVES,
    MdToolbar,
    MdButton,
    MdInput,
    MdCheckbox,
    MdRadioGroup,
    MdRadioButton,
    MdIcon,
    ROUTER_DIRECTIVES
  ],
  providers: [
    HTTP_PROVIDERS,
    MdIconRegistry,
    CounterService
  ],
})
export class AppComponent {
  title = 'polykube.io';

  navitems: Object[] = [
    {
      name: "about",
      description: "view the about info",
      icon: "info",
      path: "/about"
    },
    {
      name: "counters",
      description: "view the counters",
      icon: "equalizer",
      path: "/counters"
    },
    {
      name: "guestbook",
      description: "view the guestbook",
      icon: "list",
      path: "/guestbook"
    }
  ];
}
