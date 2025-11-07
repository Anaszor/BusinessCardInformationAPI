import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusinessCard } from '../models/business-card.model';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class BusinessCardService {
    private apiUrl = 'https://localhost:7131/api/BusinessCard';

    constructor(private http: HttpClient) { }

    create(card: BusinessCard): Observable<any> {
        return this.http.post(this.apiUrl, card);
    }

    getAll(filterText: any = ''): Observable<BusinessCard[]> {
        const params = filterText ? { search: filterText } : {};
        return this.http.get<BusinessCard[]>(this.apiUrl, { params });
    }

    delete(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    export(format: 'csv' | 'xml'): Observable<Blob> {
        return this.getAll().pipe(
            map(cardsData => {
                if (format === 'csv') {
                    const header = 'Name,Gender,DateOfBirth,Email,Phone,Address\n';
                    const rows = cardsData
                        .map(c => `${c.name},${c.gender},${c.dateOfBirth},${c.email},${c.phone},"${c.address}",${c.photo}`)
                        .join('\n');
                    return new Blob([header + rows], { type: 'text/csv;charset=utf-8;' });
                } else { // xml
                    const cardsXml = cardsData
                        .map(c =>
                            `  <BusinessCard>
  <Name>${c.name}</Name>
  <Gender>${c.gender}</Gender>
  <DateOfBirth>${c.dateOfBirth}</DateOfBirth>
  <Email>${c.email}</Email>
  <Phone>${c.phone}</Phone>
  <Address>${c.address}</Address>
  <Photo>${c.photo || ''}</Photo>
</BusinessCard>`
                        )
                        .join('\n');
                    const xmlString = `<?xml version="1.0" encoding="UTF-8"?>\n<BusinessCards>\n${cardsXml}\n</BusinessCards>`;
                    return new Blob([xmlString], { type: 'application/xml;charset=utf-8;' });
                }
            })
        );
    }


    // Mock import functionality

    import(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post(`${this.apiUrl}/import`, formData);
    }

    // Import QR Code image
    importQr(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('qrFile', file);
        return this.http.post(`${this.apiUrl}/import-qr`, formData);
    }

}
