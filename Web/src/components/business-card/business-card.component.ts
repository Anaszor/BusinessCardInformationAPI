
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessCard } from '../../models/business-card.model';

@Component({
  selector: 'app-business-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './business-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BusinessCardComponent {
  card = input.required<Partial<BusinessCard>>();
  showActions = input<boolean>(true);
  delete = output<number>();

  onDelete() {
    if (this.card().id) {
        this.delete.emit(this.card().id);
    }
  }
}
