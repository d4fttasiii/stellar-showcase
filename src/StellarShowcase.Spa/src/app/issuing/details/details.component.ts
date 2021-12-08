import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { IssuerDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss']
})
export class DetailsComponent implements OnInit {

  issuer: IssuerDto;
  isLoading = true;

  constructor(
    private issuerService: IssuerService,
    private activatedRoute: ActivatedRoute,
    private router: Router) { }

  ngOnInit(): void {
    this.activatedRoute.params
      .subscribe(params =>
        this.loadData(params['id']));
  }

  goToCreateToken() {
    this.router.navigate(['issuing', this.issuer.id, 'create-asset'])
  }

  goToTransfer() {
    this.router.navigate(['issuing', this.issuer.id, 'transfer-asset']);
  }

  private loadData(id: string): void {
    this.isLoading = true;
    this.issuerService
      .get(id)
      .subscribe({
        next: result => {
          this.issuer = result;
        },
        complete: () => setTimeout(() => this.isLoading = false, 600),
      });
  }

}
