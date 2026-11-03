import { Router } from '@angular/router';
import { Component, inject } from '@angular/core';
import { Button } from '../button/button';

@Component({
  selector: 'app-hero',
  imports: [Button],
  templateUrl: './hero.html',
  styleUrl: './hero.scss',
})
export class Hero {
  private readonly router = inject(Router);

  openMap(): void {
    void this.router.navigate(['/mapa']);
  }
}
