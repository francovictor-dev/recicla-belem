import { Component } from '@angular/core';
import { LucideAngularModule, Map, MapPin } from 'lucide-angular';
import { Button } from '../button/button';
import { Card } from '../card/card';

@Component({
  selector: 'app-how-work',
  imports: [Card, Button, LucideAngularModule, Button],
  templateUrl: './how-work.html',
  styleUrl: './how-work.scss',
})
export class HowWork {
  readonly icons = { MapPin, Map };
}
