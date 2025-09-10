import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DepartmentsForm } from '../departments-form/departments-form';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-edit-departments',
  imports: [DepartmentsForm, RouterLink],
  templateUrl: './edit-departments.html',
  styleUrl: './edit-departments.scss',
})
export class EditDepartments implements OnInit, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();
  private readonly activateRoute: ActivatedRoute = inject(ActivatedRoute);
  departmentIdToEdit!: number;

  ngOnInit(): void {
    const sub = this.activateRoute.paramMap.subscribe({
      next: (params) => {
        this.departmentIdToEdit = Number(params.get('id'));
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
