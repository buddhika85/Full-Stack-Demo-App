import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  readonly loading$: Observable<boolean> = this.loadingSubject.asObservable();

  show(): void {
    setTimeout(() => this.loadingSubject.next(true), 0);
  }

  hide(): void {
    setTimeout(() => this.loadingSubject.next(false), 0);
  }
}
