export interface User {
  id?: number;
  firstName: string;
  lastName: string;
  documentType: string;
  documentNumber: string;
  phone: string;
  email: string;
  password?: string;
}
