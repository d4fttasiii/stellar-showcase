import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import ls from 'localstorage-slim';

import {
  ActiveOrderDto,
  CreateBuyOrderDto,
  CreateSellOrderDto,
  CreateUserAccountDto,
  CredentialsDto,
  UserAccountDto,
} from '../models/dto';
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

  createTrustline(id: string, assetId: string, issuerId: string, credentials: CredentialsDto): Observable<string> {
    return this.api.post(`${this.controllerName}/${id}/asset/${assetId}/create-trustline/${issuerId}`, credentials);
  }

  createBuyOrder(id: string, order: CreateBuyOrderDto): Observable<string> {
    return this.api.post(`${this.controllerName}/${id}/orders/buy`, order);
  }

  createSellOrder(id: string, order: CreateSellOrderDto): Observable<string> {
    return this.api.post(`${this.controllerName}/${id}/orders/sell`, order);
  }

  getActiverOrders(id: string): Observable<ActiveOrderDto[]> {
    return this.api.get<ActiveOrderDto[]>(`${this.controllerName}/${id}/orders`);
  }

  cancelOrder(id: string, orderId: number, credentials: CredentialsDto): Observable<boolean> {
    return this.api.delete(`${this.controllerName}/${id}/orders/${orderId}/cancel`, credentials);
  }

  storePassphrase(id: string, passphrase: string) {
    ls.set(id, passphrase, {
      encrypt: true,
      ttl: 300,
    });
  }

  getPassphrase(id: string): string {
    return ls.get(id, {
      decrypt: true
    });
  }

}
