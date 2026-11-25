import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Contact, ContactDetails } from '../models/Contact';

@Injectable({
  providedIn: 'root'
})
export class ContactsService {
  private readonly API_URL = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getContacts(userId?: number): Observable<Contact[]> {
    const url = userId ? `${this.API_URL}/Contacts/${userId}` : `${this.API_URL}/Contacts`;
    return this.http.get<Contact[]>(url);
  }

  getContactById(contactId: number): Observable<Contact> {
    return this.http.get<Contact>(`${this.API_URL}/Contacts/${contactId}`);
  }

  getContactDetails(contactId: number): Observable<ContactDetails> {
    return this.http.get<ContactDetails>(`${this.API_URL}/Contacts/${contactId}/details`);
  }

  addContact(contact: Contact): Observable<any> {
    return this.http.post<any>(`${this.API_URL}/Contacts`, contact);
  }

  updateContact(contactId: number, data: { relationship: string; phone?: string; email?: string }): Observable<any> {
    return this.http.put<any>(`${this.API_URL}/Contacts/${contactId}`, data);
  }

  deleteContact(contactId: number): Observable<any> {
    return this.http.delete<any>(`${this.API_URL}/Contacts/${contactId}`);
  }

  searchDirectory(query?: string): Observable<Contact[]> {
    const q = query ? `?q=${encodeURIComponent(query)}` : '';
    return this.http.get<Contact[]>(`${this.API_URL}/Contacts/search-directory${q}`);
  }
}
