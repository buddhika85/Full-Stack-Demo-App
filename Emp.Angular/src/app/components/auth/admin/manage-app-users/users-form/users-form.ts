import {
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

@Component({
  selector: 'app-users-form',
  imports: [ReactiveFormsModule],
  templateUrl: './users-form.html',
  styleUrl: './users-form.scss',
})
export class UsersForm implements OnInit, OnChanges, OnDestroy {
  private readonly compositeSubscription: Subscription = new Subscription();

  @Input() userId!: number | null;
  private editMode: boolean = false;
  private readonly userService: UserService = inject(UserService);

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
          Validators.minLength(6),
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
          Validators.minLength(6),
          Validators.maxLength(50),
        ],
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(6),
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
      this.password.disable();
      this.prepareFormForEditMode(this.userId);
    } else {
      this.editMode = false;
      this.password.enable();
    }
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
    if (this.formGroup.valid) {
      // save user
    }
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
