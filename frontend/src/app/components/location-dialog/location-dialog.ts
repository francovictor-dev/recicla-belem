import { CommonModule } from '@angular/common';
import { Component, inject, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CollectorPointService } from '../../services/collector-point';
import { Button } from '../button/button';
import { InputComponent } from '../input/input';

@Component({
  selector: 'app-location-dialog',
  imports: [CommonModule, InputComponent, ReactiveFormsModule, Button],
  templateUrl: './location-dialog.html',
  styleUrl: './location-dialog.scss',
})
export class LocationDialog {
  collectorPointService = inject(CollectorPointService);
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: { lat: number; lng: number; collectors: Collector[] },
    private fb: FormBuilder,
  ) {
    this.createPointForm = this.fb.group({
      complement: [''],
    });
  }

  createPointForm!: FormGroup;
  fileImg!: File;
  selectedItems: Collector[] = [];

  selectedIds: number[] = [];

  toggleCollector(id: number) {
    const exists = this.selectedIds.includes(id);

    if (exists) {
      this.selectedIds = this.selectedIds.filter((x) => x !== id);
    } else {
      this.selectedIds.push(id);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;

    if (!input.files?.length) return;

    const file = input.files[0];
    this.fileImg = file;
  }

  async submit() {
    if (!this.fileImg) {
      alert('Imagem é necessária. Por favor, tente novamente.');
      return;
    }

    if (this.selectedIds.length === 0) {
      alert('Selecione pelo menos um coletor. Por favor, tente novamente.');
      return;
    }

    if (this.createPointForm.invalid) {
      this.createPointForm.markAllAsTouched();
      return;
    }

    const createCollectorPointDTO: CreateCollectorPointDTO = {
      address: {
        complement: this.createPointForm.get('complement')?.value,
        lat: this.data.lat,
        lng: this.data.lng,
      },
      collectors: this.selectedIds,
      is_active: true,
      status: 'pending',
    };

    this.collectorPointService
      .createCollectorPoint(createCollectorPointDTO, this.fileImg)
      .subscribe({
        next: (data) => {
          alert('Ponto de coleta criado com sucesso!');
          console.log(data);
        },
        error: (error) => {
          console.log(error);
          alert('Ocorreu um erro ao criar o ponto de coleta. Por favor, tente novamente.');
        },
      });
  }
}
