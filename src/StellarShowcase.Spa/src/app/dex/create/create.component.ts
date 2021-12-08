import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { ComponentBase } from '../../core/component-base';
import { AssetDto, CreateMarketDto } from '../../core/models/dto';
import { AssetService } from '../../core/services/asset.service';
import { DexService } from '../../core/services/dex.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent extends ComponentBase implements OnInit {

  assets: AssetDto[];
  baseAsset: AssetDto;
  quoteAsset: AssetDto;

  constructor(
    private assetService: AssetService,
    private dexService: DexService,
    private snackBar: MatSnackBar) {
    super();
  }

  ngOnInit(): void {
    this.loadData();
  }

  submit() {
    const model: CreateMarketDto = {
      baseAssetId: this.baseAsset.id,
      quoteAssetId: this.quoteAsset.id,
      name: `${this.baseAsset.unitName}-${this.quoteAsset.unitName}`,
    };
    this.isSubmitting = true;
    this.dexService
      .createMarket(model)
      .subscribe({
        next: () => {
          this.snackBar.open(`Market: ${model.name} created!`, 'OK', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            politeness: 'polite',
          });
        },
        complete: () => this.stopSubmitting()
      });
  }

  private loadData() {
    this.isLoading = true;
    this.assetService
      .getAll()
      .subscribe({
        next: assets => {
          this.assets = assets;
        },
        complete: () => this.stopLoading(),
      });
  }

}
