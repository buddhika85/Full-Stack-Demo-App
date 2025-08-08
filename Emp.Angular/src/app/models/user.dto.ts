import { BaseDto } from './base.dto';
import { UserRoles } from './userRoles';

export interface UserDto extends BaseDto {
  username: string;
  firstName: string;
  lastName: string;
  role: UserRoles;
  isActive: boolean;
}
