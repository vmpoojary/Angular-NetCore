import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]' //*appHasRole='["Admin","Thing"]'
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[]=[];
  user:User={} as User;

  constructor(private viewContainerRef:ViewContainerRef,private tempateRef:TemplateRef<any>,
            private accountService:AccountService ) {
              this.accountService.currentUser$.pipe(take(1)).subscribe({
                next:user=>{
                  if(user) this.user=user;
                }
              })
             }
  ngOnInit(): void {

    if(this.user.role.some(r=>this.appHasRole.includes(r))){
      this.viewContainerRef.createEmbeddedView(this.tempateRef);
    }
    else{
      this.viewContainerRef.clear()
    }
  }

}
