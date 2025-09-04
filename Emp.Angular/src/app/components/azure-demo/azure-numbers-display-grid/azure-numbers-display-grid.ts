import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core';
import { AzNumberListDto } from '../../../models/azNumberListDto';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { AzNumItemDto } from '../../../models/azNumItemDto';

@Component({
  selector: 'app-azure-numbers-display-grid',
  imports: [MatFormFieldModule, MatInputModule, MatTableModule, MatIconModule],
  templateUrl: './azure-numbers-display-grid.html',
  styleUrl: './azure-numbers-display-grid.scss',
})
export class AzureNumbersDisplayGrid implements OnChanges, OnInit {
  @Input() numbersDto: AzNumberListDto = {
    isSuccess: true,
    items: [],
  };
  @Input() heading: string = '';

  readonly displayedColumns: string[] = ['id', 'number'];
  dataSource!: MatTableDataSource<AzNumItemDto>;

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    // was numbersDto received in this ngOnChanges ?
    if (changes['numbersDto'] && this.numbersDto?.items) {
      this.dataSource = new MatTableDataSource(this.numbersDto.items);

      this.dataSource.filterPredicate = (
        data: AzNumItemDto,
        filter: string
      ) => {
        const normalizedFilter = filter.trim().toLowerCase();
        return data.number.toString().includes(normalizedFilter);
      };
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }
}
