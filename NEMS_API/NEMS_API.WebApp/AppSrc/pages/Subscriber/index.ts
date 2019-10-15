import { observable, inject } from "aurelia-framework";
import { IKeyValuePair } from "../../core/interfaces/IKeyValuePair";
import { ExampleSvc } from "../../core/services/ExampleService";
import { ListSvc } from "../../core/services/ListService";
import { SystemSvc } from "../../core/services/SystemService";
import { IExampleOptions } from "../../core/interfaces/IExampleOptions";
import { ISdsEntry } from "../../core/interfaces/ISdsEntry";
import { IRequest } from "../../core/interfaces/IRequest";
import { HttpRequest } from "../../core/models/HttpRequest";
import { SubscribeSvc } from "../../core/services/SubscribeService";
import { JwtSvc } from "../../core/services/JwtService";
import { IHttpResponse } from "../../core/interfaces/IHttpResponse";
import { SubscriptionUtilsSvc } from "../../core/services/SubscriptionUtilsService";
import { INemsSubscription } from "../../core/interfaces/fhir/INemsSubscription";
import { MessageExchangeSvc } from "../../core/services/MessageExchangeService";
import { IInboxRecord } from "../../core/interfaces/IInboxRecord";
import { MailboxSvc } from "../../core/services/MailboxService";
import { IMailboxEntry } from "../../core/interfaces/IMailbox";

@inject(ExampleSvc, ListSvc, SystemSvc, JwtSvc, SubscribeSvc, SubscriptionUtilsSvc, MessageExchangeSvc, MailboxSvc)
export class Subscriber {

    heading: string = 'Subscriber App';

    @observable
    selectedSubscriber: ISdsEntry;

    subscribers: ISdsEntry[];
    loadingSubscribers: boolean = true;

    spine?: ISdsEntry;

    subscribeRequest: IRequest = new HttpRequest({ method: "post" } as IRequest); 

    exampleMessageBody?: string;

    exampleMessageFormats: IKeyValuePair[];

    subscriptions: INemsSubscription[];

    response?: IHttpResponse<any>;

    @observable
    selectedExampleMessageFormat: IKeyValuePair;

    mailboxEntries: any[] = [];
    checkingMailbox: boolean = false;
    promptMailbox: boolean = false;
    mailboxInitCheck: any;

    subscriberLayout: any = {
        subscriptionsListClass: "col-xs-12",
        showSubscription: false,
        activeSubscriptionClass: {},
        activeSubscription: {},
        subscriptionViewActive: false,
        creating: false,
        loading: true
    };

    eventLayout: any = {
        eventListClass: "col-xs-12",
        showEvent: false,
        activeEventClass: {},
        activeEvent: {},
        loading: true
    };
    

    constructor(private exampleSvc: ExampleSvc, private listSvc: ListSvc, private systemSvc: SystemSvc, private jwtSvc: JwtSvc, private subscribeSvc: SubscribeSvc,
        private subscriptionUtilsSvc: SubscriptionUtilsSvc, private messageExchangeSvc: MessageExchangeSvc, private mailboxSvc: MailboxSvc) {

        this.getSpineDetails();

        this.getSubscribers();

        this.getValidContentTypes();
    }

    attached() {
        $('a[data-toggle="tab"]').on('shown.bs.tab', (e) => {

            let target: HTMLAnchorElement = e.target as HTMLAnchorElement;
            
            this.subscriberLayout.subscriptionViewActive = (target && target.href.indexOf("#mySubscriptions") > -1);

            if (!this.subscriberLayout.subscriptionViewActive) {
                this.checkMailbox();
            }

        });

    }

    detached() {
        $('a[data-toggle="tab"]').off('shown.bs.tab');

        this.subscriberLayout.subscriptionViewActive = false;

        this.stopMailbox(this.selectedSubscriber.workflowIds);
    }

    private initMailboxCheck() {
        let initRes = this.messageExchangeSvc.init(this.selectedSubscriber.workflowIds, () => {

            this.promptMailbox = true;
            this.checkMailbox();

            clearTimeout(this.mailboxInitCheck);
        });

        if (typeof initRes === "undefined") {
            this.mailboxInitCheck = setTimeout(() => { this.initMailboxCheck(); }, 500);
        }
    }

    private checkMailbox() {

        if (this.checkingMailbox || this.subscriberLayout.subscriptionViewActive) {
            console.log("Skipped mailbox check");
            return;
        }

        this.checkingMailbox = true;

        this.messageExchangeSvc.checkMessage(this.selectedSubscriber.meshMailboxId).then(record => {

               this.nextFromMailbox(record);
        });
    }

    private nextFromMailbox(record: IInboxRecord) {

        if (!record) {
            console.log("No messages to pull");
            this.checkingMailbox = false;
            this.promptMailbox = false;
            return;
        }

        console.log(`Request message ${record.messageID}`);

        this.messageExchangeSvc.getMessage(this.selectedSubscriber.meshMailboxId, record.messageID).then(message => {

            let messageBody = JSON.stringify(message, null, 2);
            let messgeHeader = (message.entry as any[]).find(elm => { return elm.resource.resourceType === "MessageHeader"; });

            let entry = {
                mailboxId: this.selectedSubscriber.meshMailboxId,
                messageId: record.messageID,
                type: messgeHeader.resource.event.display,
                message: messageBody
            } as IMailboxEntry;

            this.mailboxEntries = this.mailboxSvc.addMessage(entry);        

            this.messageExchangeSvc.acknowledgeMessage(this.selectedSubscriber.meshMailboxId, record.messageID).then(() => {
                this.checkingMailbox = false;
                this.checkMailbox();
            });
        });
    }

    private stopMailbox(workflowIds: string[]) {
        this.messageExchangeSvc.end(workflowIds);
    }

    private showEvent(event: any) {

        this.eventLayout.activeEvent = event;
        this.eventLayout.showEvent = true;
        this.eventLayout.eventListClass = "col-xs-4 sub-minimal";
        this.eventLayout.activeEventClass = {};
        this.eventLayout.activeEventClass[event.messageId] = "warning active";
    }

    private hideEvent() {

        this.eventLayout.showEvent = false;
        this.eventLayout.eventListClass = "col-xs-12";
        this.eventLayout.activeEvent = {};
        this.eventLayout.activeEventClass = {};
    }

    private removeEvent(eventId: string) {

        this.mailboxEntries = this.mailboxSvc.removeMessage({ mailboxId: this.selectedSubscriber.meshMailboxId, messageId: eventId } as IMailboxEntry);

        this.hideEvent();
    }

    private showSubscription(sub: INemsSubscription) {

        this.subscriberLayout.activeSubscription = sub;
        this.subscriberLayout.showSubscription = true;
        this.subscriberLayout.subscriptionsListClass = "col-xs-4 sub-minimal";
        this.subscriberLayout.activeSubscriptionClass = {};
        this.subscriberLayout.activeSubscriptionClass[sub.id] = "warning active";
        this.subscriberLayout.creating = false;

        this.convertExample();

        this.prepDeleteEvent(); 
    }


    public showNewSubscription() {

        this.subscriberLayout.showSubscription = true;
        this.subscriberLayout.subscriptionsListClass = "col-xs-4 sub-minimal";
        this.subscriberLayout.activeSubscriptionClass = {};
        this.subscriberLayout.activeSubscription = {};
        this.subscriberLayout.creating = true;

        this.prepCreateEvent();

        this.getExample();
    }



    private hideSubscription() {

        this.subscriberLayout.showSubscription = false;
        this.subscriberLayout.subscriptionsListClass = "col-xs-12";
        this.subscriberLayout.activeSubscription = {};
        this.subscriberLayout.activeSubscriptionClass = {};
        this.subscriberLayout.creating = false;
    }

    private getSubscribers() {
        this.systemSvc.getSubscribers().then(subscribers => {

            this.subscribers = subscribers;   

            this.loadingSubscribers = false;
        });
    }

    private getSpineDetails() {
        this.systemSvc.getSpine().then(spine => {
            this.spine = spine;
            this.subscribeRequest.toAsid = spine.asid;
        });
    }

    private getValidContentTypes() {
        this.listSvc.getValidContentTypes().then(contentTypes => {
            this.exampleMessageFormats = contentTypes;

            this.selectedExampleMessageFormat = this.exampleMessageFormats[0];

            this.subscribeRequest.contentType = this.selectedExampleMessageFormat.value;
        });
    }

    private getInteractionId(key: string, ids?: string[]) {

        return (ids || new Array<string>()).find(ia => { return ia.indexOf(key) > -1; }) || "";
    }

    public prepCreateEvent() {

        this.subscribeRequest.method = "post";
        this.subscribeRequest.interactionId = this.getInteractionId("ApiPost", this.selectedSubscriber.interactions);
        this.subscribeRequest.endPoint = this.subscribeSvc.subscribeEndpoint;

    }

    private prepDeleteEvent() {
        this.subscribeRequest.method = "delete";
        this.subscribeRequest.interactionId = this.getInteractionId("ApiDelete", this.selectedSubscriber.interactions);
        this.subscribeRequest.endPoint = `${this.subscribeSvc.subscribeEndpoint}/${this.subscriberLayout.activeSubscription.id}`;
    }

    private send(): void {

        this.subscribeRequest.running = true;
        this.response = undefined;

        this.subscribeSvc.sendCommand(this.subscribeRequest).then(res => {

            if (typeof res.body == "object") {
                res.body = JSON.stringify(res.body, null, 2);
            }

            this.subscribeRequest.running = false;

            this.response = res;

            if ([200, 201].indexOf(this.response.statusCode) > -1) {

                this.getSubscriptions();
            }
        });
    }

    private setSelectedSubscriber(sub: ISdsEntry) {

        this.selectedSubscriber = sub;

        this.subscribeRequest.fromAsid = this.selectedSubscriber.asid;

        $('#subscriberTabs a[href="#mySubscriptions"]').tab('show');
        this.subscriberLayout.subscriptionViewActive = true;

        this.mailboxEntries = this.mailboxSvc.getMailbox({ mailboxId: this.selectedSubscriber.meshMailboxId } as IMailboxEntry);

        this.promptMailbox = false;
        this.checkingMailbox = false;

        this.initMailboxCheck();

        this.getSubscriptions();

        let odsCode = this.selectedSubscriber.odsCode || "";

        this.jwtSvc.generate(odsCode, this.selectedSubscriber.asid, "Subscription", "write").then(jwt => {

            this.subscribeRequest.jwt = jwt;
        });
    }


    private selectedExampleMessageFormatChanged(newValue: IKeyValuePair, oldValue: IKeyValuePair) {

        if (newValue && (!oldValue || newValue.key === oldValue.key)) {
            return true;
        }

        this.selectedExampleMessageFormat = newValue;
        this.subscribeRequest.contentType = newValue.value;

        if (this.subscriberLayout.creating) {
            this.getExample();
        } else {
            this.convertExample();
        }
    }

    private selectedSubscriberChanged(newValue: ISdsEntry, oldValue: ISdsEntry) {
        
        if (!newValue || (oldValue && newValue.odsCode === oldValue.odsCode)) {

            if (oldValue) {
                this.stopMailbox(oldValue.workflowIds);
            }

            return true;
        }

        if (oldValue) {
            this.stopMailbox(oldValue.workflowIds);
        }

        this.hideSubscription();
        this.setSelectedSubscriber(newValue);
    }

    private getSubscriptions() {

        this.subscriberLayout.loading = true;

        let request = new HttpRequest({
                contentType: "application/fhir+json",
                fromAsid: this.selectedSubscriber.asid,
                method: "get"
            } as IRequest);

        this.subscriptionUtilsSvc.getSubscriptions(request).then(subscriptions => {

            this.subscriberLayout.loading = false;
            this.subscriptions = subscriptions;
        });
    }

    private getExample() {

        this.response = undefined;
        this.subscribeRequest.body = this.exampleMessageBody = "";

        this.exampleSvc.generateSubscribe(this.subscriberLayout.activeSubscription.criteria, this.selectedSubscriber.asid, this.selectedExampleMessageFormat.value).then(example => {
            this.subscribeRequest.body = this.exampleMessageBody = example;
        });
    }

    private convertExample() {

        this.response = undefined;
        this.subscribeRequest.body = this.exampleMessageBody = "";

        this.exampleSvc.convert(this.subscriberLayout.activeSubscription, this.selectedExampleMessageFormat.value).then(example => {
            this.subscribeRequest.body = this.exampleMessageBody = example;
        });
    }
}