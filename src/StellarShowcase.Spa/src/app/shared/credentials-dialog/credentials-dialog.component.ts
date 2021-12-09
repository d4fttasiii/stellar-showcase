import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogCredentials } from '../../core/models/dialog-credentials';
import { UserAccountService } from '../../core/services/user-account.service';

@Component({
  selector: 'app-credentials-dialog',
  templateUrl: './credentials-dialog.component.html',
  styleUrls: ['./credentials-dialog.component.scss']
})
export class CredentialsDialogComponent implements OnInit {

  credentials: DialogCredentials;

  constructor(public dialogRef: MatDialogRef<CredentialsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private userAccountService: UserAccountService) { }

  ngOnInit(): void {
    this.credentials = {
      passphrase: '',
      canStore: false,
    };
  }

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if(this.credentials.canStore){
      this.userAccountService.storePassphrase(this.data, this.credentials.passphrase);
    }
    this.dialogRef.close(this.credentials);
  }

}
