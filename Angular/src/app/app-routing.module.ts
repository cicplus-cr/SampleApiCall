import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApiTesterComponent } from './components/api-tester/api-tester.component';
import { SigninComponent } from './components/signin/signin.component';

const routes: Routes = [
  {
    path: '',
    component: SigninComponent
  },
  {
    path: 'api-tester',
    component: ApiTesterComponent
  }

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
