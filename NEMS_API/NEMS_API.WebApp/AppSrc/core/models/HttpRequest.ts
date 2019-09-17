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
            if (request.method) {
                this.method = request.method;
            }
        }
    }
}