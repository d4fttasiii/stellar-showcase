import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { IssuerDto, IssuerTransferDto, UpsertAssetDto } from '../models/dto';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class IssuerService {
  private controllerName = 'issuer';

  constructor(private api: ApiService) { }

  getAll(): Observable<IssuerDto[]> {
    return this.api.get<IssuerDto[]>(this.controllerName);
  }

  get(id: string): Observable<IssuerDto> {
    return this.api.get<IssuerDto>(`${this.controllerName}/${id}`);
  }

  create(): Observable<string> {
    return this.api.post(this.controllerName, {});
  }

  createAsset(id: string, data: UpsertAssetDto): Observable<string> {
    return this.api.post(`${this.controllerName}/${id}/asset`, data);
  }

  transferAsset(id: string, assetId: string, tx: IssuerTransferDto): Observable<string> {
    return this.api.post(`${this.controllerName}/${id}/asset/${assetId}/transfer`, tx);
  }

}
