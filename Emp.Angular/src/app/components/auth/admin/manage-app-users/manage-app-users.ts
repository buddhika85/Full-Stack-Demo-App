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
  private readonly userService: UserService = inject(UserService);
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
    const sub = this.userService.getUsers().subscribe({
      next: (users: UserDto[]) => {
        this.users = users;
        this.dataSource = new MatTableDataSource(this.users);
        console.log(users);
      },
      error: (error) => {
        console.error(error);
      },
    });
    this.compositeSubscription.add(sub);
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  addUser(): void {
    console.log('add user');
  }

  onDeactivate(id: number) {
    console.log(`Activate/Deactivate ${id}`);
  }

  onEdit(id: number) {
    console.log(`Edit ${id}`);
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
