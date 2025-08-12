import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'boolToYesNo',
  standalone: true,
})
export class BoolToYesNoPipe implements PipeTransform {
  transform(
    value: boolean,
    trueLabel: string = 'Yes',
    falseLabel: string = 'No'
  ): string {
    return value ? trueLabel : falseLabel;
  }
}
