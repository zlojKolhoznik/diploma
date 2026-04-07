import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideTablerIcons } from '@tabler/icons-angular';
import {
  IconAlertCircle,
  IconCheck,
  IconAlertTriangle,
  IconInfoCircle,
  IconHome,
  IconUser,
  IconEye,
  IconEyeOff,
  IconHelpCircle,
  IconChevronRight,
  IconX,
} from '@tabler/icons-angular';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideTablerIcons({
      IconAlertCircle,
      IconCheck,
      IconAlertTriangle,
      IconInfoCircle,
      IconHome,
      IconUser,
      IconEye,
      IconEyeOff,
      IconHelpCircle,
      IconChevronRight,
      IconX,
    }),
  ]
};
