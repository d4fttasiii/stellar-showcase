import { Component, OnInit, ViewChild } from '@angular/core';
import { MatSidenav } from '@angular/material/sidenav';

import { MenuItem } from './core/models/menu-item';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  @ViewChild(MatSidenav)
  sidenav!: MatSidenav;
  menuItems: MenuItem[];

  ngOnInit(): void {
    this.menuItems = [
      {
        icon: 'fa-home',
        label: 'Home',
        route: '',
      },
      {
        icon: 'fa-users',
        label: 'User',
        route: 'user',
      },
      {
        icon: 'fa-money-check-alt',
        label: 'Issuing',
        route: 'issuing',
      },
      {
        icon: 'fa-exchange-alt',
        label: 'DEX',
        route: 'dex',
      },
    ];
  }

  ngAfterViewInit() {
    this.sidenav.mode = 'over';
    this.sidenav.close();
  }
}
