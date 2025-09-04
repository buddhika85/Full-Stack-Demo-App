import { Component, inject, OnDestroy, signal } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  imports: [
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    RouterLink,
    CommonModule,
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar implements OnDestroy {
  readonly title = signal<string>('Demo Project');

  readonly authService: AuthService = inject(AuthService);
  private readonly router: Router = inject(Router);
  private readonly compositeSubscription: Subscription = new Subscription();

  logOut() {
    const sub = this.authService.logout().subscribe({
      next: (response) => {
        if (response.loggedOut) this.router.navigate(['login']);
      },
      error: (error) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
