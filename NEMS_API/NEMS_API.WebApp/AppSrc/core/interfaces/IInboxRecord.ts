export interface IInboxRecord {
    messageID: string;
    errorEvent?: string;
    errorCode?: number;
    errorDescription?: string;
}