import { BaseDto } from './base.dto';
import { UserRoles } from './userRoles';

export interface UpdateUserDto extends BaseDto {
  username: string;
  firstName: string;
  lastName: string;
  role: number;
  isActive: boolean;
}
