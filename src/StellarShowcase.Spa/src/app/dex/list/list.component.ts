import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ComponentBase } from '../../core/component-base';

import { MarketDto } from '../../core/models/dto';
import { DexService } from '../../core/services/dex.service';

interface MarketDtoEx extends MarketDto {
  nr: number;
}

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent extends ComponentBase implements OnInit {

  markets: MarketDtoEx[];
  columns = ['nr', 'name', 'price', 'action'];

  constructor(
    private dexService: DexService,
    private router: Router) {
      super();
     }

  ngOnInit(): void {
    this.loadData();
  }

  goToCreateMarket() {
    this.router.navigate(['dex', 'create-market']);
  }

  goToTrading(id: string) {
    this.router.navigate(['dex', id, 'trading']);
  }

  private loadData() {
    this.isLoading = true;
    this.dexService
      .getAll()
      .subscribe({
        next: (markets) => {
          this.markets = [];
          let counter = 0;
          markets.forEach(m => {
            const market = m as MarketDtoEx;
            market.nr = ++counter;
            this.markets.push(market);
          });
        },
        complete: () => this.stopLoading(),
      });
  }

}
