import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

import { IssuerDto } from '../../core/models/dto';
import { IssuerService } from '../../core/services/issuer.service';

interface IssuerDtoEx extends IssuerDto {
  nr: number;
}

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit {

  issuers: IssuerDtoEx[];
  columns = ['nr', 'issuerAccountId', 'distributorAccountId', 'action'];

  constructor(
    private issuerService: IssuerService,
    private snackBar: MatSnackBar,
    private router: Router) { }

  ngOnInit(): void {
    this.loadData();
  }

  createIssuer(): void {
    this.issuerService.create().subscribe(() => {
      this.snackBar.open('Issuer created!', 'OK', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        politeness: 'polite',
      });
      this.loadData();
    });
  }

  goToDetails(id: string){
    this.router.navigate(['issuing', id]);
  }

  private loadData(): void {
    this.issuerService
      .getAll()
      .subscribe(result => {
        this.issuers = [];
        let cnt = 0;
        result.forEach(i => {
          const issuer = i as IssuerDtoEx;
          issuer.nr = ++cnt;
          this.issuers.push(issuer);
        });
      });
  }

}
