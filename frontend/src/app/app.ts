import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpLoadingService } from './services/http-loading.service';
import { Loading } from './components/loading/loading';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Loading],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  readonly httpLoading = inject(HttpLoadingService);
  protected readonly title = signal('recicla-belem-web');
}
