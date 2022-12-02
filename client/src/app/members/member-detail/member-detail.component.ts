import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/Member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  @ViewChild('memberTabs',{static:true}) memberTabs?:TabsetComponent ;
  member: Member={} as Member;
  activeTab?: TabDirective;
  messages:Message[]=[];

  constructor(private memberService:MembersService, private route:ActivatedRoute,
              private messageService:MessageService
            ) { }

  ngOnInit(): void {
    
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
  

  loadMember()
  {
    this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe(
      member=>{
        this.member=member;
       
      }
    )
  }

  onTabActivated(data : TabDirective)
  {
    this.activeTab=data;
    if(this.activeTab.heading==='Messages'){
      this.loadMessages();
    }
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
