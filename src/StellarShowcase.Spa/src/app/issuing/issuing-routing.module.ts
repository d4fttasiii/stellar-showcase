import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateAssetComponent } from './create-asset/create-asset.component';

import { DetailsComponent } from './details/details.component';
import { OverviewComponent } from './overview/overview.component';
import { TransferAssetComponent } from './transfer-asset/transfer-asset.component';

const routes: Routes = [{
  path: '',
  component: OverviewComponent,
}, {
  path: ':id',
  component: DetailsComponent,
}, {
  path: ':id/create-asset',
  component: CreateAssetComponent,
}, {
  path: ':id/transfer-asset',
  component: TransferAssetComponent,
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IssuingRoutingModule { }
