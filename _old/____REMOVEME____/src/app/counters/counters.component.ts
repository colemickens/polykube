import { Component, OnInit } from '@angular/core';

import { Counter } from './counter.ts'
import { CounterService } from './counter.service'

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
  selector: 'app-counters',
  templateUrl: 'counters.component.html',
  styleUrls: ['counters.component.css'],
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
    MdIcon
  ],
  providers: [MdIconRegistry],
})
export class CountersComponent implements OnInit {
  counters: Counter[];
  selectedCounter: Counter;

  constructor(
    private counterService: CounterService) {}

  ngOnInit() {
    this.getCounters();
  }

  getCounters() {
    this.counterService.getCounters()
      .then(counters => this.counters = counters);
  }
}
