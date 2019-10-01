import { IRequest } from "../interfaces/IRequest";

export class HttpRequest implements IRequest {

    fromAsid: string = "";
    toAsid: string = "";
    endPoint?: string;
    method: string = "";
    jwt: string = "";
    interactionId: string = "";
    body?: string;
    contentType: string = "";

    constructor(request?: IRequest) {

        if (request) {

            for (let key in request) {
                if (request.hasOwnProperty(key) && request[key]) {
                    this[key] = request[key];
                }
            }
        }
    }
}