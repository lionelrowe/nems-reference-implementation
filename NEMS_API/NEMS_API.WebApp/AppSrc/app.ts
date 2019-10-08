import { Aurelia } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';
import { PLATFORM } from 'aurelia-pal';

export class App {
    router: Router;

    configureRouter(config: RouterConfiguration, router: Router) {
        config.title = 'NEMS (National Events Management Service) Demonstrator Apps';
        config.map([
            { route: [''], name: 'home', moduleId: PLATFORM.moduleName('./pages/Home/index'), nav: true, title: 'Home' },
            { route: ['publisher'], name: 'publisher', moduleId: PLATFORM.moduleName('./pages/Publisher/index'), nav: true, title: 'Publisher App' },
            { route: ['subscriber'], name: 'subscriber', moduleId: PLATFORM.moduleName('./pages/Subscriber/index'), nav: true, title: 'Subscriber App' }

        ]);

        this.router = router;
    }
}
