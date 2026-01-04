import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { BlogComponent } from './blog/blog.component';
import { CmsComponent } from './cms/cms.component';
import { DraftsComponent } from './drafts/drafts.component';
import { authGuard } from './guards/auth.guard.functional';
import { adminGuard } from './guards/admin.guard.functional';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'blog', component: BlogComponent },
  { 
    path: 'cms', 
    component: CmsComponent, 
    canActivate: [authGuard, adminGuard] 
  },
  { 
    path: 'drafts', 
    component: DraftsComponent, 
    canActivate: [authGuard, adminGuard] 
  },
  { path: '**', redirectTo: '' }
];
