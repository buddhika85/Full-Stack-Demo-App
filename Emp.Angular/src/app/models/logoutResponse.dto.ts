export interface LogoutResponseDto {
  loggedOut: boolean;
  username: string;
  logoutMessage: string | null;
}
