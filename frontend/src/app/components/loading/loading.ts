import { Component } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-loading',
  imports: [MatProgressSpinnerModule],
  template: '<div class="loading" role="status" aria-live="polite"><mat-spinner diameter="32" /><span>Aguarde…</span></div>',
  styles: '.loading { display: flex; align-items: center; justify-content: center; gap: 1rem; padding: 1rem; color: var(--color-primary-600); }',
})
export class Loading {}
