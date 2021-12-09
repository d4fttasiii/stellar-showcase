import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

import { ComponentBase } from '../../core/component-base';
import { DialogCredentials } from '../../core/models/dialog-credentials';
import { AssetDto } from '../../core/models/dto';
import { AssetService } from '../../core/services/asset.service';
import { UserAccountService } from '../../core/services/user-account.service';
import { CredentialsDialogComponent } from '../../shared/credentials-dialog/credentials-dialog.component';

@Component({
  selector: 'app-add-asset',
  templateUrl: './add-asset.component.html',
  styleUrls: ['./add-asset.component.scss']
})
export class AddAssetComponent extends ComponentBase implements OnInit {

  id: string;
  selectedAsset: AssetDto;
  assets: AssetDto[];

  constructor(
    private assetService: AssetService,
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private router: Router) {
    super();
  }

  ngOnInit(): void {
    this.activatedRoute.params
      .subscribe(params => {
        this.id = params['id'];
        this.loadData();
      });
  }

  openDialog() {
    let passphrase = this.userAccountService.getPassphrase(this.id);
    if (!passphrase) {
      const dialogRef = this.dialog.open(CredentialsDialogComponent, {
        data: this.id,
      });

      dialogRef.afterClosed().subscribe((result: DialogCredentials) => {
        if (result && result.passphrase) {
          this.submit(result.passphrase);
        }
      });
    } else {
      this.submit(passphrase);
    }
  }

  private submit(passphrase: string) {
    this.isSubmitting = true;
    this.userAccountService
      .createTrustline(this.id, this.selectedAsset.id, this.selectedAsset.issuerId, {
        passphrase: passphrase,
      })
      .subscribe({
        next: () => {
          this.snackBar.open(`Asset: ${this.selectedAsset.unitName} added!`, 'OK', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            politeness: 'polite',
          });
          this.router.navigate(['user']);
        },
        complete: () => this.stopSubmitting(),
      });
  }

  private loadData() {
    this.isLoading = true;
    this.assetService
      .getAll()
      .subscribe({
        next: (result) => {
          this.assets = result;
        },
        complete: () => this.stopLoading(),
      });
  }
}
