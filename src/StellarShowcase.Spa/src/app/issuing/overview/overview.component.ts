import { Component, OnInit } from '@angular/core';

import { IssuerService } from '../../core/services/issuer.service';

@Component({
  selector: 'app-overview',
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit {

  constructor(private issuerService: IssuerService) { }

  ngOnInit(): void {
  }

}
