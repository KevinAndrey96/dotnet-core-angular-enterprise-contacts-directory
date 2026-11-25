import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../service/auth.service';
import { User } from '../../models/User';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  user: User = {
    firstName: '',
    lastName: '',
    documentType: 'CC',
    documentNumber: '',
    phone: '',
    email: '',
    password: ''
  };

  confirmPassword: string = '';
  loading: boolean = false;
  errorMessage: string | null = null;
  infoMessage: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSubmit(): void {
    this.errorMessage = null;
    this.infoMessage = null;

    if (!this.user.firstName || !this.user.lastName || !this.user.documentNumber || !this.user.phone || !this.user.email || !this.user.password) {
      this.errorMessage = 'Por favor complete todos los campos obligatorios.';
      return;
    }

    if (this.user.password !== this.confirmPassword) {
      this.errorMessage = 'Las contrase\u00f1as no coinciden.';
      return;
    }

    this.loading = true;

    this.authService.register(this.user).subscribe({
      next: (res) => {
        this.loading = false;
        if (res && res.registeredByMessage) {
          this.infoMessage = res.registeredByMessage;
          setTimeout(() => {
            this.router.navigate(['/dashboard']);
          }, 3500);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.alert || err.error?.message || 'Error durante el registro. Intente con otro correo o documento.';
      }
    });
  }
}
