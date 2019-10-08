import { observable, inject } from "aurelia-framework";
import { PatientSvc } from "../../core/services/PatientService";
import { IPatient } from "../../core/interfaces/IPatient";
import { IKeyValuePair } from "../../core/interfaces/IKeyValuePair";
import { ExampleSvc } from "../../core/services/ExampleService";
import { ListSvc } from "../../core/services/ListService";
import { SystemSvc } from "../../core/services/SystemService";
import { IExampleOptions } from "../../core/interfaces/IExampleOptions";
import { ISdsEntry } from "../../core/interfaces/ISdsEntry";
import { IRequest } from "../../core/interfaces/IRequest";
import { HttpRequest } from "../../core/models/HttpRequest";
import { PublishSvc } from "../../core/services/PublishService";
import { JwtSvc } from "../../core/services/JwtService";
import { IHttpResponse } from "../../core/interfaces/IHttpResponse";

@inject(PatientSvc, ExampleSvc, ListSvc, SystemSvc, JwtSvc, PublishSvc)
export class Publisher {

    heading: string = 'Publisher App';

    //patients: IPatient[];

    //@observable
    //selectedPatient?: IPatient;

    eventMessageTypes: IKeyValuePair[];

    @observable
    selectedEventMessageType?: IKeyValuePair;

    defaultPublisher?: ISdsEntry;

    spine?: ISdsEntry;

    publishRequest: IRequest = new HttpRequest({ method: "post" } as IRequest); 

    exampleMessageBody?: string;

    exampleMessageFormats: IKeyValuePair[];

    response?: IHttpResponse<any>;

    @observable
    selectedExampleMessageFormat: IKeyValuePair;
    

    constructor(private patientSvc: PatientSvc, private exampleSvc: ExampleSvc, private listSvc: ListSvc, private systemSvc: SystemSvc, private jwtSvc: JwtSvc, private publishSvc: PublishSvc) {

        //this.getPatients();

        this.getSpineDetails();

        this.getDefaultPublisher().then(() => {
            this.getEventTypeCodes();
        });

        this.getValidContentTypes();

        
    }


    //private get isEventDisabled(): boolean {
    //    return !this.selectedPatient;
    //}

    //private getPatients() {
    //    this.patientSvc.getPatients().then(patients => {
    //        this.patients = patients;
    //    });
    //}

    private setSelectedInteractionId() {

        if (this.defaultPublisher && this.defaultPublisher.interactions && this.selectedEventMessageType) {
            this.publishRequest.interactionId = this.defaultPublisher.interactions.find(ia => { return this.selectedEventMessageType ? ia.indexOf(this.selectedEventMessageType.key) > -1 : false; }) || "";
        }
         
    } 

    private getEventTypeCodes() {
       this.listSvc.getEventCodes().then(eventCode => {
            this.eventMessageTypes = eventCode;
        });
    }

    private getDefaultPublisher() : Promise<any> {
        return this.systemSvc.getDefaultPublisher().then(publisher => {
            this.defaultPublisher = publisher;   

            this.publishRequest.fromAsid = publisher.asid;
            this.publishRequest.endPoint = this.publishSvc.publishEndpoint;

            let odsCode = publisher.odsCode || "";

            this.jwtSvc.generate(odsCode, publisher.asid, "Bundle", "write").then(jwt => {

                this.publishRequest.jwt = jwt;
            });
        });
    }

    private getSpineDetails() {
        this.systemSvc.getSpine().then(spine => {
            this.spine = spine;
            this.publishRequest.toAsid = spine.asid;
        });
    }

    private getValidContentTypes() {
        this.listSvc.getValidContentTypes().then(contentTypes => {
            this.exampleMessageFormats = contentTypes;

            this.selectedExampleMessageFormat = this.exampleMessageFormats[0];

            this.publishRequest.contentType = this.selectedExampleMessageFormat.value;
        });
    }

    private sendEvent(): void {

        this.publishRequest.running = true;
        this.response = undefined;

        this.publishSvc.publishEvent(this.publishRequest).then(res => {

            if (typeof res.body == "object") {
                res.body = JSON.stringify(res.body, null, 2);
            }

            this.response = res;
            this.publishRequest.running = false;
        });
    }

    //private selectedPatientChanged(newValue: any, oldValue: string) {

    //    this.getExample();
    //}

    private selectedEventMessageTypeChanged(newValue: any, oldValue: string) {

        this.getExample();

        this.setSelectedInteractionId();
    }

    private selectedExampleMessageFormatChanged(newValue: IKeyValuePair, oldValue: IKeyValuePair) {

        if (newValue && (!oldValue || newValue.key === oldValue.key)) {
            return true;
        }

        this.selectedExampleMessageFormat = newValue;
        this.publishRequest.contentType = newValue.value;
        this.getExample();
    }

    private getExample() {

        if (/*!this.selectedPatient ||*/ !this.selectedEventMessageType) {
            return true;
        }

        this.response = undefined;
        this.publishRequest.body = this.exampleMessageBody = "";

        let options: IExampleOptions = {
            nhsNumber: (/*this.selectedPatient || */{ nhsNumber: undefined }).nhsNumber,
            eventMessageTypeId: (this.selectedEventMessageType || { key: undefined}).key
        };

        this.exampleSvc.generatePublish(options, this.selectedExampleMessageFormat.value).then(example => {

            this.publishRequest.body = this.exampleMessageBody = example;
        });
    }
}