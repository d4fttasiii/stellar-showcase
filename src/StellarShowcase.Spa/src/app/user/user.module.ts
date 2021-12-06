import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserRoutingModule } from './user-routing.module';
import { ListComponent } from './list/list.component';
import { CreateComponent } from './create/create.component';
import { CoreModule } from '../core/core.module';
import { SharedModule } from '../shared/shared.module';
import { DetailsComponent } from './details/details.component';


@NgModule({
  declarations: [
    ListComponent,
    CreateComponent,
    DetailsComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    SharedModule,
    UserRoutingModule,
  ]
})
export class UserModule { }
