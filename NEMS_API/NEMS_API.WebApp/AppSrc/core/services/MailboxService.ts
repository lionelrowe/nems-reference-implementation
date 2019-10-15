import { inject } from "aurelia-framework";
import { StorageSvc } from "./StorageService";
import { IMailboxEntry } from "../interfaces/IMailbox";


@inject(StorageSvc)
export class MailboxSvc {

    //TODO: make static key

    constructor(private cache: StorageSvc) {}

    public getMailbox(actionConfig: IMailboxEntry): IMailboxEntry[] {

        return this.cache.getCache(actionConfig.mailboxId) || [];
    }

    public addMessage(actionConfig: IMailboxEntry): IMailboxEntry[] {

        let mailbox = this.getMailbox(actionConfig);

        mailbox.push(actionConfig);

        this.cache.setCache(actionConfig.mailboxId, mailbox);

        return this.getMailbox(actionConfig);
    }

    public removeMessage(actionConfig: IMailboxEntry): IMailboxEntry[] {

        let mailbox = this.getMailbox(actionConfig).filter(message => { return message.messageId !== actionConfig.messageId; });

        this.cache.setCache(actionConfig.mailboxId, mailbox);

        return this.getMailbox(actionConfig);
    }

    public emptyMailbox(actionConfig: IMailboxEntry) {

        this.cache.removeCache(actionConfig.mailboxId);
    }
}