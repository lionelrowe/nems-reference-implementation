import * as signalR from '@aspnet/signalr';
import { WebAPI } from './WebApi';
import { inject } from 'aurelia-framework';
import { IHttpRequest } from '../interfaces/IHttpRequest';
import { IInboxRecord } from '../interfaces/IInboxRecord';

@inject(WebAPI)
export class MessageExchangeSvc {

    connection?: signalR.HubConnection;

    baseUrl: string = 'STU3/Events/1';
    query: string = '';

    constructor(private api: WebAPI) {}

    init(workflowIds: string[], callback: () => void) : Promise<any> | undefined {
        
        if (!this.connection || this.connection.state != signalR.HubConnectionState.Connected) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/nems-ri/messageexchange/syncstatus")
                .configureLogging(signalR.LogLevel.None)
                .build();

            this.manageHandlers(workflowIds, "syncMailbox_", true, callback);

            return this.connection.start().then(() => console.log("started connection to mailbox sync")).catch(err => console.log(err));
        }

        return undefined;
    }

    end(workflowIds: string[]) {

        this.manageHandlers(workflowIds, "syncMailbox_", false, () => {

            if (!this.connection) {
                return;
            }

            this.connection.stop().then(() => console.log("stopped connection to mailbox sync"));
        });
        
    }

    checkMessage(mailboxId: string) {

        let message = this.api.do<IInboxRecord>({ url: `MessageExchange/${mailboxId}/inbox`, method: 'get' } as IHttpRequest);

        return message;
    }

    getMessage(mailboxId: string, messageId: string) {

        let message = this.api.do<any>({ url: `MessageExchange/${mailboxId}/inbox/${messageId}`, method: 'get' } as IHttpRequest);

        return message;
    }

    acknowledgeMessage(mailboxId: string, messageId: string) {

        let message = this.api.do<any>({ url: `MessageExchange/${mailboxId}/inbox/${messageId}/status/acknowledged`, method: 'put' } as IHttpRequest);

        return message;
    }

    private manageHandlers(handlerIds: string[], handlerPrefix: string, register: boolean, callback: () => void) : void {

        if (!this.connection) {
            return;
        }

        for (let i = 0, len = handlerIds.length; i < len; i++) {

            let handlerId = `${handlerPrefix}${handlerIds[i]}`;

            if (register) {
                console.log(`adding sync request for mailbox workflow ${handlerIds[i]}`);
                this.connection.on(handlerId, () => {

                    console.log(`sync request received for mailbox workflow ${handlerIds[i]}`);

                    if (typeof callback === "function") {
                        callback();
                    }
                });
            } else {
                console.log(`removing sync request for mailbox workflow ${handlerIds[i]}`);
                this.connection.off(handlerId);
            }
        }

        if (!register && typeof callback === "function") {
            callback();
        }
    }

}

