import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { CreateProjectRequest } from '../models/create-project.model';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Project } from '../models/project.model';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  constructor(private http: HttpClient) {}

  CreateProject(model: CreateProjectRequest): Observable<any> {
    return this.http.post<any>(`${environment.apiBaseUrl}/project`, model);
  }

  GetAllProject(): Observable<Project[]> {
    return this.http.get<Project[]>(
      `${environment.apiBaseUrl}/project/get-all`
    );
  }

  SearchProject(searchValue: string, status: string): Observable<Project[]> {
    return this.http.get<Project[]>(
      `${environment.apiBaseUrl}/project/search?searchValue=${searchValue}&status=${status}`
    );
  }

  GetByProjectNumber(projectNumber: number): Observable<CreateProjectRequest> {
    return this.http.get<CreateProjectRequest>(
      `${environment.apiBaseUrl}/project/get-by-project-num/${projectNumber}`
    );
  }

  UpdateProject(
    projectNumber: number,
    model: CreateProjectRequest
  ): Observable<any> {
    return this.http.put<any>(
      `${environment.apiBaseUrl}/project/${projectNumber}`,
      model
    );
  }
}
