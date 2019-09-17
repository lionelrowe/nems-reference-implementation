export interface IRequest {
        
    fromAsid: string;
    toAsid: string;
    endPoint?: string;
    method: string;
    jwt: string;
    interactionId: string;
    body?: string;
    contentType: string;
}