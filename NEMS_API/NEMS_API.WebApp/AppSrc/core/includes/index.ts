import { PLATFORM } from "aurelia-framework";

export function configure(config) {
    config.globalResources([
        PLATFORM.moduleName('./nav-bar.html')
    ]);
}