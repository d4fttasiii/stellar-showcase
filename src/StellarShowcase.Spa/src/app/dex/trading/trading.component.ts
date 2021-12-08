import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ActiveOrderDto, CreateBuyOrderDto, CreateSellOrderDto, MarketDto, UserAccountDto } from '../../core/models/dto';
import { DexService } from '../../core/services/dex.service';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-trading',
  templateUrl: './trading.component.html',
  styleUrls: ['./trading.component.scss']
})
export class TradingComponent implements OnInit {

  loaded = false;
  market: MarketDto;
  userAccounts: UserAccountDto[];
  impersonatedUserAccount: UserAccountDto;
  userAccountActiveOrders: ActiveOrderDto[];
  model: CreateSellOrderDto | CreateBuyOrderDto;
  isBuy: boolean;

  chartOptions: any;
  orderColumns = ['id', 'buying', 'selling', 'volume', 'price', 'total', 'action'];
  orderbookColumns = ['volume', 'price', 'total'];

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

  impersonate(event: any) {
    this.impersonatedUserAccount = this.userAccounts.find(u => u.id === event.value);
    this.reloadActiveOrders(this.impersonatedUserAccount.id);
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

  public cancelOrder(orderId: number) {
    this.userAccountService
      .cancelOrder(this.impersonatedUserAccount.id, orderId)
      .subscribe(() => {
        this.snackBar.open(`Order ${orderId} cancelled!`, 'OK', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          politeness: 'polite',
        });
      });
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

  private reloadActiveOrders(id: string) {
    this.userAccountService
      .getActiverOrders(id)
      .subscribe(
        (orders) => this.userAccountActiveOrders = orders.filter(o => o.marketId == this.market.id),
      );
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
      tooltip: {
      },
      legend: {
        show: false,
      },
      grid: {
        left: '0%',
        right: '0%',
        bottom: '0%',
        top: '0%',
        containLabel: true
      },
      xAxis: {
        type: 'value',
        // boundaryGap: [0, 0.01]
      },
      yAxis: {
        type: 'category',
        data: xAxisData
      },
      series: [
        {
          name: 'Buy',
          type: 'bar',
          data: yAxisDataBuy
        },
        {
          name: 'Sell',
          type: 'bar',
          data: yAxisDataSell
        }
      ]
    };
  }
}
