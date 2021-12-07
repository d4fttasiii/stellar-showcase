import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DexRoutingModule } from './dex-routing.module';
import { ListComponent } from './list/list.component';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';
import { CreateComponent } from './create/create.component';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    SharedModule,
    DexRoutingModule
  ]
})
export class DexModule { }
