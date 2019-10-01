import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IHttpRequest } from '../interfaces/IHttpRequest';

@inject(WebAPI)
export class JwtSvc {

    baseUrl: string = 'AppUtilities/RequestHelper';
    query: string = '?';

    constructor(private api: WebAPI) { }

    /**
     * Requests a new jwt based on fromASID and OdsCode.
     * @returns A jwt as an encoded string.
     */
    generate(odsCode: string, asid: string, scopeResource: string, scopeAction: string) {

        let query = `${this.query}odsCode=${odsCode}&asid=${asid}&scopeResource=${scopeResource}&scopeAction=${scopeAction}`;

        let jwt = this.api.do<any>({ url: `${this.baseUrl}/GenerateJwtToken${query}`, method: 'get', asText: true } as IHttpRequest);

        return jwt.then(jwt => { return jwt.replace(/"/g, ""); });
    }

}