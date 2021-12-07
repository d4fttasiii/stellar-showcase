import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

import { AssetDto } from '../../core/models/dto';
import { AssetService } from '../../core/services/asset.service';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-add-asset',
  templateUrl: './add-asset.component.html',
  styleUrls: ['./add-asset.component.scss']
})
export class AddAssetComponent implements OnInit {

  private id: string;
  selectedAsset: AssetDto;
  assets: AssetDto[];
  loaded = false;

  constructor(
    private assetService: AssetService,
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { }

  ngOnInit(): void {
    this.activatedRoute.params
      .subscribe(params => {
        this.id = params['id'];
        this.loadData();
      });
  }

  submit() {
    this.userAccountService
      .createTrustline(this.id, this.selectedAsset.id, this.selectedAsset.issuerId)
      .subscribe(() => this.snackBar.open(`Asset: ${this.selectedAsset.unitName} added!`, 'OK', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        politeness: 'polite',
      }));
  }

  private loadData() {
    this.assetService
      .getAll()
      .subscribe((result) => {
        this.assets = result;
        this.loaded = true;
      });
  }
}
