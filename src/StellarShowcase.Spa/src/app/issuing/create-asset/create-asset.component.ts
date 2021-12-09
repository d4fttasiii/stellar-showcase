import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

import { ComponentBase } from '../../core/component-base';
import { UpsertAssetDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';

@Component({
  selector: 'app-create-asset',
  templateUrl: './create-asset.component.html',
  styleUrls: ['./create-asset.component.scss']
})
export class CreateAssetComponent extends ComponentBase implements OnInit {

  issuerId: string;
  model: UpsertAssetDto;

  constructor(
    private issuerService: IssuerService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { super(); }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(params => {
      this.issuerId = params['id'];
      this.model = {
        unitName: '',
        totalSupply: 0,
        isAuthRequired: false,
        isAuthRevocable: false,
        isAuthImmutable: false,
        isClawbackEnabled: false,
      };
    });
    this.stopLoading();
  }

  submit() {
    this.isSubmitting = true;
    this.issuerService
      .createAsset(this.issuerId, this.model)
      .subscribe({
        next: () => {
          this.snackBar.open(`Asset: ${this.model.unitName} created!`, 'OK', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            politeness: 'polite',
          });
          this.router.navigate(['issuing']);
        },
        complete: () =>
          this.stopSubmitting(),
      });
  }
}
