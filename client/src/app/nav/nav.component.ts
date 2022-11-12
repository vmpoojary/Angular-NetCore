import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  constructor(public accountService:AccountService) { }
  model:any={}
  

  ngOnInit(): void {
    
  }

  login()
  {
    
    this.accountService.login(this.model).subscribe(
      response=>{
        console.log(this.model);
      },error=>{
        console.log(error)
      }
    )
  }

  logout()
  {
    this.accountService.logout();
  }



}
