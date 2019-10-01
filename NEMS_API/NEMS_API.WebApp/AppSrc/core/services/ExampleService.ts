import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IExampleOptions } from '../interfaces/IExampleOptions';
import { FhirSvc } from './FhirService';
import { IHttpRequest } from '../interfaces/IHttpRequest';
import { IEndPoints } from '../interfaces/IEndPoints';

@inject(WebAPI, FhirSvc)
export class ExampleSvc {

    baseUrl: string = 'AppUtilities/Example';
    query: string = '';
    endPoints: IEndPoints = { publish: "Publish", subscribe: "Subscribe" };

    constructor(private api: WebAPI, private fhirSvc: FhirSvc) { }

    /**
     * Requests a new example based on selected Patient and Event Message Type.
     * @returns A FHIR Bundle of type Message.
     */
    generatePublish(exampleOptions: IExampleOptions, format: string) {

        return this.generateExample(exampleOptions, format, this.endPoints.publish);
    }

        /**
     * Requests a new example subscription based on selected Patient.
     * @returns A FHIR Subscription of type Message.
     */
    generateSubscribe(criteria: string, format: string) {

        let exampleOptions: IExampleOptions = {
            nhsNumber: this.fhirSvc.getCriteriaNhsNumber(criteria)
        };

        return this.generateExample(exampleOptions, format, this.endPoints.subscribe);
    }

    private generateExample(options: IExampleOptions, format: string, endPoint: string) {

        let headers = this.fhirSvc.getFhirRequestHeaders(format);

        let query = this.buildQuery(options);

        let example = this.api.do<any>({ url: `${this.baseUrl}/${endPoint}${query}`, method: 'get', headers: headers, asText: true } as IHttpRequest);

        return example.then(x => {
            if (this.fhirSvc.isFhirXml(format)) {
                return `<?xml version="1.0" encoding="UTF-8"?>\n${x}`;
            }
            return x;
        });
    }

    private buildQuery(exampleOptions: IExampleOptions): string {

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