import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-departments-form',
  imports: [],
  templateUrl: './departments-form.html',
  styleUrl: './departments-form.scss',
})
export class DepartmentsForm {
  @Input() departmentId: number | null = null;
}
