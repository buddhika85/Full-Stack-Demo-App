import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({
  providedIn: 'root',
})
export class SnackbarService {
  private snackBar = inject(MatSnackBar);

  error(message: string) {
    this.snackBar.open(message, '', {
      duration: 5000,
      panelClass: ['snack-error'],
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }

  success(message: string) {
    this.snackBar.open(message, '', {
      duration: 5000,
      panelClass: ['snack-success'],
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }

  info(message: string) {
    this.snackBar.open(message, '', {
      duration: 4000,
      panelClass: ['snack-info'],
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }

  warn(message: string) {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      panelClass: ['snack-warn'],
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
    });
  }
}
