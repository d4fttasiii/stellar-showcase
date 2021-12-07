import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';

import { ApiService } from './services/api.service';
import { AssetService } from './services/asset.service';
import { DexService } from './services/dex.service';
import { IssuerService } from './services/issuer.service';
import { UserAccountService } from './services/user-account.service';

@NgModule({
  declarations: [],
  providers: [
    ApiService, 
    UserAccountService, 
    IssuerService, 
    AssetService,
    DexService],
  imports: [
    CommonModule,
    HttpClientModule,
  ]
})
export class CoreModule { }
