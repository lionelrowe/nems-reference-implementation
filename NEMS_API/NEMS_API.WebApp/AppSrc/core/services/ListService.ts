import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';

@inject(WebAPI)
export class ListSvc {

    baseUrl: string = 'AppUtilities/Lists';
    query: string = '';

    constructor(private api: WebAPI) { }

    /**
     * Requests a list of Event Codes from the backend service.
     * @returns A list of  Event Codes in the form of KeyValuePair.
     */
    getEventCodes() {
        let response = this.api.do<any>(`${this.baseUrl}/EventCodes${this.query}`, null, 'get');

        return response;
    }

    /**
 * Requests a list of ContentTypes from the backend service.
 * @returns A list of  valid ContentTypes in the form of KeyValuePair.
 */
    getValidContentTypes() {
        let response = this.api.do<any>(`${this.baseUrl}/ValidContentTypes${this.query}`, null, 'get');

        return response;
    }
}