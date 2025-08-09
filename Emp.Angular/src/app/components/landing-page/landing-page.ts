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

  ngOnInit(): void {
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

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
