import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { ActiveLinkDirective } from '../../../directives/active-link-directive';

@Component({
  selector: 'app-index',
  imports: [RouterOutlet, RouterLink, ActiveLinkDirective],
  templateUrl: './index.html',
  styleUrl: './index.scss',
})
export class Index {}
