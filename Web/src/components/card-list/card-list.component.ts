import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, startWith, switchMap, Observable, Subject, merge } from 'rxjs';
import { BusinessCardService } from '../../services/business-card.service';
import { BusinessCardComponent } from '../business-card/business-card.component';
import { NotificationService } from '../../services/notification.service';
import { BusinessCard } from '../../models/business-card.model';

@Component({
    selector: 'app-card-list',
    standalone: true,
    imports: [CommonModule, RouterLink, ReactiveFormsModule, BusinessCardComponent],
    templateUrl: './card-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardListComponent implements OnInit {
    private cardService = inject(BusinessCardService);
    private notificationService = inject(NotificationService);
    private router = inject(Router);

    filterControl = new FormControl<string>('');
    private reload$ = new Subject<void>(); // ✅ Trigger reload on delete

    filteredCards$!: Observable<BusinessCard[]>;

    ngOnInit(): void {
        // Merge filter changes and reload trigger
        this.filteredCards$ = merge(
            this.filterControl.valueChanges.pipe(
                debounceTime(300),
                distinctUntilChanged(),
                startWith('') // initial load
            ),
            this.reload$ // triggered after delete
        ).pipe(
            switchMap(() => this.cardService.getAll(this.filterControl.value ?? ''))
        );
    }

    handleDelete(id: number) {
        if (confirm('Are you sure you want to delete this business card?')) {
            this.cardService.delete(id).subscribe(() => {
                this.notificationService.show('Card deleted successfully.', 'success');
                this.reload$.next(); // ✅ trigger list refresh
            });
        }
    }

    private triggerDownload(blob: Blob, filename: string) {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
    }

    exportCsv() {
        this.cardService.export('csv').subscribe(blob => {
            this.triggerDownload(blob, 'business-cards.csv');
            this.notificationService.show('Exporting cards as CSV.', 'info');
        });
    }

    exportXml() {
        this.cardService.export('xml').subscribe(blob => {
            this.triggerDownload(blob, 'business-cards.xml');
            this.notificationService.show('Exporting cards as XML.', 'info');
        });
    }

    trackByCardId(index: number, card: BusinessCard): number {
        return card.id;
    }
}
