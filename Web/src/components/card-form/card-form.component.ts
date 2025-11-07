import { ChangeDetectionStrategy, Component, inject, signal, effect, WritableSignal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BusinessCardComponent } from '../business-card/business-card.component';
import { BusinessCardService } from '../../services/business-card.service';
import { NotificationService } from '../../services/notification.service';
import { BusinessCard } from '../../models/business-card.model';

@Component({
    selector: 'app-card-form',
    standalone: true,
    imports: [CommonModule, RouterLink, ReactiveFormsModule, BusinessCardComponent],
    templateUrl: './card-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardFormComponent {
    private fb = inject(FormBuilder);
    private router = inject(Router);
    private cardService = inject(BusinessCardService);
    private notificationService = inject(NotificationService);

    cardForm: FormGroup;
    cardPreview: WritableSignal<Partial<BusinessCard>>;

    constructor() {
        this.cardForm = this.fb.group({
            name: ['', Validators.required],
            gender: ['', Validators.required],
            dateOfBirth: ['', Validators.required],
            email: ['', [Validators.required, Validators.email]],
            phone: ['', Validators.required],
            address: ['', Validators.required],
            photo: [''],
        });

        this.cardPreview = signal(this.cardForm.value);

        this.cardForm.valueChanges.subscribe(value => {
            this.cardPreview.set(value);
        });
    }

    handlePhotoUpload(event: Event): void {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (file) {
            // Basic check for image type and size
            if (!file.type.startsWith('image/')) {
                this.notificationService.show('Please select an image file.', 'error');
                return;
            }
            if (file.size > 1 * 1024 * 1024) { // 1MB
                this.notificationService.show('Image size should not exceed 1MB.', 'error');
                return;
            }

            const reader = new FileReader();
            reader.onload = (e) => {
                this.cardForm.patchValue({ photo: e.target?.result as string });
            };
            reader.readAsDataURL(file);
        }
    }

    handleImportCsv(event: Event): void {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;

        this.cardService.import(file).subscribe({
            next: response => {
                this.notificationService.show('Business card created successfully!', 'success');
                this.router.navigate(['/cards']);
            },
            error: err => {
                console.error(err);
                this.notificationService.show('Failed to import CSV file.', 'error');
            }
        });
        (event.target as HTMLInputElement).value = '';
    }

    handleImportXml(event: Event): void {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;

        this.cardService.import(file).subscribe({
            next: response => {
                this.notificationService.show('Business card created successfully!', 'success');
                this.router.navigate(['/cards']);
            },
            error: err => {
                console.error(err);
                this.notificationService.show('Failed to import XML file.', 'error');
            }
        });
        (event.target as HTMLInputElement).value = '';
    }

    handleImportQr(event: Event): void {
        const file = (event.target as HTMLInputElement).files?.[0];
        if (!file) return;

        this.cardService.importQr(file).subscribe({
            next: response => {
                this.notificationService.show('Business card created successfully!', 'success');
                this.router.navigate(['/cards']);
            },
            error: err => {
                console.error(err);
                this.notificationService.show('Failed to import QR code image.', 'error');
            }
        });
        (event.target as HTMLInputElement).value = '';
    }

    onSubmit(): void {
        this.cardForm.markAllAsTouched();
        if (this.cardForm.valid) {
            this.cardService.create(this.cardForm.value).subscribe({
                next: response => {
                    this.notificationService.show('Business card created successfully!', 'success');
                    this.router.navigate(['/cards']);
                },
                error: err => {
                    console.error(err);
                    this.notificationService.show('Failed to create business card.', 'error');
                }
            });
        } else {
            this.notificationService.show('Please fill out all required fields correctly.', 'error');
        }
    }
}