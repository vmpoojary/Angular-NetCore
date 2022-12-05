import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  constructor(public accountService:AccountService,private router:Router,private toastr:ToastrService) { }
  model:any={}
  

  ngOnInit(): void {
    
  }

  login()
  {
    
    this.accountService.login(this.model).subscribe(
      response=>{
        this.router.navigateByUrl('/members');
        this.model={};
      },error=>{
        this.toastr.error(error.error)
        console.log(error)
      }
    )
  }

  logout()
  {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }



}
