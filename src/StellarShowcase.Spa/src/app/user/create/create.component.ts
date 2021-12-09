import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

import { ComponentBase } from '../../core/component-base';
import { CreateUserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent extends ComponentBase implements OnInit {

  model: CreateUserAccountDto;

  constructor(
    private userAccountService: UserAccountService,
    private snackBar: MatSnackBar,
    private router: Router) {
    super();
  }

  ngOnInit(): void {
    this.model = {
      fullName: '',
      fullAddress: '',
      email: '',
      phone: '',
      passphrase: ''
    };
    this.stopLoading();
  }

  submit() {
    this.isSubmitting = true;
    this.userAccountService
      .create(this.model)
      .subscribe({
        next: () => {
          this.snackBar.open('User account created!', 'OK', {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            politeness: 'polite',
          });
          this.router.navigate(['user']);
        },
        complete: () => this.stopSubmitting(),
      });
  }

}
