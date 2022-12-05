import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/Member';
import { Message } from 'src/app/_models/message';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit,OnDestroy {

  @ViewChild('memberTabs',{static:true}) memberTabs?:TabsetComponent ;
  member: Member={} as Member;
  activeTab?: TabDirective;
  messages:Message[]=[];
  user?: User;

  constructor(private accountService:AccountService, private route:ActivatedRoute,
              private messageService:MessageService, public presenceService:PresenceService
            ) { 
                this.accountService.currentUser$.pipe(take(1)).subscribe({
                  next: user => {
                    if(user) this.user=user;
                  }
                })
            }
  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  ngOnInit(): void {
    debugger
    this.route.data.subscribe({
      next: data=>this.member=data['member']
    })

    this.route.queryParams.subscribe({
      next: params=>{
        if(params['tab'])
        {
          this.selectTab(params['tab'])
        }
      }
    })
  }
  

  // loadMember()
  // {
  //   this.messageService.getMember(this.member.userName).subscribe(
  //     member=>{
  //       this.member=member;
       
  //     }
  //   )
  // }

  onTabActivated(data : TabDirective)
  {
    this.activeTab=data;
    if(this.activeTab.heading==='Messages'  && this.user)
      this.messageService.createHubConnection(this.user,this.member.userName);
    else
      this.messageService.stopHubConnection();
    
  }

  loadMessages()
  {
    debugger
    if(this.member.userName)
    {
      this.messageService.getMessageThread(this.member.userName).subscribe(
        {
          next: messages=>{
            debugger
            this.messages=messages}
        }
      )
    }
  }

  selectTab(heading:string)
  {
    debugger;
    if(this.memberTabs){
      this.memberTabs.tabs.find(x=>x.heading===heading)!.active=true
    }
  }

}
