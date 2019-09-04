import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { ISdsEntry } from '../interfaces/ISdsEntry';

@inject(WebAPI)
export class SystemSvc {

    baseUrl: string = 'AppUtilities/Systems';
    query: string = '';

    constructor(private api: WebAPI) { }

    /**
     * Requests the default publisher system details from the backend service.
     * @returns A publisher system details.
     */
    getDefaultPublisher() {
        let response = this.api.do<ISdsEntry>(`${this.baseUrl}/DefaultPublisher${this.query}`, null, 'get');

        return response;
    }
}