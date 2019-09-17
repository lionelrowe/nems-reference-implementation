import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IPublishExampleOptions } from '../interfaces/IPublishExampleOptions';
import { FhirSvc } from './FhirService';
import { IRequest } from '../interfaces/IRequest';
import { IHttpRequest } from '../interfaces/IHttpRequest';
import { IHttpResponse } from '../interfaces/IHttpResponse';

@inject(WebAPI, FhirSvc)
export class PublishSvc {

    baseUrl: string = 'STU3/Events/1';
    query: string = '';

    constructor(private api: WebAPI, private fhirSvc: FhirSvc) { }

    /**
     * Requests a new example based on selected Patient and Event Message Type.
     * @returns A FHIR Bundle of type Message.
     */
    publishEvent(request: IRequest) {

        let headers = this.fhirSvc.getFhirRequestHeaders(request.contentType);

        headers.fromASID = request.fromAsid;
        headers.toASID = request.toAsid;
        headers.InteractionID = request.interactionId;
        headers.Authorization = `Bearer ${request.jwt}`;

        let isText = this.fhirSvc.isFhirXml(request.contentType);

        let event = this.api.do<IHttpResponse<any>>({ url: `${this.baseUrl}/$process-message${this.query}`, body: request.body, method: request.method, headers: headers, asText: isText, returnResponse: true } as IHttpRequest);

        return event;
    }

   

}