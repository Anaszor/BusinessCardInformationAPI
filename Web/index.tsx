import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, withHashLocation } from '@angular/router';
import { provideZonelessChangeDetection, importProvidersFrom } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { AppComponent } from './src/app.component';
import { APP_ROUTES } from './src/app.routes';
import { CustomHttpInterceptor } from './src/interceptors/custom-http-interceptor.interceptor';

bootstrapApplication(AppComponent, {
    providers: [
        provideZonelessChangeDetection(),
        provideRouter(APP_ROUTES, withHashLocation()),
        provideHttpClient(
            withInterceptorsFromDi() 
        ),
        importProvidersFrom(CustomHttpInterceptor), 
    ],
}).catch(err => console.error(err));
