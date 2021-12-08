import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxEchartsModule } from 'ngx-echarts';

import { DexRoutingModule } from './dex-routing.module';
import { ListComponent } from './list/list.component';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';
import { CreateComponent } from './create/create.component';
import { TradingComponent } from './trading/trading.component';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    TradingComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    SharedModule,
    DexRoutingModule,
    NgxEchartsModule.forRoot({
      echarts: () => import('echarts')
    })
  ]
})
export class DexModule { }
