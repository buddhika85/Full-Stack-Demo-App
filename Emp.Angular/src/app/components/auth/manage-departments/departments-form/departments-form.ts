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
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { CreateDepartmentDto } from '../../../../models/createDepartment.dto';
import { UpdateDepartmentDto } from '../../../../models/updateDepartment.dto';
import { Router } from '@angular/router';

@Component({
  selector: 'app-departments-form',
  imports: [ReactiveFormsModule],
  templateUrl: './departments-form.html',
  styleUrl: './departments-form.scss',
})
export class DepartmentsForm implements OnInit, OnChanges, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();
  private readonly router: Router = inject(Router);
  private readonly snackBarService: SnackbarService = inject(SnackbarService);
  private readonly departmentService: DepartmentService =
    inject(DepartmentService);
  @Input() departmentId: number | null = null;

  private departmentModel: DepartmentDto = { id: 0, name: '' };
  private readonly formBuilder: FormBuilder = new FormBuilder();
  private editMode: boolean = false;
  formGroup!: FormGroup<{
    name: FormControl<string>;
  }>;

  ngOnInit(): void {
    this.buildForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.departmentId) {
      this.editMode = true;
      this.prepareFormForEditMode(this.departmentId);
    }
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  get name(): FormControl<string> {
    return this.formGroup.controls.name;
  }

  onSubmit(): void {
    // console.log(this.formGroup.getRawValue());
    if (this.formGroup.invalid) return;
    if (this.editMode) {
      this.updateDepartment();
      return;
    }
    this.createDepartment();
  }

  onReset(): void {
    this.formGroup.reset();
    if (this.editMode) this.prepareFormForEditMode(this.departmentModel.id);
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
        this.formGroup.patchValue(this.departmentModel);
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

  private createDepartment(): void {
    const createDepartmentDto: CreateDepartmentDto = {
      ...this.formGroup.getRawValue(),
    };
    const sub = this.departmentService
      .createDepartment(createDepartmentDto)
      .subscribe({
        next: (value: DepartmentDto) => {
          if (value) {
            this.snackBarService.success(
              `New Department with ID ${value.id} and Name ${value.name} was created`
            );
            setTimeout(() => {
              this.router.navigate(['manage-departments']);
            }, 3000);
            return;
          }
          this.snackBarService.warn(`Please check`);
        },
        error: (error: any) => {
          if (error.error && error.error.detail) {
            this.snackBarService.error(`${error.error.detail}`);
            return;
          }

          this.snackBarService.error(
            `Error occured while creating new department with Name ${createDepartmentDto.name}`
          );
        },
      });
    this.compositeSubscription.add(sub);
  }

  private updateDepartment(): void {
    const updateDepartmentDto: UpdateDepartmentDto = {
      id: this.departmentModel.id,
      ...this.formGroup.getRawValue(),
    };
    const sub = this.departmentService
      .updateDepartment(this.departmentModel.id, updateDepartmentDto)
      .subscribe({
        next: () => {
          this.snackBarService.success(
            `Department with ID ${this.departmentModel.id} was updated`
          );

          setTimeout(() => {
            this.router.navigate(['manage-departments']);
          }, 3000);
        },
        error: (error: any) => {
          if (error.error && error.error.detail) {
            this.snackBarService.error(`${error.error.detail}`);
            return;
          }

          this.snackBarService.error(
            `Error occured while updating department with ID ${updateDepartmentDto.id}`
          );
        },
      });
    this.compositeSubscription.add(sub);
  }
}
