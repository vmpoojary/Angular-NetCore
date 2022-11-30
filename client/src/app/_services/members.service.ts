import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/Member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';



@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl=environment.apiUrl;
  members:Member[]=[];   
  memberCache=new Map();
  user:User|undefined;
  userParams:UserParams|undefined
  ;
  

  constructor(private http:HttpClient,private accountService:AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next:user=>{
        if(user){
          this.userParams=new UserParams(user);
          this.user=user;
        }
      }
    })
   }

   getUserParams()
   {
    return this.userParams;
   }

   setUserParams(params:UserParams)
   {
    this.userParams= params; 
   }

   resetUserParams()
   {
      if(this.user)
      {
        this.userParams =new UserParams(this.user);
        return this.userParams;
      }
      return;
   }

  getMembers(userParams:UserParams)
  {
    const response=this.memberCache.get(Object.values(userParams).join('-'));

    if(response) return of(response);  //chapter 13 caching

    let params = this.getPagnationHeaders(userParams.pageNumber,userParams.pageSize);

    params=params.append('minAge',userParams.minAge);
    params=params.append('maxAge',userParams.maxAge);
    params=params.append('gender',userParams.gender);
    params=params.append('orderBy',userParams.orderBy)

    //if(this,this.members.length>0) return of(this.members);       //caching check
    return this.getPaginatedResult<Member[]>(this.baseUrl+'users' ,params).pipe(
      map(response=>{
        this.memberCache.set(Object.values(userParams).join('-'),response);
        return response
      })
    );
  }

  private getPaginatedResult<T>(url:string,params: HttpParams) {
    const paginatedResult:PaginatedResult<T>=new PaginatedResult<T>;
    debugger;
    return this.http.get<T>(url, { observe: 'response', params }).pipe(

      map(response => {
        if (response.body) {
          paginatedResult.result = response.body;
        }
        const pagination = response.headers.get('pagination');
        if (pagination) {
          paginatedResult.pagination = JSON.parse(pagination);
        }
        return  paginatedResult;
      })

    );
  }

  private getPagnationHeaders(pageNumber:number,pageSize:number) {
    let params = new HttpParams();
    
      params = params.append('pageNumber', pageNumber);
      params = params.append('pageSize', pageSize);
    
    return params;
  }

  getMember(userName:string)
  {
    const member=[...this.memberCache.values()]
      .reduce((arr,elem)=>arr.concat(elem.result),[])
      .find((member:Member)=>member.userName===userName);
      ;
      
    // const member=this.members.find(x=>x.userName===userName)
      if(member) return of(member)
    return this.http.get<Member>(this.baseUrl+'users/'+userName)
  }

  updateMember(member:Member)
  {
    return this.http.put(this.baseUrl + 'users/', member)
    .pipe(
      map(()=>{
        const index=this.members.indexOf(member);
        this.members[index]=member; 
      }))
    ;
  }

  setMainPhoto(photoId:number){
    return this.http.put(this.baseUrl+'users/set-main-photo/'+photoId,{});
  }
  deletePhoto(photoId:number){
    return this.http.delete(this.baseUrl + "users/delete-photo/"+photoId);
  }

  addLike(username:string)
  {
    return this.http.post(this.baseUrl+'likes/'+username,{})
  }

  getLikes(predicate:string,pageNumber:number,pageSize:number)
  {
    debugger
    let params=this.getPagnationHeaders(pageNumber,pageSize);

    params=params.append('predicate',predicate)

    return this.getPaginatedResult<Member[]>(this.baseUrl+'likes',params);
  }
}
