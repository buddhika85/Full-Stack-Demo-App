import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { LandingDto } from '../../models/landing.dtos';
import { HomeService } from '../../services/home.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-landing-page',
  imports: [],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
})
export class LandingPage implements OnInit, OnDestroy {
  private readonly authService: AuthService = inject(AuthService);
  private readonly homeService: HomeService = inject(HomeService);
  private readonly compositeSubscription: Subscription = new Subscription(); // Composite Subscription Pattern. - can hold multiple subscriptions
  landingDto: LandingDto | null = null;
  allowedOrgins: string[] | null = null;

  ngOnInit(): void {
    this.fetchApiHealth();

    if (this.authService.isAdmin() || this.authService.isStaff()) {
      this.fetchAllowedOrings();
      this.fetchDiagnostics();
    }

    this.fetchLandingContent();
  }

  fetchApiHealth(): void {
    const sub = this.homeService.health().subscribe({
      next: (value: string[]) => {
        console.log('Health: ', value);
      },
      error: (error: any) => {
        console.error('Error retrieving API Health:', error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  fetchDiagnostics(): void {
    const sub = this.homeService.diagnostics().subscribe({
      next: (value: string[]) => {
        console.log('Diagnostics: ', value);
      },
      error: (error: any) => {
        console.error('Error retrieving API Diagnostics:', error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  fetchLandingContent(): void {
    const sub = this.homeService.landingContent().subscribe({
      next: (value: LandingDto) => {
        this.landingDto = value;
      },
      error: (error: any) => {
        console.error('Error retrieving landing content:', error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  fetchAllowedOrings(): void {
    const sub = this.homeService.allowedOrigins().subscribe({
      next: (value: string[]) => {
        this.allowedOrgins = value;
        console.log('Allowed Origins: ', this.allowedOrgins);
      },
      error: (error: any) => {
        console.error('Error retrieving allowed origins:', error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
