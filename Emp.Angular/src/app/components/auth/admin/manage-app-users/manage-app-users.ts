import {
  AfterViewInit,
  Component,
  inject,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { UserService } from '../../../../services/user.service';
import { Subscription } from 'rxjs';
import { UserDto } from '../../../../models/user.dto';

@Component({
  selector: 'app-manage-app-users',
  imports: [],

  templateUrl: './manage-app-users.html',
  styleUrl: './manage-app-users.scss',
})
export class ManageAppUsers implements OnInit, OnDestroy {
  private readonly userService: UserService = inject(UserService);
  private readonly compositeSubscription: Subscription = new Subscription();

  private users!: UserDto[];

  ngOnInit(): void {
    const sub = this.userService.getUsers().subscribe({
      next: (users: UserDto[]) => {
        this.users = users;
        console.log(users);
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  ngOnDestroy(): void {
    this.compositeSubscription.unsubscribe();
  }
}
