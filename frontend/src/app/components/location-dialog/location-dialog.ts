import { CommonModule } from "@angular/common";
import {
  Component,
  DestroyRef,
  inject,
  Inject,
  OnInit,
  signal,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormBuilder, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { firstValueFrom } from "rxjs";
import { CollectorService } from "../../services/collector";
import { CollectorPointService } from "../../services/collector-point";
import { Button } from "../button/button";
import { Loading } from "../loading/loading";

@Component({
  selector: "app-location-dialog",
  imports: [
    CommonModule,
    ReactiveFormsModule,
    Button,
    MatSnackBarModule,
    Loading,
  ],
  templateUrl: "./location-dialog.html",
  styleUrl: "./location-dialog.scss",
})
export class LocationDialog implements OnInit {
  private readonly collectorService = inject(CollectorService);
  private readonly destroyRef = inject(DestroyRef);
  readonly collectors = signal<Collector[]>([]);
  readonly loadingCollectors = signal(true);
  readonly collectorsError = signal("");

  ngOnInit(): void {
    this.loadCollectors();
  }

  loadCollectors(): void {
    this.loadingCollectors.set(true);
    this.collectorsError.set("");
    this.collectorService
      .getCollectors()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (collectors) => {
          this.collectors.set(collectors);
          this.loadingCollectors.set(false);
        },
        error: () => {
          this.loadingCollectors.set(false);
          this.collectorsError.set("Não foi possível carregar os coletores.");
        },
      });
  }

  private readonly dialog = inject(MatDialogRef<LocationDialog>);
  private readonly toast = inject(MatSnackBar);
  readonly submitting = signal(false);
  images: File[] = [];

  collectorPointService = inject(CollectorPointService);
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: { lat: number; lng: number },
    private fb: FormBuilder,
  ) {
    this.createPointForm = this.fb.group({
      complement: [""],
    });
  }

  createPointForm!: FormGroup;

  selectedIds: string[] = [];

  toggleCollector(id: string) {
    const exists = this.selectedIds.includes(id);

    if (exists) {
      this.selectedIds = this.selectedIds.filter((x) => x !== id);
    } else {
      this.selectedIds.push(id);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    if (
      files.length > 10 ||
      files.some(
        (file) =>
          file.type !== "image/png" ||
          file.size === 0 ||
          file.size > 5 * 1024 * 1024,
      )
    ) {
      this.images = [];
      input.value = "";
      this.notify("Envie até dez imagens PNG de no máximo 5 MB cada.");
      return;
    }
    this.images = files;
  }

  private notify(message: string): void {
    this.toast.open(message, "Fechar", {
      duration: 6000,
      verticalPosition: "top",
      horizontalPosition: "center",
    });
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    if (this.images.length === 0 || this.selectedIds.length === 0) {
      this.notify("Selecione pelo menos uma imagem PNG e um coletor.");
      return;
    }
    this.submitting.set(true);
    this.dialog.disableClose = true;
    try {
      const point = await firstValueFrom(
        this.collectorPointService.createCollectorPoint(
          {
            lat: this.data.lat,
            lng: this.data.lng,
            collectorIds: this.selectedIds,
          },
          this.images,
        ),
      );
      this.notify("Ponto de coleta criado com sucesso!");
      this.dialog.close(point);
    } catch {
      this.notify("Não foi possível criar o ponto de coleta. Tente novamente.");
    } finally {
      this.submitting.set(false);
      this.dialog.disableClose = false;
    }
  }
}
