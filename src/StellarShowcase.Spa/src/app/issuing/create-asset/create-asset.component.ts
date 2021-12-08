import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

import { UpsertAssetDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';

@Component({
  selector: 'app-create-asset',
  templateUrl: './create-asset.component.html',
  styleUrls: ['./create-asset.component.scss']
})
export class CreateAssetComponent implements OnInit {

  issuerId: string;
  model: UpsertAssetDto;
  isLoading = true;

  constructor(
    private issuerService: IssuerService,
    private activatedRoute: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router) { }

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
    setTimeout(() => this.isLoading = false, 600);
  }

  submit() {
    this.issuerService
      .createAsset(this.issuerId, this.model)
      .subscribe(() => {
        this.snackBar.open(`Asset: ${this.model.unitName} created!`, 'OK', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          politeness: 'polite',
        });
        this.router.navigate(['issuing']);
      });
  }
}
