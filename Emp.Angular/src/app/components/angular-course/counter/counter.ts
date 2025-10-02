import { Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-counter',
  imports: [],
  templateUrl: './counter.html',
  styleUrl: './counter.scss',
})
export class Counter {
  count: number = 0;

  increase(): void {
    ++this.count;
  }

  decrease(): void {
    --this.count;
  }
}
