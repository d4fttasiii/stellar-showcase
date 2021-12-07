import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IssuingRoutingModule } from './issuing-routing.module';
import { OverviewComponent } from './overview/overview.component';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';
import { DetailsComponent } from './details/details.component';
import { CreateAssetComponent } from './create-asset/create-asset.component';
import { TransferAssetComponent } from './transfer-asset/transfer-asset.component';

@NgModule({
  declarations: [
    OverviewComponent,
    DetailsComponent,
    CreateAssetComponent,
    TransferAssetComponent,
  ],
  imports: [
    CommonModule,
    CoreModule,
    SharedModule,
    IssuingRoutingModule
  ]
})
export class IssuingModule { }
