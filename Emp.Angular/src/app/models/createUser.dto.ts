import { BaseDto } from './base.dto';
import { UserRoles } from './userRoles';

export interface CreateUserDto extends BaseDto {
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  role: UserRoles;
  isActive: boolean;
}
