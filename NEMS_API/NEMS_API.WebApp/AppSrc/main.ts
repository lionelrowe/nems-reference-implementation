import { Aurelia, PLATFORM } from 'aurelia-framework';

import 'font-awesome/css/font-awesome.css';
import 'bootstrap/dist/css/bootstrap.css';
import '../AppStyles/main.scss';
import 'bootstrap/dist/js/bootstrap.min';
import * as Bluebird from 'bluebird';

Bluebird.config({ warnings: { wForgottenReturn: false } });

export async function configure(au: Aurelia) {
    au.use.standardConfiguration()
        .developmentLogging()
        .plugin(PLATFORM.moduleName('core/helpers/loaders'))
        .plugin(PLATFORM.moduleName('core/helpers/converters'))
        .plugin(PLATFORM.moduleName('core/includes'));

    await au.start().then(a => {
        a.setRoot(PLATFORM.moduleName('app'));
    });
}