import { inject } from 'aurelia-framework';
import { fetch } from 'whatwg-fetch';
import { HttpClient, json } from 'aurelia-fetch-client';
import { EventAggregator } from 'aurelia-event-aggregator';
import { IHttpRequest } from '../interfaces/IHttpRequest';
import { IHttpResponse } from '../interfaces/IHttpResponse';
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

    do<T>(request: IHttpRequest) {
        this.httpClient.configure(config => {
            config
                .useStandardConfiguration()
                //Running on same host:port as API so base url can be left as is
                .withBaseUrl('/nems-ri/')
                .withDefaults({
                    credentials: 'same-origin',
                    headers: this.getHeaders(request.headers),
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

        let reqBody = (request.method === "get") ? {} : {
            method: request.method,
            body: (request.asText === true) ? request.body : json(request.body)
        };

        let that = this;
        return new Promise<T>(function (resolve, reject) {
            that.httpClient
                .fetch(request.url, reqBody)
                .then(response => {

                    let resovable = (request.asText === true ? response.text() : response.json()).then(res => {

                        if (request.returnResponse === true) {
                            return that.buildHttpResponse<T>(response, res);
                        }

                        return res;
                    });

                    resolve(resovable);

                },
                error => {

                    let resovable = (request.asText === true ? error.text() : error.json()).then(res => {

                        if (request.returnResponse === true) {
                            return that.buildHttpResponse<T>(error, res);
                        }

                        return res;
                    });

                    if (request.returnResponse === true) {
                        resolve(resovable);
                    }
                    else {
                        //TODO: dialog
                        reject(error);
                    }
                });
        });
    }

    private buildHttpResponse<T>(response: Response, body: any): IHttpResponse<T> {

        let responseHeaders: { [key: string]: string } = {};

        response.headers.forEach((value, key) => {
            responseHeaders[key] = value;
        });

        let httpResponse = { statusCode: response.status, statusText: response.statusText, body: body, headers: responseHeaders } as IHttpResponse<T>;

        return httpResponse;
    }

}
