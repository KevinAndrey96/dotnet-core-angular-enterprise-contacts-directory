import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ContactsService } from '../../service/contacts.service';
import { AuthService } from '../../service/auth.service';
import { Contact, ContactDetails } from '../../models/Contact';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  currentUser: any = null;
  contacts: Contact[] = [];
  filteredContacts: Contact[] = [];
  searchTerm: string = '';
  activeTab: 'contacts' | 'new-contact' | 'directory' = 'contacts';

  // New contact form model
  newContact: Contact = {
    firstName: '',
    lastName: '',
    documentType: 'CC',
    documentNumber: '',
    phone: '',
    email: '',
    relationship: 'Amigo'
  };

  // Directory users for search
  directoryUsers: Contact[] = [];
  directorySearchQuery: string = '';

  // Selected contact details & mutual contacts modal
  selectedDetails: ContactDetails | null = null;
  selectedContactForEdit: Contact | null = null;
  editRelationshipValue: string = 'Amigo';

  loading: boolean = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private contactsService: ContactsService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadContacts();
    this.loadDirectoryUsers();
  }

  loadContacts(): void {
    this.loading = true;
    const userId = this.currentUser?.userId;
    this.contactsService.getContacts(userId).subscribe({
      next: (data) => {
        this.contacts = data || [];
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'No fue posible cargar los contactos.';
      }
    });
  }

  loadDirectoryUsers(): void {
    this.contactsService.searchDirectory(this.directorySearchQuery).subscribe({
      next: (data) => {
        this.directoryUsers = data || [];
      },
      error: (err) => {
        console.error('Error fetching directory users', err);
      }
    });
  }

  applyFilter(): void {
    if (!this.searchTerm.trim()) {
      this.filteredContacts = [...this.contacts];
      return;
    }
    const term = this.searchTerm.toLowerCase();
    this.filteredContacts = this.contacts.filter(c =>
      (c.firstName && c.firstName.toLowerCase().includes(term)) ||
      (c.lastName && c.lastName.toLowerCase().includes(term)) ||
      (c.email && c.email.toLowerCase().includes(term)) ||
      (c.documentNumber && c.documentNumber.includes(term)) ||
      (c.relationship && c.relationship.toLowerCase().includes(term))
    );
  }

  onAddContactSubmit(): void {
    this.clearAlerts();
    if (!this.newContact.firstName || !this.newContact.documentNumber || !this.newContact.relationship) {
      this.errorMessage = 'Por favor complete el nombre, documento y tipo de relaci\u00f3n.';
      return;
    }

    this.newContact.idUser = this.currentUser?.userId;
    this.loading = true;

    this.contactsService.addContact(this.newContact).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMessage = res.alert || 'Contacto agregado exitosamente.';
        this.resetNewContactForm();
        this.loadContacts();
        this.activeTab = 'contacts';
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.alert || 'Error al agregar contacto.';
      }
    });
  }

  addFromDirectory(user: Contact, relationship: string = 'Colega'): void {
    this.clearAlerts();
    const contactToAdd: Contact = {
      idUser: this.currentUser?.userId,
      idContact: user.idContact,
      firstName: user.firstName,
      lastName: user.lastName,
      documentType: user.documentType,
      documentNumber: user.documentNumber,
      phone: user.phone,
      email: user.email,
      relationship: relationship
    };

    this.contactsService.addContact(contactToAdd).subscribe({
      next: (res) => {
        this.successMessage = res.alert || `Contacto ${user.firstName} agregado con \u00e9xito.`;
        this.loadContacts();
        this.loadDirectoryUsers();
      },
      error: (err) => {
        this.errorMessage = err.error?.alert || 'Error al asociar usuario del directorio.';
      }
    });
  }

  viewDetails(contact: Contact): void {
    this.clearAlerts();
    if (!contact.id) return;
    this.contactsService.getContactDetails(contact.id).subscribe({
      next: (details) => {
        this.selectedDetails = details;
      },
      error: (err) => {
        this.errorMessage = 'No se pudieron obtener los detalles del contacto.';
      }
    });
  }

  closeDetailsModal(): void {
    this.selectedDetails = null;
  }

  openEditModal(contact: Contact): void {
    this.selectedContactForEdit = contact;
    this.editRelationshipValue = contact.relationship || 'Amigo';
  }

  closeEditModal(): void {
    this.selectedContactForEdit = null;
  }

  saveRelationshipEdit(): void {
    if (!this.selectedContactForEdit || !this.selectedContactForEdit.id) return;

    this.contactsService.updateContact(this.selectedContactForEdit.id, {
      relationship: this.editRelationshipValue,
      phone: this.selectedContactForEdit.phone,
      email: this.selectedContactForEdit.email
    }).subscribe({
      next: () => {
        this.successMessage = 'Relaci\u00f3n actualizada correctamente.';
        this.closeEditModal();
        this.loadContacts();
      },
      error: (err) => {
        this.errorMessage = err.error?.alert || 'Error al actualizar relaci\u00f3n.';
      }
    });
  }

  deleteContact(contact: Contact): void {
    if (!contact.id) return;
    const confirmDelete = confirm(`\u00bfEst\u00e1s seguro de desasociar a ${contact.firstName} ${contact.lastName}?`);
    if (!confirmDelete) return;

    this.contactsService.deleteContact(contact.id).subscribe({
      next: () => {
        this.successMessage = 'Contacto desasociado correctamente.';
        this.loadContacts();
      },
      error: (err) => {
        this.errorMessage = err.error?.alert || 'Error al eliminar contacto.';
      }
    });
  }

  resetNewContactForm(): void {
    this.newContact = {
      firstName: '',
      lastName: '',
      documentType: 'CC',
      documentNumber: '',
      phone: '',
      email: '',
      relationship: 'Amigo'
    };
  }

  clearAlerts(): void {
    this.errorMessage = null;
    this.successMessage = null;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
