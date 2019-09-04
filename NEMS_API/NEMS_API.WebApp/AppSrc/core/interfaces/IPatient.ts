import { IIdentifier } from "./fhir/IIdentifier";
import { IResourceReference } from "./fhir/IResourceReference";
import { IAddress } from "./fhir/IAddress";
import { IContactPoint } from "./fhir/IContactPoint";
import { IName } from "./fhir/IName";
import { IMeta } from "./fhir/IMeta";

export interface IPatient {
    resourceType?: string;
    id?: string;
    identifier?: IIdentifier[];
    name?: IName[];
    telecom?: IContactPoint[];
    gender?: string;
    birthDate?: Date;
    deceased?: boolean;
    address?: IAddress[];
    managingOrganization?: IResourceReference;
    nhsNumber: string;
    meta?: IMeta;
    displayName: string;
}