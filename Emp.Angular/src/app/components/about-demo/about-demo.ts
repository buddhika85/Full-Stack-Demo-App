import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-about-demo',
  imports: [],
  templateUrl: './about-demo.html',
  styleUrl: './about-demo.scss',
})
export class AboutDemo {
  currentTime = signal<string>(
    new Date().toLocaleString('en-AU', {
      timeZone: 'Australia/Sydney',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false,
    })
  );
}
