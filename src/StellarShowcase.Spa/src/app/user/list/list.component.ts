import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { ComponentBase } from '../../core/component-base';
import { UserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent extends ComponentBase implements OnInit {

  columns = ['fullName', 'fullAddress', 'email', 'phone', 'accountId', 'action']
  userAccounts: UserAccountDto[];

  constructor(
    private userAccountService: UserAccountService,
    private router: Router) {
    super();
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.userAccountService
      .getAll()
      .subscribe({
        next: result => this.userAccounts = result,
        complete: () => this.stopLoading(),
      });
  }

  goToDetail(id: string) {
    this.router.navigate(['user', id, 'details']);
  }

  goToCreate() {
    this.router.navigate(['user', 'create'])
  }
}
