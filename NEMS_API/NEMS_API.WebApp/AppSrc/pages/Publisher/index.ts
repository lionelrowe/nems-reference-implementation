import { observable, inject } from "aurelia-framework";
import { PatientSvc } from "../../core/services/PatientService";
import { IPatient } from "../../core/interfaces/IPatient";
import { IKeyValuePair } from "../../core/interfaces/IKeyValuePair";
import { ExampleSvc } from "../../core/services/ExampleService";
import { ListSvc } from "../../core/services/ListService";
import { SystemSvc } from "../../core/services/SystemService";
import { IPublishExampleOptions } from "../../core/interfaces/IPublishExampleOptions";
import { ISdsEntry } from "../../core/interfaces/ISdsEntry";

@inject(PatientSvc, ExampleSvc, ListSvc, SystemSvc)
export class Publisher {

    heading: string = 'Publisher';

    patients: IPatient[];

    @observable
    selectedPatient?: IPatient;

    eventMessageTypes: IKeyValuePair[];

    @observable
    selectedEventMessageType?: IKeyValuePair;

    defaultPublisher?: ISdsEntry;

    publishMessage: any = {};

    publishMessagePreview: string;

    exampleMessageFormats: IKeyValuePair[];

    @observable
    selectedExampleMessageFormat: IKeyValuePair;
    

    constructor(private patientSvc: PatientSvc, private exampleSvc: ExampleSvc, private listSvc: ListSvc, private systemSvc: SystemSvc) {

        this.getPatients();

        this.getEventTypeCodes();

        this.getValidContentTypes();

        this.getDefaultPublisher();
    }


    private get isEventDisabled(): boolean {
        return !this.selectedPatient;
    }

    private getPatients() {
        this.patientSvc.getPatients().then(patients => {
            this.patients = patients;
        });
    }

    private getEventTypeCodes() {
        this.listSvc.getEventCodes().then(eventCode => {
            this.eventMessageTypes = eventCode;
        });
    }

    private getDefaultPublisher() {
        this.systemSvc.getDefaultPublisher().then(publisher => {
            this.defaultPublisher = publisher;
        });
    }

    private getValidContentTypes() {
        this.listSvc.getValidContentTypes().then(contentTypes => {
            this.exampleMessageFormats = contentTypes;

            this.selectedExampleMessageFormat = this.exampleMessageFormats[0];
        });
    }

    private sendEvent(): void {
        console.log(this.publishMessage);
    }

    private selectedPatientChanged(newValue: any, oldValue: string) {

        this.getExample();
    }

    private selectedEventMessageTypeChanged(newValue: any, oldValue: string) {

            this.getExample();
    }

    private selectedExampleMessageFormatChanged(newValue: IKeyValuePair, oldValue: IKeyValuePair) {

        if (newValue && (!oldValue || newValue.key === oldValue.key)) {
            return true;
        }

        this.selectedExampleMessageFormat = newValue;
        this.getExample();
    }

    private getExample() {

        if (!this.selectedPatient || !this.selectedEventMessageType) {
            return true;
        }

        let options: IPublishExampleOptions = {
            nhsNumber: (this.selectedPatient || { nhsNumber: undefined }).nhsNumber,
            eventMessageTypeId: (this.selectedEventMessageType || { key: undefined}).key
        };

        this.exampleSvc.generatePublish(options, this.selectedExampleMessageFormat.value).then(example => {
            this.publishMessage = this.publishMessagePreview = example;
        });
    }
}