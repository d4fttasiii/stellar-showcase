import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { MarketDto, UserAccountDto } from '../../core/models/dto';
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
  loaded = false;

  constructor(
    private userAccountService: UserAccountService,
    private dexService: DexService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(
      (params) => this.loadData(params['id']),
    );
  }

  impersonate(userAccount: UserAccountDto) {
    this.impersonatedUserAccount = userAccount;
  }

  private loadData(id: string) {
    const q = forkJoin([
      this.dexService.get(id),
      this.userAccountService.getAll()]);

    q.subscribe(results => {
      this.market = results[0];
      this.userAccounts = results[1];
      this.loaded = true;
    });
  }
}
