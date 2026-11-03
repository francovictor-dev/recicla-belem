import { DOCUMENT } from "@angular/common";
import { inject, Injectable } from "@angular/core";
import { environment } from "../../environments/environment";

export type GoogleCredential = { credential: string };
export interface GoogleIdentityApi {
  initialize(config: {
    client_id: string;
    auto_select: boolean;
    callback: (result: GoogleCredential) => void;
  }): void;
  renderButton(
    element: HTMLElement,
    config: {
      theme: string;
      size: string;
      shape: string;
      text: string;
      locale: string;
      click_listener: () => void;
    },
  ): void;
}

@Injectable({ providedIn: "root" })
export class GoogleAuthService {
  private readonly document = inject(DOCUMENT);
  private loading?: Promise<GoogleIdentityApi>;
  private initialized = false;
  private active?: (credential: string) => void;
  readonly configured = !!environment.googleClientId;

  private api(): GoogleIdentityApi | undefined {
    return (
      this.document.defaultView as
        | (Window & { google?: { accounts?: { id?: GoogleIdentityApi } } })
        | null
    )?.google?.accounts?.id;
  }

  private load(): Promise<GoogleIdentityApi> {
    const api = this.api();
    if (api) return Promise.resolve(api);
    if (this.loading) return this.loading;
    this.loading = new Promise((resolve, reject) => {
      const script = this.document.createElement("script");
      script.src = "https://accounts.google.com/gsi/client";
      script.async = true;
      const fail = () => {
        clearTimeout(timeout);
        script.remove();
        this.loading = undefined;
        reject(new Error("Não foi possível carregar o login Google."));
      };
      const timeout = setTimeout(fail, 15000);
      script.onerror = fail;
      script.onload = () => {
        clearTimeout(timeout);
        const loaded = this.api();
        if (loaded) resolve(loaded);
        else fail();
      };
      this.document.head.appendChild(script);
    });
    return this.loading;
  }

  async render(
    element: HTMLElement,
    callback: (credential: string) => void,
    isAlive: () => boolean,
  ): Promise<() => void> {
    if (!this.configured) throw new Error("Google Client ID não configurado.");
    const api = await this.load();
    if (!isAlive()) return () => {};
    if (!this.initialized) {
      api.initialize({
        client_id: environment.googleClientId,
        auto_select: false,
        callback: (response) => {
          const active = this.active;
          this.active = undefined;
          if (response.credential) {
            active?.(response.credential);
            console.log("Enviando token para fluxo de autenticação...");
          }
        },
      });
      this.initialized = true;
    }
    const guarded = (credential: string) => {
      if (isAlive()) callback(credential);
    };
    api.renderButton(element, {
      theme: "outline",
      size: "large",
      shape: "pill",
      text: "continue_with",
      locale: "pt-BR",
      click_listener: () => {
        this.active = guarded;
      },
    });
    return () => {
      if (this.active === guarded) this.active = undefined;
      element.replaceChildren();
    };
  }
}
