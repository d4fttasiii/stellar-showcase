import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';

import { ComponentBase } from '../../core/component-base';
import { DialogCredentials } from '../../core/models/dialog-credentials';
import { ActiveOrderDto, CreateBuyOrderDto, CreateSellOrderDto, MarketDto, UserAccountDto } from '../../core/models/dto';
import { DexService } from '../../core/services/dex.service';
import { UserAccountService } from '../../core/services/user-account.service';
import { CredentialsDialogComponent } from '../../shared/credentials-dialog/credentials-dialog.component';

@Component({
  selector: 'app-trading',
  templateUrl: './trading.component.html',
  styleUrls: ['./trading.component.scss']
})
export class TradingComponent extends ComponentBase implements OnInit {

  market: MarketDto;
  userAccounts: UserAccountDto[];
  impersonatedUserAccount: UserAccountDto;
  userAccountActiveOrders: ActiveOrderDto[];
  model: CreateSellOrderDto | CreateBuyOrderDto;
  isBuy: boolean;

  chartOptions: any;
  orderColumns = ['id', 'buying', 'selling', 'volume', 'price', 'total', 'action'];

  constructor(
    private userAccountService: UserAccountService,
    private dexService: DexService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private dialog: MatDialog) {
    super();
  }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(
      (params) => {
        this.model = {
          marketId: params['id'],
          price: null,
          volume: null,
          passphrase: ''
        };
        this.loadData(params['id']);
      },
    );
  }

  impersonate(event: any) {
    this.impersonatedUserAccount = this.userAccounts.find(u => u.id === event.value);
    this.reloadActiveOrders(this.impersonatedUserAccount.id);
  }

  toggle(idx: number) {
    if (idx > 1)
      return;

    this.isBuy = idx === 0;
  }
  
  openDialog() {
    let passphrase = this.userAccountService.getPassphrase(this.impersonatedUserAccount.id);
    if (!passphrase) {
      const dialogRef = this.dialog.open(CredentialsDialogComponent, {
        data: this.impersonatedUserAccount.id,
      });

      dialogRef.afterClosed().subscribe((result: DialogCredentials) => {
        if (result && result.passphrase) {
          this.submit(result.passphrase);
        }
      });
    } else {
      this.submit(passphrase);
    }
  }

  private submit(passphrase: string) {
    this.isSubmitting = true;
    this.model.passphrase = passphrase;
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
          complete: () => {
            this.stopSubmitting();
            this.reloadMarketData();
          },
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
          complete: () => {
            this.stopSubmitting();
            this.reloadMarketData();
          },
        });
    }
  }

  cancelOrder(orderId: number) {
    this.isSubmitting = true;
    this.userAccountService
      .cancelOrder(this.impersonatedUserAccount.id, orderId)
      .subscribe({
        next: () => {
          this.snackBar.open(`Order ${orderId} cancelled!`, 'OK', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            politeness: 'polite',
          });
        },
        complete: () => this.stopSubmitting(),
      });
  }

  private loadData(id: string) {
    const q = forkJoin([
      this.dexService.get(id),
      this.userAccountService.getAll()]);

    q.subscribe({
      next: results => {
        this.market = results[0];
        this.userAccounts = results[1];
        this.rebuildChartData();
      },
      complete: () => this.stopLoading(),
    });
  }

  private reloadActiveOrders(id: string) {
    this.userAccountActiveOrders = [];
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
        top: '5%',
        containLabel: true
      },
      xAxis: {
        type: 'category',
        data: xAxisData
        // boundaryGap: [0, 0.01]
      },
      yAxis: {
        type: 'value',
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
