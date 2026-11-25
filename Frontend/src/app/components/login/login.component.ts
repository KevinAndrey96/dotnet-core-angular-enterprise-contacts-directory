import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../service/auth.service';
import { Login } from '../../models/Login';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  credentials: Login = {
    Username: 'kaherrera@hubbec.com',
    Pass: 'Password123!'
  };

  loading: boolean = false;
  errorMessage: string | null = null;
  returnUrl: string = '/dashboard';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    if (!this.credentials.Username || !this.credentials.Pass) {
      this.errorMessage = 'Por favor ingrese su correo y contrase\u00f1a.';
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    this.authService.login(this.credentials).subscribe({
      next: (res) => {
        this.loading = false;
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.alert || err.error?.message || 'Error al iniciar sesi\u00f3n. Verifique sus credenciales.';
      }
    });
  }
}
