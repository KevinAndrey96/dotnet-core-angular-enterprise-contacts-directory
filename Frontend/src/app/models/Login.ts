export interface Login {
  Username: string;
  Pass: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  registeredByMessage?: string;
}
