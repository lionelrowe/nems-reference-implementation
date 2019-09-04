﻿import { Aurelia } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';
import { PLATFORM } from 'aurelia-pal';

export class App {
    router: Router;

    configureRouter(config: RouterConfiguration, router: Router) {
        config.title = 'NEMS Demonstrator Apps';
        config.map([
            { route: [''], name: 'home', moduleId: PLATFORM.moduleName('./pages/Home/index'), nav: true, title: 'Home' },
            { route: ['publisher'], name: 'publisher', moduleId: PLATFORM.moduleName('./pages/Publisher/index'), nav: true, title: 'Publisher' }
        ]);

        this.router = router;
    }
}
