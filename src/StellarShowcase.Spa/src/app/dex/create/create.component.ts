import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AssetDto, CreateMarketDto } from '../../core/models/dto';
import { AssetService } from '../../core/services/asset.service';
import { DexService } from '../../core/services/dex.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  assets: AssetDto[];
  baseAsset: AssetDto;
  quoteAsset: AssetDto;
  loaded = false;

  constructor(
    private assetService: AssetService,
    private dexService: DexService,
    private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.loadData();
  }

  submit() {
    const model: CreateMarketDto = {
      baseAssetId: this.baseAsset.id,
      quoteAssetId: this.quoteAsset.id,
      name: `${this.baseAsset.unitName}-${this.quoteAsset.unitName}`,
    };
    this.dexService
      .createMarket(model)
      .subscribe(() => {
        this.snackBar.open(`Market: ${model.name} created!`, 'OK', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          politeness: 'polite',
        });
      });
  }

  private loadData() {
    this.assetService
      .getAll()
      .subscribe(assets => {
        this.assets = assets;
        this.loaded = true;
      });
  }

}
