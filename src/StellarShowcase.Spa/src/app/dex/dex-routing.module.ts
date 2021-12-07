import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CreateComponent } from './create/create.component';
import { ListComponent } from './list/list.component';
import { TradingComponent } from './trading/trading.component';

const routes: Routes = [
  { path: '', component: ListComponent },
  { path: 'create-market', component: CreateComponent, },
  { path: ':id/trading', component: TradingComponent, },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DexRoutingModule { }
