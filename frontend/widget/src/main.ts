import { createApplication } from '@angular/platform-browser';
import { createCustomElement } from '@angular/elements';
import { ApplicationRef } from '@angular/core';
import { AppComponent } from './app/app.component';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

(async () => {
  const app: ApplicationRef = await createApplication({
    providers: [
      provideHttpClient(withFetch()),
      provideAnimations(),
    ],
  });

  const widgetElement = createCustomElement(AppComponent, { injector: app.injector });

  if (!customElements.get('customer-engagement-widget')) {
    customElements.define('customer-engagement-widget', widgetElement);
  }
})();
