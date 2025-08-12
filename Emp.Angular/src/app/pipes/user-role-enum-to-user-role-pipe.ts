import { Pipe, PipeTransform } from '@angular/core';
import { UserRoles } from '../models/userRoles';

@Pipe({
  name: 'userRoleEnumToUserRole',
  standalone: true,
})
export class UserRoleEnumToUserRolePipe implements PipeTransform {
  transform(value: number): unknown {
    return UserRoles[value];
  }
}
