import {
  Component,
  inject,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { DepartmentService } from '../../../../services/department.service';

@Component({
  selector: 'app-departments-form',
  imports: [],
  templateUrl: './departments-form.html',
  styleUrl: './departments-form.scss',
})
export class DepartmentsForm implements OnChanges, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();
  private readonly departmentService: DepartmentService =
    inject(DepartmentService);
  @Input() departmentId: number | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (this.departmentId) {
      this.prepareFormForEditMode();
    }
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  prepareFormForEditMode() {
    throw new Error('Method not implemented.');
  }
}
