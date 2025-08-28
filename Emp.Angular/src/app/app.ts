import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  inject,
} from '@angular/core';
import { Navbar } from './shared/components/navbar/navbar';
import { RouterOutlet } from '@angular/router';
import { Footer } from './shared/components/footer/footer';
import { LoadingService } from './services/loading.service';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
@Component({
  selector: 'app-root',
  imports: [
    Navbar,
    RouterOutlet,
    Footer,
    CommonModule,
    MatProgressSpinnerModule,
  ], // RouterOutlet
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly loadingService: LoadingService = inject(LoadingService);

  readonly isLoading$: Observable<boolean> = this.loadingService.loading$;
}
