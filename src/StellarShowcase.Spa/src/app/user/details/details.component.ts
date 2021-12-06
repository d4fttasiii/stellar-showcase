import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { UserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss']
})
export class DetailsComponent implements OnInit {

  userAccount: UserAccountDto;

  constructor(
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute) { }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(
      params => this.userAccountService
        .get(params['id'])
        .subscribe(result => this.userAccount = result),
    );
  }

}
