import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IExampleOptions } from '../interfaces/IExampleOptions';
import { FhirSvc } from './FhirService';
import { IRequest } from '../interfaces/IRequest';
import { IHttpRequest } from '../interfaces/IHttpRequest';
import { IHttpResponse } from '../interfaces/IHttpResponse';
import { INemsSubscription } from '../interfaces/fhir/INemsSubscription';
import { IBundle } from '../interfaces/fhir/IBundle';

@inject(WebAPI, FhirSvc)
export class SubscriptionUtilsSvc {

    baseUrl: string = 'AppUtilities/Subscription';
    query: string = '';

    constructor(private api: WebAPI, private fhirSvc: FhirSvc) { }

    /**
     * Requests a subscribers subscritions based on their ASID.
     * @returns A FHIR Bundle containing subscriptions.
     */
    getSubscriptions(request: IRequest) {

        let headers = this.fhirSvc.getFhirRequestHeaders(request.contentType);

        let isText = this.fhirSvc.isFhirXml(request.contentType);

        let bundle = this.api.do<IBundle<INemsSubscription>>({ url: `${this.baseUrl}/${request.fromAsid}${this.query}`, method: request.method, headers: headers, asText: isText } as IHttpRequest);

        let subscriptions = bundle.then( b => { return this.fhirSvc.getBundleEntryResources<INemsSubscription>(b); });

        return subscriptions;
    }

    /**
     * Requests a MESH mailbox ids that would of recieved a message.
     * @returns An array of MESH mailbox ids.
     */
    getSubscribersOfMessage(messageId: string) {

        let mailboxIds = this.api.do<string[]>({ url: `${this.baseUrl}/MessageMatch/${messageId}${this.query}`, method: 'get' } as IHttpRequest);

        return mailboxIds;
    }
   

}