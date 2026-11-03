import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { environment } from "../../environments/environment";
import { AuthStore } from "../store/user.store";
import { User } from "../types/user.type";

@Injectable({ providedIn: "root" })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly baseUrl = `${environment.apiUrl}/api`;

  login(data: { email: string; password: string }) {
    return this.http.post<{ accessToken: string }>(
      `${this.baseUrl}/Auth/login`,
      data,
    );
  }

  register(
    data: { name: string; email: string; password: string },
    image: File | null,
  ) {
    const body = new FormData();
    body.set("User.Email", data.email);
    body.set("User.Password", data.password);
    body.set("Profile.Name", data.name.trim());
    if (image) body.set("Image", image);

    return this.http.post(`${this.baseUrl}/User/default`, body);
  }

  async signIn(data: { email: string; password: string }): Promise<void> {
    const response = await firstValueFrom(this.login(data));
    await this.completeSignIn(response.accessToken);
  }

  async signInWithGoogle(idToken: string): Promise<void> {
    const response = await firstValueFrom(this.http.post<{ accessToken: string }>(
      `${this.baseUrl}/Auth/google`, { idToken },
    ));
    await this.completeSignIn(response.accessToken);
  }

  private async completeSignIn(accessToken: string): Promise<void> {
    localStorage.setItem("token", accessToken);
    try {
      this.store.setUser(await firstValueFrom(this.me()));
      this.store.setAuth(true);
    } catch (error) {
      this.logout();
      throw error;
    }
  }

  logout(): void {
    localStorage.removeItem("token");
    this.store.clearAll();
  }

  me() {
    return this.http.get<User>(`${this.baseUrl}/Auth/me`);
  }
}
