import { PLATFORM } from "aurelia-framework";

export function configure(config) {
    config.globalResources([
        PLATFORM.moduleName('./spinner-msg.html')
    ]);
}