import { NgClass } from '@angular/common';
import { Component, input } from '@angular/core';

@Component({
  selector: 'app-button',
  templateUrl: './button.html',
  styleUrl: './button.scss',
  imports: [NgClass],
  standalone: true,
})
export class Button {
  variant = input<'contained' | 'outline' | 'text'>('contained');
  type = input<'button' | 'reset' | 'submit'>('button');
  class = input<string>('');
}
