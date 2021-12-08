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
  isLoading = true;

  constructor(
    private userAccountService: UserAccountService,
    private router: Router) { }

  ngOnInit(): void {
    this.isLoading = true;
    this.userAccountService
      .getAll()
      .subscribe({
        next: result => this.userAccounts = result,
        complete: () => setTimeout(() => this.isLoading = false, 600),
      });
  }

  goToDetail(id: string) {
    this.router.navigate(['user', id, 'details']);
  }

  goToCreate() {
    this.router.navigate(['user', 'create'])
  }
}
