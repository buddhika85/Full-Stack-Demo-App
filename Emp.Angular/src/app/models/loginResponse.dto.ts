import { UserDto } from './user.dto';

export interface LoginResponseDto {
  token: string;
  user: UserDto;
}
