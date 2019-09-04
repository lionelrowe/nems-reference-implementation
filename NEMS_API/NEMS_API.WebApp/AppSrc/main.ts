import { Aurelia, PLATFORM } from 'aurelia-framework';

import 'font-awesome/css/font-awesome.css';
import 'bootstrap/dist/css/bootstrap.css';
import '../AppStyles/main.scss';
import * as Bluebird from 'bluebird';

Bluebird.config({ warnings: { wForgottenReturn: false } });

export async function configure(au: Aurelia) {
    au.use.standardConfiguration()
          .developmentLogging();

    await au.start().then(a => {
        a.setRoot(PLATFORM.moduleName('app'));
    });
}