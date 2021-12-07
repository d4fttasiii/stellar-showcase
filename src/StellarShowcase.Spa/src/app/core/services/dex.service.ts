import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { CreateMarketDto, MarketDto } from '../models/dto';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class DexService {
  private controllerName = 'exchange';

  constructor(private api: ApiService) { }

  getAll(): Observable<MarketDto[]> {
    return this.api.get<MarketDto[]>(`${this.controllerName}/markets`);
  }

  get(id: string): Observable<MarketDto> {
    return this.api.get<MarketDto>(`${this.controllerName}/markets/${id}`);
  }

  createMarket(data: CreateMarketDto): Observable<string> {
    return this.api.post(`${this.controllerName}/markets`, data);
  }
}
