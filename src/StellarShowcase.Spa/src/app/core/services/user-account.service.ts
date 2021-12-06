import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateUserAccountDto, UserAccountDto } from '../models/dto';

import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class UserAccountService {

  private controllerName = 'user-account';

  constructor(private api: ApiService) { }

  getAll(): Observable<UserAccountDto[]> {
    return this.api.get<UserAccountDto[]>(this.controllerName);
  }

  get(id: string): Observable<UserAccountDto> {
    return this.api.get<UserAccountDto>(`${this.controllerName}/${id}`);
  }

  create(data: CreateUserAccountDto): Observable<string> {
    return this.api.post(this.controllerName, data);
  }

}