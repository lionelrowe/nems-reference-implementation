﻿import { inject } from 'aurelia-framework';
import { fetch } from 'whatwg-fetch';
import { HttpClient, json } from 'aurelia-fetch-client';
import { EventAggregator } from 'aurelia-event-aggregator';
//import { DialogRequested } from './helpers/EventMessages';

interface ApiResponse {
    Message: string;
    Success: boolean;
    Data: Object;
}

@inject(EventAggregator)
export class WebAPI {

    constructor(private ea: EventAggregator) { }

    httpClient = new HttpClient();
    apiResponse: ApiResponse;

    get isRequesting() {
        return this.httpClient.isRequesting;
    }

    getHeaders(headers?: { [key: string]: string }) {
        var requestHeaders = {
            'Accept': 'application/json',
            'X-Requested-With': 'Fetch'
        };

        if (headers) {
            for (var key in headers) {
                // skip loop if the property is from prototype
                if (!headers.hasOwnProperty(key)) continue;

                let val = headers[key];
                requestHeaders[key] = val;
            }
        }

        return requestHeaders;
    }

    do<T>(url, body, type, headers?, asText?) {
        this.httpClient.configure(config => {
            config
                .useStandardConfiguration()
                //Running on same host:port as API so base url can be left as is
                .withBaseUrl('/nems-ri/')
                .withDefaults({
                    credentials: 'same-origin',
                    headers: this.getHeaders(headers),
                    mode: 'same-origin'
                })
                .withInterceptor({
                    request(request) {
                        //let authHeader = fakeAuthService.getAuthHeaderValue(request.url);
                        //request.headers.append('Authorization', authHeader);
                        //console.log(`Requesting ${request.method} ${request.url}`);
                        return request;
                    },
                    response(response) {
                        //console.log(`Received ${response.status} ${response.url}`);
                        return response;
                    }
                });
        });


        let reqBody = (type === "get") ? {} : {
            method: type,
            body: json(body)
        };

        let that = this;
        return new Promise<T>(function (resolve, reject) {
            that.httpClient
                .fetch(url, reqBody)
                .then(response => {
                    //let resovable = (response.headers.get("Content-Type") || "").indexOf("xml") > -1 ? response.text() : response.json();

                    let resovable = asText && asText === true ? response.text() : response.json();
                    let rsv = resolve(resovable);
                    return rsv;
                }, error => {
                    try {
                        error.json().then(serverError => {
                            if (!error.ok && error.status === 404 && (!serverError || serverError.resourceType != "OperationOutcome")) {
                                //that.ea.publish(new DialogRequested({ details: error.statusText }));
                            } else {
                                //TODO: manage all issues
                                //that.ea.publish(new DialogRequested(serverError.issue[0] || { details: error.statusText }));
                            }
                        });
                    } catch (e) {
                        //that.ea.publish(new DialogRequested({ details: error.statusText }));
                    }
                });
        });
    }

}
