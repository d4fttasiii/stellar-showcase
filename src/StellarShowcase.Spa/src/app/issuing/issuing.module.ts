import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IssuingRoutingModule } from './issuing-routing.module';
import { OverviewComponent } from './overview/overview.component';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    OverviewComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    SharedModule,
    IssuingRoutingModule
  ]
})
export class IssuingModule { }
