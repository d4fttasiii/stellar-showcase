import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { UserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {

  columns = ['fullName', 'fullAddress', 'email', 'phone', 'action']
  userAccounts: UserAccountDto[];

  constructor(
    private userAccountService: UserAccountService,
    private router: Router) { }

  ngOnInit(): void {
    this.userAccountService
      .getAll()
      .subscribe(
        result => this.userAccounts = result,
      );
  }

  goToDetail(id: string) {
    this.router.navigate(['user', id, 'details']);
  }

  goToCreate() {
    this.router.navigate(['user', 'create'])
  }
}
