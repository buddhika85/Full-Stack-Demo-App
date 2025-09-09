import { Component } from '@angular/core';
import { DepartmentsForm } from '../departments-form/departments-form';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-create-departments',
  imports: [DepartmentsForm, RouterLink],
  templateUrl: './create-departments.html',
  styleUrl: './create-departments.scss',
})
export class CreateDepartments {}
