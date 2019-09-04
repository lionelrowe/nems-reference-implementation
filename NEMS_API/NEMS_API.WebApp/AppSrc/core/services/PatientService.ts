import { WebAPI } from './WebApi';
import { bindable, inject } from 'aurelia-framework';
import { IPatient } from '../interfaces/IPatient';
import { IBundle } from '../interfaces/fhir/IBundle';
import { FhirSvc } from './FhirService';

@inject(WebAPI, FhirSvc)
export class PatientSvc {

    baseUrl: string = 'AppUtilities/Patients';
    query: string = '';

    constructor(private api: WebAPI, private fhirSvc: FhirSvc) { }

    /**
     * Requests a list of Patient Resources from the backend service.
     * @returns A list of FHIR Patients in the form of IPatient.
     */
    getPatients() {
        let headers = this.fhirSvc.getFhirRequestHeaders();

        let response = this.api.do<IBundle<IPatient>>(`${this.baseUrl}${this.query}`, null, 'get', headers);

        let patients: Promise<IPatient[]> = response.then(bundle => {
            let patientList = new Array<IPatient>();

            if (bundle != null && bundle.entry != null) {
                for (let i = 0, len = bundle.entry.length; i < len; i++) {
                    let patient: IPatient = bundle.entry[i].resource;

                    if (patient != null) {

                        patient.nhsNumber = this.fhirSvc.getPatientNhsNumber(patient);
                        patient.displayName = this.fhirSvc.getPatientDisplayName(patient);
                        patientList.push(patient);
                    }
                }
            }

            return patientList;
        });

        return patients;
    }

}