import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DepartmentService } from '../../../services/department.service';

import { DepartmentDto } from '../../../models/department.dto';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';

import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

import { Subscription } from 'rxjs';

import { NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-manage-departments',
  imports: [MatFormFieldModule, MatInputModule, MatTableModule, MatIconModule],
  templateUrl: './manage-departments.html',
  styleUrl: './manage-departments.scss',
})
export class ManageDepartments implements OnInit, OnDestroy {
  private readonly departmentService: DepartmentService =
    inject(DepartmentService);
  private readonly compositeSubscription: Subscription = new Subscription();
  private departmentsList!: DepartmentDto[];

  readonly displayedColumns: string[] = ['id', 'name', 'actions'];
  dataSource!: MatTableDataSource<DepartmentDto>;

  ngOnInit(): void {
    this.loadDepartmentsToGrid();
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  addDepartment(): void {}

  editDepartment(id: number): void {}

  deleteDepartment(id: number): void {}

  private loadDepartmentsToGrid(): void {
    const sub = this.departmentService.getDepartments().subscribe({
      next: (value: DepartmentDto[]) => {
        //console.log(value);
        this.departmentsList = value;
        this.dataSource = new MatTableDataSource(this.departmentsList);
      },
      error: (error) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }
}
