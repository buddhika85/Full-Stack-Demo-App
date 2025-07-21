import { Component } from '@angular/core';
import { Navbar } from './shared/components/navbar/navbar';
import { RouterOutlet } from '@angular/router';
import { Footer } from './shared/components/footer/footer';

@Component({
  selector: 'app-root',
  imports: [Navbar, RouterOutlet, Footer], // RouterOutlet
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
