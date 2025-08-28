import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { UserService } from '../../../../services/user.service';
import { Subscription } from 'rxjs';
import { UserDto } from '../../../../models/user.dto';
import { BoolToYesNoPipe } from '../../../../pipes/bool-to-yes-no-pipe';
import { UserRoleEnumToUserRolePipe } from '../../../../pipes/user-role-enum-to-user-role-pipe';
import { NgClass } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { SnackbarService } from '../../../../services/snackbar.service';

@Component({
  selector: 'app-manage-app-users',
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    BoolToYesNoPipe,
    UserRoleEnumToUserRolePipe,
    NgClass,
    MatIconModule,
  ],

  templateUrl: './manage-app-users.html',
  styleUrl: './manage-app-users.scss',
})
export class ManageAppUsers implements OnInit, OnDestroy {
  private readonly router: Router = inject(Router);
  private readonly userService: UserService = inject(UserService);
  private readonly snackbarService: SnackbarService = inject(SnackbarService);
  private readonly compositeSubscription: Subscription = new Subscription();

  private users!: UserDto[];

  readonly displayedColumns: string[] = [
    'id',
    'username',
    'firstName',
    'lastName',
    'role',
    'isActive',
    'actions',
  ];
  dataSource!: MatTableDataSource<UserDto>;

  ngOnInit(): void {
    this.loadUsersGrid();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  addUser(): void {
    this.router.navigate(['manage-app-users/create']);
  }

  onEdit(id: number) {
    this.router.navigate(['manage-app-users/edit', id]);
  }

  onDeactivate(id: number) {
    const sub = this.userService.activateDeactivateUser(id).subscribe({
      next: () => {
        this.snackbarService.success('User active status updated');
        this.loadUsersGrid(); // reload grid
      },
      error: (err: any) => {
        console.error('User status change error', err);
        let errorMsg: string =
          err.error.detail ??
          `An error occured while changing active status of the user with Id ${id}.`;
        errorMsg = `Error - ${errorMsg}`;
        console.error(errorMsg);
        this.snackbarService.error(errorMsg);
      },
    });
    this.compositeSubscription.add(sub);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }

  private loadUsersGrid(): void {
    const sub = this.userService.getUsers().subscribe({
      next: (users: UserDto[]) => {
        this.users = users;
        this.dataSource = new MatTableDataSource(this.users);
      },
      error: (error) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }
}
