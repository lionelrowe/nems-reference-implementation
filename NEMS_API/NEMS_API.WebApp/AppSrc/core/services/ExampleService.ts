import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IPublishExampleOptions } from '../interfaces/IPublishExampleOptions';
import { FhirSvc } from './FhirService';
import { IHttpRequest } from '../interfaces/IHttpRequest';

@inject(WebAPI, FhirSvc)
export class ExampleSvc {

    baseUrl: string = 'AppUtilities/Example';
    query: string = '';

    constructor(private api: WebAPI, private fhirSvc: FhirSvc) { }

    /**
     * Requests a new example based on selected Patient and Event Message Type.
     * @returns A FHIR Bundle of type Message.
     */
    generatePublish(exampleOptions: IPublishExampleOptions, format: string) {

        let headers = this.fhirSvc.getFhirRequestHeaders(format);

        let query = this.buildQuery(exampleOptions);

        let example = this.api.do<any>({url: `${this.baseUrl}/Publish${query}`, method: 'get', headers: headers, asText: true } as IHttpRequest);

        return example.then(x => {
            if (this.fhirSvc.isFhirXml(format)) {
                return `<?xml version="1.0" encoding="UTF-8"?>\n${x}`;
            }
            return x;
        });
    }

    private buildQuery(exampleOptions: IPublishExampleOptions): string {

        this.query = "?";

        for (let key in exampleOptions) {

            if (exampleOptions.hasOwnProperty(key)) {
                let prefix = this.query === "?" ? "" : "&";
                this.query += `${prefix}${key}=${exampleOptions[key]}`;
            }
        }

        return this.query;
    }

}