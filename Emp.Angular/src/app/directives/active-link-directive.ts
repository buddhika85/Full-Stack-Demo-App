import { Directive, ElementRef, Renderer2 } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';

@Directive({
  selector: '[appActiveLinkDirective]',
})
export class ActiveLinkDirective {
  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        const currentUrl = this.router.url;
        const linkUrl =
          this.el.nativeElement.getAttribute('href') ||
          this.el.nativeElement.getAttribute('routerLink');

        if (currentUrl.endsWith(linkUrl)) {
          this.renderer.addClass(this.el.nativeElement, 'active-link');
        } else {
          this.renderer.removeClass(this.el.nativeElement, 'active-link');
        }
      });
  }
}
