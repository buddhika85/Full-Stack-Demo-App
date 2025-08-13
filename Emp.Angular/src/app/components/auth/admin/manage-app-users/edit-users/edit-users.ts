import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { UsersForm } from '../users-form/users-form';

@Component({
  selector: 'app-edit-users',
  imports: [RouterLink, UsersForm],
  templateUrl: './edit-users.html',
  styleUrl: './edit-users.scss',
})
export class EditUsers implements OnInit, OnDestroy {
  id!: number;
  private readonly activatedRoute: ActivatedRoute = inject(ActivatedRoute);
  private readonly compositeSubscription: Subscription = new Subscription();

  ngOnInit(): void {
    const sub = this.activatedRoute.paramMap.subscribe({
      next: (params) => {
        this.id = Number(params.get('id'));
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
