import { Component } from '@angular/core';
import { Hero } from '../../components/hero/hero';
import { HowWork } from '../../components/how-work/how-work';

@Component({
  selector: 'app-home',
  imports: [Hero, HowWork],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {}
