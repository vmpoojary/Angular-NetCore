import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPagnationHeaders } from './pagInationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl=environment.apiUrl;


  constructor(private http: HttpClient) { }

    getMessages(pageNumber:number,pageSize:number,container:string){
      let params=getPagnationHeaders(pageNumber,pageSize);
      params=params.append('Container',container);

      return getPaginatedResult<Message[]>(this.baseUrl+'messages',params,this.http);
    }

    getMessageThread(username:string)
    {
      return this.http.get<Message[]>(this.baseUrl+'messages/thread/'+username);
    }

    sendMessage(username:string,content:string)
    {
      return this.http.post<Message>(this.baseUrl+'messages',{recipientUsername:username,Content:content})
    }

    deleteMessage(id:number)
    {
      return this.http.delete(this.baseUrl+'messages/'+id)
    }
}
