import { Component, inject, OnInit } from '@angular/core';
import { LandingDto } from '../../models/landing.dtos';
import { HomeService } from '../../services/home.service';

@Component({
  selector: 'app-landing-page',
  imports: [],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
})
export class LandingPage implements OnInit {
  private homeService: HomeService = inject(HomeService);

  landingDto: LandingDto | null = null;

  ngOnInit(): void {
    debugger;
    this.homeService.landingContent().subscribe({
      next: (value: LandingDto) => {
        this.landingDto = value;
        console.log(this.landingDto);
      },
      error: (error: any) => {
        console.log('Error occurred while retriving landing content: ', error);
      },
      complete: () => {},
    });
  }
}
