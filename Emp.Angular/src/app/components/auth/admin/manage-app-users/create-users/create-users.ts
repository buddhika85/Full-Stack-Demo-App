import { Component } from '@angular/core';
import { UsersForm } from '../users-form/users-form';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-create-users',
  imports: [UsersForm, RouterLink],
  templateUrl: './create-users.html',
  styleUrl: './create-users.scss',
})
export class CreateUsers {}
