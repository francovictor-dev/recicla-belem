import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, Header, Footer],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.scss',
})
export class MainLayout {}
