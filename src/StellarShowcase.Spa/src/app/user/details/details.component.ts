import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { ComponentBase } from '../../core/component-base';
import { UserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss']
})
export class DetailsComponent extends ComponentBase implements OnInit {

  userAccount: UserAccountDto;

  constructor(
    private userAccountService: UserAccountService,
    private activatedRoute: ActivatedRoute,
    private router: Router) {
      super();
  }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe(
      params => this.loadData(params['id']),
    );
  }

  goToAddAsset() {
    this.router.navigate(['user', this.userAccount.id, 'add-asset']);
  }

  private loadData(id: string) {
    this.isLoading = true;
    this.userAccountService
      .get(id)
      .subscribe({
        next: result => this.userAccount = result,
        complete: () => this.stopLoading(),
      });
  }
}
