import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { ISdsEntry } from '../interfaces/ISdsEntry';
import { IHttpRequest } from '../interfaces/IHttpRequest';

@inject(WebAPI)
export class SystemSvc {

    baseUrl: string = 'AppUtilities/RequestHelper';
    query: string = '';

    constructor(private api: WebAPI) { }

    /**
     * Requests the default publisher system details from the backend service.
     * @returns A publisher system details.
     */
    getDefaultPublisher() {
        let response = this.api.do<ISdsEntry>({ url: `${this.baseUrl}/GetDefaultPublisherSystem${this.query}`, method: 'get' } as IHttpRequest);

        return response;
    }

    getSubscribers() {
        let response = this.api.do<ISdsEntry[]>({ url: `${this.baseUrl}/GetSubscriberSystems${this.query}`, method: 'get' } as IHttpRequest);

        return response;
    }

    getSpine() {
        let response = this.api.do<ISdsEntry>({ url: `${this.baseUrl}/GetSpineSystem${this.query}`, method: 'get'} as IHttpRequest);

        return response;
    }
}