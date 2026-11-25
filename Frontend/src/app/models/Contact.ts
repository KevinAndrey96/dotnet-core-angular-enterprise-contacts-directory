export interface Contact {
  id?: number;
  idUser?: number;
  idContact?: number;
  firstName: string;
  lastName: string;
  documentType: string;
  documentNumber: string;
  phone: string;
  email: string;
  relationship: string;
  isRegisteredUser?: boolean;
}

export interface ContactDetails {
  contact: Contact;
  isRegisteredUser: boolean;
  mutualContacts: Contact[];
  otherContacts: Contact[];
}
