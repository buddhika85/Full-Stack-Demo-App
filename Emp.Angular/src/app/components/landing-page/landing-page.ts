import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { LandingDto } from '../../models/landing.dtos';
import { HomeService } from '../../services/home.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-landing-page',
  imports: [],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
})
export class LandingPage implements OnInit, OnDestroy {
  private readonly homeService: HomeService = inject(HomeService);
  private readonly compositeSubscription: Subscription = new Subscription(); // Composite Subscription Pattern. - can hold multiple subscriptions
  landingDto: LandingDto | null = null;
  allowedOrgins: string[] | null = null;

  ngOnInit(): void {
    this.fetchLandingContent();
    this.fetchAllowedOrings();
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
