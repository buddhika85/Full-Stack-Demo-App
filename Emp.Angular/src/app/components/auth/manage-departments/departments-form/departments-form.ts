import {
  Component,
  inject,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { DepartmentService } from '../../../../services/department.service';
import { DepartmentDto } from '../../../../models/department.dto';
import { SnackbarService } from '../../../../services/snackbar.service';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  Validators,
} from '@angular/forms';

@Component({
  selector: 'app-departments-form',
  imports: [],
  templateUrl: './departments-form.html',
  styleUrl: './departments-form.scss',
})
export class DepartmentsForm implements OnInit, OnChanges, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();
  private readonly snackBarService: SnackbarService = inject(SnackbarService);
  private readonly departmentService: DepartmentService =
    inject(DepartmentService);
  @Input() departmentId: number | null = null;

  private departmentModel: DepartmentDto = { id: 0, name: '' };
  private readonly formBuilder: FormBuilder = new FormBuilder();
  formGroup!: FormGroup<{
    name: FormControl<string>;
  }>;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.departmentId) {
      this.prepareFormForEditMode(this.departmentId);
    }
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  private buildForm(): void {
    this.formGroup = this.formBuilder.group({
      name: new FormControl<string>(this.departmentModel.name, {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(50),
        ],
      }),
    });
  }

  private prepareFormForEditMode(editId: number) {
    const sub = this.departmentService.getDepartmentById(editId).subscribe({
      next: (value: DepartmentDto) => {
        this.departmentModel = { ...value };
      },
      error: (error: any) => {
        //console.error('Error - ', error);
        if (error.error && error.error.detail) {
          this.snackBarService.error(`${error.error.detail}`);
          return;
        }

        this.snackBarService.error(
          `Error occured while retrieving department with ID ${editId}`
        );
      },
    });
    this.compositeSubscription.add(sub);
  }
}
