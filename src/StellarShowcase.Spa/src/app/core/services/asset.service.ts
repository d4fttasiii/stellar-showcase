import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { AssetDto } from '../models/dto';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AssetService {
  private controllerName = 'asset';

  constructor(private api: ApiService) { }

  getAll(): Observable<AssetDto[]> {
    return this.api.get<AssetDto[]>(this.controllerName);
  }
}
