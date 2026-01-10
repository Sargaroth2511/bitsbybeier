import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  standalone: true
})
export class HomeComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit() {
    // If there's a returnUrl query param, redirect appropriately
    const returnUrl = this.route.snapshot.queryParams['returnUrl'];
    if (returnUrl) {
      if (this.authService.isAuthenticated()) {
        // User is authenticated, navigate directly to the return URL
        this.router.navigateByUrl(returnUrl);
      } else {
        // User is not authenticated, redirect to login
        this.router.navigate(['/login'], { queryParams: { returnUrl } });
      }
    }
  }
}
