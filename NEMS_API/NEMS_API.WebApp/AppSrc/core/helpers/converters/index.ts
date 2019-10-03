import { PLATFORM } from "aurelia-framework";

export function configure(config) {
    config.globalResources([
        PLATFORM.moduleName('./keys'),
        PLATFORM.moduleName('./uppercase')
    ]);
}