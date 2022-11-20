import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/Member';



@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl=environment.apiUrl;
  constructor(private http:HttpClient) { }

  getMembers()
  {
    return this.http.get<Member[]>(this.baseUrl+'users')
  }

  getMember(userName:string)
  {
    return this.http.get<Member>(this.baseUrl+'users/'+userName)
  }

}