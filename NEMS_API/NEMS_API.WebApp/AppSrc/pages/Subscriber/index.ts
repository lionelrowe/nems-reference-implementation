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

@inject(ExampleSvc, ListSvc, SystemSvc, JwtSvc, SubscribeSvc, SubscriptionUtilsSvc)
export class Subscriber {

    heading: string = 'Subscriber';

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

    subscriberLayout: any = {
        subscriptionsListClass: "col-xs-12",
        showSubscription: false,
        activeSubscriptionClass: {},
        activeSubscription: {},
        creating: false,
        loading: true
    };
    

    constructor(private exampleSvc: ExampleSvc, private listSvc: ListSvc, private systemSvc: SystemSvc, private jwtSvc: JwtSvc, private subscribeSvc: SubscribeSvc, private subscriptionUtilsSvc: SubscriptionUtilsSvc) {

        this.getSpineDetails();

        this.getSubscribers();

        this.getValidContentTypes();

    }

    private showSubscription(sub: INemsSubscription) {

        this.subscriberLayout.activeSubscription = sub;
        this.subscriberLayout.showSubscription = true;
        this.subscriberLayout.subscriptionsListClass = "col-xs-4 sub-minimal";
        this.subscriberLayout.activeSubscriptionClass = {};
        this.subscriberLayout.activeSubscriptionClass[sub.id] = "warning";
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

            this.setSelectedSubscriber(subscribers[0]);

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
            return true;
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