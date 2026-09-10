import { Component } from '@angular/core';
import { Button } from '../button/button';

@Component({
  selector: 'app-hero',
  imports: [Button],
  templateUrl: './hero.html',
  styleUrl: './hero.scss',
})
export class Hero {}
