export interface IHttpRequest {
        
    url: string;
    body: any;
    method: string;
    headers?: { [key: string]: string };
    asText?: boolean;
    returnResponse?: boolean;

}