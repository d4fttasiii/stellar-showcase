import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { CreateBuyOrderDto, CreateSellOrderDto, MarketDto, UserAccountDto } from '../../core/models/dto';
import { DexService } from '../../core/services/dex.service';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-trading',
  templateUrl: './trading.component.html',
  styleUrls: ['./trading.component.scss']
})
export class TradingComponent implements OnInit {

  market: MarketDto;
  userAccounts: UserAccountDto[];
  impersonatedUserAccount: UserAccountDto;
  model: CreateSellOrderDto | CreateBuyOrderDto;
  isBuy: boolean;
  chartOptions: any;
  loaded = false;

  constructor(
    private userAccountService: UserAccountService,
    private dexService: DexService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(
      (params) => {
        this.model = {
          marketId: params['id'],
          price: null,
          volume: null,
        };
        this.loadData(params['id']);
      },
    );
  }

  impersonate(userAccount: UserAccountDto) {
    this.impersonatedUserAccount = userAccount;
  }

  stopImpersonating() {
    this.impersonatedUserAccount = undefined;
  }

  toggle() {
    this.isBuy = !this.isBuy;
  }

  submit() {
    if (this.isBuy) {
      this.userAccountService
        .createBuyOrder(this.impersonatedUserAccount.id, this.model)
        .subscribe({
          next: () => {
            this.snackBar.open(`Buy order created: ${this.model.volume} for ${this.model.price}!`, 'OK', {
              duration: 5000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              politeness: 'polite',
            });
          },
          complete: () => this.reloadMarketData(),
        });
    }
    else {
      this.userAccountService
        .createSellOrder(this.impersonatedUserAccount.id, this.model)
        .subscribe({
          next: () => {
            this.snackBar.open(`Sell order created: ${this.model.volume} for ${this.model.price}!`, 'OK', {
              duration: 5000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              politeness: 'polite',
            });
          },
          complete: () => this.reloadMarketData()
        });
    }
  }

  private loadData(id: string) {
    const q = forkJoin([
      this.dexService.get(id),
      this.userAccountService.getAll()]);

    q.subscribe(results => {
      this.market = results[0];
      this.userAccounts = results[1];
      this.rebuildChartData();
      this.loaded = true;
    });
  }

  private reloadMarketData() {
    this.dexService
      .get(this.market.id)
      .subscribe(market => {
        this.market = market;
        this.rebuildChartData();
      })
  }

  private rebuildChartData() {
    const xAxisData = [];
    const yAxisDataBuy = [];
    const yAxisDataSell = [];

    this.market.orderBooks.buys.forEach(b => {
      xAxisData.push(b.price);
      yAxisDataBuy.push(b.volume);
      yAxisDataSell.push(null);
    });

    this.market.orderBooks.sells.forEach(s => {
      xAxisData.push(s.price);
      yAxisDataSell.push(s.volume);
    });

    this.chartOptions = {
      legend: {
        data: ['Buy', 'Sell'],
        // align: 'top',
      },
      tooltip: {},
      xAxis: {
        data: xAxisData,
        silent: false,
        splitLine: {
          show: false,
        },
      },
      yAxis: {},
      series: [
        {
          name: 'Buy Volume',
          type: 'bar',
          data: yAxisDataBuy,
          animationDelay: (idx) => idx * 10,
        }, {
          name: 'Sell Volume',
          type: 'bar',
          data: yAxisDataSell,
          animationDelay: (idx) => idx * 10,
        },
      ],
      animationEasing: 'elasticOut',
      animationDelayUpdate: (idx) => idx * 5,
    };
  }
}
