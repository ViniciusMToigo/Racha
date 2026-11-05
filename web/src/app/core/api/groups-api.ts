import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { APP_CONFIG } from '../config/app-config';
import { Group, GroupSummary } from '../models/api.models';

/** Thin, typed wrapper over the /groups endpoints. No state here: components own their state. */
@Injectable({ providedIn: 'root' })
export class GroupsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${inject(APP_CONFIG).apiUrl}/groups`;

  listMine(): Observable<GroupSummary[]> {
    return this.http.get<GroupSummary[]>(this.baseUrl);
  }

  get(groupId: string): Observable<Group> {
    return this.http.get<Group>(`${this.baseUrl}/${groupId}`);
  }

  create(name: string): Observable<Group> {
    return this.http.post<Group>(this.baseUrl, { name });
  }

  rename(groupId: string, name: string): Observable<Group> {
    return this.http.put<Group>(`${this.baseUrl}/${groupId}`, { name });
  }

  delete(groupId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${groupId}`);
  }

  addMember(groupId: string, email: string): Observable<Group> {
    return this.http.post<Group>(`${this.baseUrl}/${groupId}/members`, { email });
  }
}
