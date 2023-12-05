import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LoginRequest } from '../models/login-request.model';
import { Observable } from 'rxjs';
import { LoginResponse } from '../models/login-response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  constructor(private http: HttpClient) {}

  Login(model: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      'https://localhost:7099/auth/login',
      model
    );
  }
}