import {
  AfterViewInit,
  Component,
  inject,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UserRoles } from '../../../../../models/userRoles';
import { UserService } from '../../../../../services/user.service';
import { UserDto } from '../../../../../models/user.dto';
import { Subscription } from 'rxjs';
import { UpdateUserDto } from '../../../../../models/updateUser.dto';
import { CreateUserDto } from '../../../../../models/createUser.dto';
import { SnackbarService } from '../../../../../services/snackbar.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-users-form',
  imports: [ReactiveFormsModule],
  templateUrl: './users-form.html',
  styleUrl: './users-form.scss',
})
export class UsersForm implements OnInit, OnChanges, AfterViewInit, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();

  @Input() userId!: number | null;
  private editMode: boolean = false;
  private readonly userService: UserService = inject(UserService);
  private readonly snackbarService: SnackbarService = inject(SnackbarService);
  private readonly router: Router = inject(Router);

  UserRolesEnum = UserRoles; // expose enum to template
  userRoles: (keyof typeof UserRoles)[] = Object.keys(UserRoles).filter((key) =>
    isNaN(Number(key))
  ) as (keyof typeof UserRoles)[];

  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  formGroup!: FormGroup<{
    username: FormControl<string>;
    password: FormControl<string>;
    firstName: FormControl<string>;
    lastName: FormControl<string>;
    role: FormControl<UserRoles>;
  }>;

  ngOnInit(): void {
    this.formGroup = this.formBuilder.group({
      username: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.email,
          Validators.minLength(4),
          Validators.maxLength(50),
        ],
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      firstName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(4),
          Validators.maxLength(50),
        ],
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(4),
          Validators.maxLength(50),
        ],
      }),
      role: new FormControl(UserRoles.Staff, {
        nonNullable: true,
        validators: [Validators.required],
      }),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.userId) {
      this.editMode = true;
      this.prepareFormForEditMode(this.userId);
    } else {
      this.editMode = false;
    }
  }

  ngAfterViewInit(): void {
    if (this.editMode) {
      // edit mode - remove all valiators of password and disable password form control
      this.password.clearValidators();
      this.password.updateValueAndValidity();
      this.password.disable();
    } else {
      this.password.setValidators([
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(50),
      ]);
      this.password.updateValueAndValidity();
      this.password.enable();
    }
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  private prepareFormForEditMode(editUserId: number): void {
    const sub = this.userService.getUser(editUserId).subscribe({
      next: (user: UserDto) => {
        this.formGroup.patchValue(user);
      },
      error: (error) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  get username(): FormControl<string> {
    return this.formGroup.controls.username;
  }

  get password(): FormControl<string> {
    return this.formGroup.controls.password;
  }

  get firstName(): FormControl<string> {
    return this.formGroup.controls.firstName;
  }

  get lastName(): FormControl<string> {
    return this.formGroup.controls.lastName;
  }

  get role(): FormControl<UserRoles> {
    return this.formGroup.controls.role;
  }

  reset() {
    this.formGroup.reset();
  }

  onSubmit() {
    if (!this.formGroup.valid || !this.formGroup.dirty) {
      return;
    }

    if (this.editMode) {
      const updateUserDto: UpdateUserDto = this.mapToUpdateUserDto();
      console.log('update', updateUserDto);
    } else {
      const createUserDto: CreateUserDto = this.mapToCreateUserDto();
      console.log('create ', createUserDto);

      const sub = this.userService.createUser(createUserDto).subscribe({
        next: (value: UserDto) => {
          this.snackbarService.success(
            `A new user with ID ${value.id} and username ${value.username} was created. Back to Users List`
          );
          setTimeout(() => {
            this.router.navigate(['manage-app-users']);
          }, 3000);
        },
        error: (err: any) => {
          console.error('User creation error', err);
          let errorMsg: string =
            err.error.detail ??
            `An error occured while creating a user with username ${createUserDto.username}.`;
          errorMsg = `Error - ${errorMsg}`;
          console.error(errorMsg);
          this.snackbarService.error(errorMsg);
        },
      });
      this.compositeSubscription.add(sub);
    }
  }

  mapToUpdateUserDto(): UpdateUserDto {
    const { password, ...formValues } = this.formGroup.getRawValue();

    const updateUserDto: UpdateUserDto = {
      id: this.userId!,
      isActive: true,
      ...formValues,
    };
    return updateUserDto;
  }

  mapToCreateUserDto(): CreateUserDto {
    const createUserDto: CreateUserDto = {
      ...this.formGroup.getRawValue(),
    };
    return createUserDto;
  }
}
