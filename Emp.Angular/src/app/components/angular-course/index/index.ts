import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-index',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './index.html',
  styleUrl: './index.scss',
})
export class Index {}
