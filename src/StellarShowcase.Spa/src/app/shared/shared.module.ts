import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { ContainerComponent } from './container/container.component';
import { MenuItemComponent } from './menu-item/menu-item.component';
import { MenuComponent } from './menu/menu.component';

// import { MatTabsModule } from '@angular/material/tabs';
// import { MatSortModule } from '@angular/material/sort';
// import { MatChipsModule } from '@angular/material/chips';
// import { MatNativeDateModule } from '@angular/material/core';
// import { MatDatepickerModule } from '@angular/material/datepicker';
// import { MatDialogModule } from '@angular/material/dialog';
// import { MatDividerModule } from '@angular/material/divider';
// import { MatExpansionModule } from '@angular/material/expansion';
// import { MatGridListModule } from '@angular/material/grid-list';
// import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
// import { MatRadioModule } from '@angular/material/radio';
// import { MatPaginatorModule } from '@angular/material/paginator';
// import { MatListModule } from '@angular/material/list';

// import {MatButtonToggleModule} from '@angular/material/button-toggle';
const MAT_MODULES = [
  MatIconModule,
  MatToolbarModule,
  MatSidenavModule,
  MatButtonModule,
  MatIconModule,
  // MatDividerModule,
  // MatProgressSpinnerModule,
  MatCardModule,
  // MatListModule,
  MatTooltipModule,
  MatTableModule,
  MatInputModule,
  // MatDatepickerModule,
  // MatNativeDateModule,
  // MatChipsModule,
  // MatExpansionModule,
  // MatTabsModule,
  MatSelectModule,
  MatSnackBarModule,
  // MatDialogModule,
  // MatGridListModule,
  // MatPaginatorModule,
  MatCheckboxModule,
  // MatSortModule,
  MatProgressBarModule,
  MatBadgeModule,
  MatMenuModule,
  // MatRadioModule,
];

const EXPORTS = [
  MenuComponent,
  MenuItemComponent,
  ContainerComponent,
];


@NgModule({
  declarations: [...EXPORTS, ContainerComponent],
  imports: [CommonModule, FormsModule, ...MAT_MODULES],
  exports: [FormsModule, ...EXPORTS, ...MAT_MODULES],
})
export class SharedModule { }
