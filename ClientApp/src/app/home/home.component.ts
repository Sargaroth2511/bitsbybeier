import { Component } from '@angular/core';
import { AuthService, UserInfo } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css'],
    standalone: false
})
export class HomeComponent {
  user: UserInfo | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.authService.user$.subscribe(user => {
      this.user = user;
    });
  }

  login(): void {
    this.authService.login();
  }

  goToCms(): void {
    this.router.navigate(['/cms']);
  }
}

