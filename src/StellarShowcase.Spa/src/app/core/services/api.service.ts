import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, map, Observable, OperatorFunction } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private baseUrl = "http://localhost:5000/api"

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar,
  ) { }

  get<TResponse>(
    path: string,
    showLoading: boolean = true,
  ): Observable<TResponse> {
    if (showLoading) {
    }

    return this.http
      .get(`${this.baseUrl}/${path}`)
      .pipe(
        this.successHandler<TResponse>(showLoading),
        this.errorHandler<TResponse>(showLoading)
      );
  }

  delete<TResponse>(
    path: string,
    showLoading: boolean = true,
  ): Observable<TResponse> {
    if (showLoading) {
    }

    return this.http
      .delete(`${this.baseUrl}/${path}`)
      .pipe(
        this.successHandler<TResponse>(showLoading),
        this.errorHandler<TResponse>(showLoading)
      );
  }

  post<TPost, TResponse>(
    path: string,
    toPost: TPost,
    showLoading: boolean = true,
  ): Observable<TResponse> {
    if (showLoading) {
    }

    return this.http
      .post(`${this.baseUrl}/${path}`, toPost)
      .pipe(
        this.successHandler<TResponse>(showLoading),
        this.errorHandler<TResponse>(showLoading)
      );
  }

  put<TPost, TResponse>(
    path: string,
    toPost: TPost,
    showLoading: boolean = true,
  ): Observable<TResponse> {
    if (showLoading) {
    }

    return this.http
      .put(`${this.baseUrl}/${path}`, toPost)
      .pipe(
        this.successHandler<TResponse>(showLoading),
        this.errorHandler<TResponse>(showLoading)
      );
  }

  patch<TPost, TResponse>(
    path: string,
    toPost: TPost,
    showLoading: boolean = true,
  ): Observable<TResponse> {
    if (showLoading) {
    }

    return this.http
      .patch(`${this.baseUrl}/${path}`, toPost)
      .pipe(
        this.successHandler<TResponse>(showLoading),
        this.errorHandler<TResponse>(showLoading)
      );
  }

  private successHandler<TResponse>(showLoading: boolean): OperatorFunction<TResponse, any> {
    return map<TResponse, any>((resp) => {
      if (showLoading) {
        // this.loadingService.stopLoading();
      }
      return resp;
    });
  }

  private errorHandler<TResponse>(showLoading: boolean): OperatorFunction<TResponse, any> {
    return catchError<TResponse, any>((errorResponse) => {
      let errorMessage = '';
      let errors = null;
      if (errorResponse && errorResponse.error && errorResponse.error.errorMessageCodes) {
        errorResponse.error.errorMessageCodes.map((m) => (errorMessage += m + '\r\n'));
      } else if (
        errorResponse &&
        errorResponse.statusText &&
        errorResponse.error &&
        errorResponse.error.status
      ) {
        errorMessage = errorResponse.statusText;
        if (errorResponse.error.title) {
          errorMessage += ' - ' + errorResponse.error.title;
        }
        if (errorResponse.error.errors) {
          errors = errorResponse.error.errors;
        }
      } else {
        errorMessage = 'Server error.';
        if (errorResponse.status && errorResponse.statusText) {
          errorMessage = errorResponse.statusText + ' (' + errorResponse.status + ')';
        }
        if (errorResponse.error) {
          errors = errorResponse.error;
        }
      }

      if (errors) {
        console.error(errorMessage, 'Error details: ', errors);
      } else {
        console.error(errorMessage);
      }

      if (showLoading) {
      }

      this.snackBar.open(errorMessage, 'OK', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        politeness: 'polite',
      });

      return null;
    });
  }
}
