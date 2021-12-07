import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

import { CreateUserAccountDto } from '../../core/models/dto';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-create',
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss']
})
export class CreateComponent implements OnInit {

  model: CreateUserAccountDto;

  constructor(
    private userAccountService: UserAccountService, 
    private snackBar: MatSnackBar) { }

  ngOnInit(): void {
    this.model = {
      fullName: '',
      fullAddress: '',
      email: '',
      phone: ''
    };
  }

  submit() {
    this.userAccountService
      .create(this.model)
      .subscribe(() => {
        this.snackBar.open('User account created!', 'OK', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'bottom',
          politeness: 'polite',
        });
      });
  }

}
