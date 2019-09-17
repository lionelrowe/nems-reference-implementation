export interface IHttpResponse<T> {
        
    body: T;
    statusCode: number;
    statusText: string;
    headers: { [key: string]: string };
}