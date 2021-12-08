import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';

import { IssuerDto, IssuerTransferDto, UserAccountDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';
import { UserAccountService } from '../../core/services/user-account.service';

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
  isLoading = true;

  constructor(
    private issuerService: IssuerService,
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.model = {
      amount: 0,
      memo: '',
      userAccountId: '',
    };
    this.activatedRoute.params
      .subscribe(params => {
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
    this.isLoading = true;
    const q = forkJoin([
      this.issuerService.get(id),
      this.userAccountService.getAll()]);

    q.subscribe({
      next: results => {
        this.issuer = results[0];
        this.userAccounts = results[1];
      },
      complete: () => setTimeout(() => this.isLoading = false, 600),
    });
  }

}
