import { outputAst } from '@angular/compiler';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Route, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {


  @Output() cancelRegister=new EventEmitter();
  constructor(private accountService:AccountService,
              private toastr:ToastrService,
              private fb:FormBuilder,
              private router:Router
          ) { }
  
  registerForm:FormGroup =new FormGroup({});
  maxDate: Date=new Date(); 
  validationErrors: string[]|undefined

  ngOnInit(): void {
    this.initalizeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }

  initalizeForm()
  {
    this.registerForm=this.fb.group({
      gender:['male'],
      username:['',Validators.required],
      knownAs:['',Validators.required],
      dateOfBirth:['',Validators.required],
      city:['',Validators.required],
      country:['',Validators.required],
      password:['',[Validators.required,Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword:['',[Validators.required,this.matchValues('password')] ],
    });
    this,this.registerForm.controls['password'].valueChanges.subscribe({
      next:()=>this.registerForm.controls['confirmPassword'].updateValueAndValidity
    })  /// this part of code checks the validity of ConfirmPassword if the value of Password field is changed
  }

  register()
  {
    const dob=this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    const values={...this.registerForm.value,dateOfBirth:dob}
    // console.log(this.registerForm?.value);
    this.accountService.register(values).subscribe({
      next:()=>{
      this.router.navigateByUrl('/members')
      }
    ,
    error:error=>{
      this.validationErrors=error
    }
    }
    );
   
  }

  matchValues(matchTo:string):ValidatorFn{
    return(control: AbstractControl)=>{
      return control.value===control.parent?.get(matchTo)?.value  ? null:{notMatching:true}
    }
  }

  cancel()
  {
    
    this.cancelRegister.emit(false);
  }

  private getDateOnly(dob: string|undefined)
  {
    if(!dob) return;
    let theDob=new Date(dob);
    return new Date(theDob.setMinutes(theDob.getMinutes()-theDob.getTimezoneOffset()))
    .toISOString().slice(0,10);
  }
}
