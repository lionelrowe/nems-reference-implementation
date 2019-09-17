import { IPatient } from "../interfaces/IPatient";
import { IName } from "../interfaces/fhir/IName";
import { IIdentifier } from "../interfaces/fhir/IIdentifier";

export class FhirSvc {

    public getPatientDisplayName(patient: IPatient): string {
        let officialName = $.grep(patient.name || new Array<IName>(), (element: IName, index) => {
            return element.use === "official";
        });

        if (officialName === null || officialName.length < 1) {
            return "NAME NOT STORED";
        }

        return `${(officialName[0].family || "").toUpperCase()}, ${(officialName[0].given.join(" "))}`;
    }

    public getPatientNhsNumber(patient: IPatient): string {

        let nhsNumber = $.grep(patient.identifier || new Array<IIdentifier>(), (element: IIdentifier, index) => {
            return element.system === "https://fhir.nhs.uk/Id/nhs-number";
        });

        if (nhsNumber && nhsNumber.length > 0) {
            return nhsNumber[0].value;
        }

        return "NHS NUMBER NOT STORED";
    }

    public getFhirRequestHeaders(altFormat?: string): { [key: string]: string }
    {
        let contentType = altFormat || "application/fhir+json";

        return { "Accept": contentType, "Content-Type": contentType };
    }

    public isFhirXml(format: string): boolean {

        return ["application/fhir+xml", "application/xml+fhir"].indexOf(format) > -1;
    }
}