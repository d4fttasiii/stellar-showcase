import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { UserAccountService } from 'src/app/core/services/user-account.service';

import { IssuerDto, IssuerTransferDto, UserAccountDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';

@Component({
  selector: 'app-transfer-asset',
  templateUrl: './transfer-asset.component.html',
  styleUrls: ['./transfer-asset.component.scss']
})
export class TransferAssetComponent implements OnInit {

  assetId: string;
  issuer: IssuerDto;
  userAccounts: UserAccountDto[];
  model: IssuerTransferDto;
  loaded = false;

  constructor(
    private issuerService: IssuerService,
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { }

  ngOnInit(): void {
    this.activatedRoute.params
      .subscribe(params => {
        this.model = {
          amount: 0,
          memo: '',
          userAccountId: '',
        };
        this.loadData(params['id']);
      });
  }

  submit() {
    this.issuerService
      .transferAsset(this.issuer.id, this.assetId, this.model)
      .subscribe(() => {
        this.snackBar.open(`Transferred ${this.model.amount}!`, 'OK', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          politeness: 'polite',
        });
      });
  }

  private loadData(id: string) {
    const q = forkJoin([
      this.issuerService.get(id),
      this.userAccountService.getAll()]);

    q.subscribe(results => {
      this.issuer = results[0];
      this.userAccounts = results[1];
      this.loaded = true;
    });
  }

}
