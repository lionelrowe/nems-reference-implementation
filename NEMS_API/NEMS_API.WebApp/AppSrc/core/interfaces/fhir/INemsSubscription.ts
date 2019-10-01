import { IContactPoint } from "./IContactPoint";
import { IBackboneElement } from "./IBackboneElement";

export interface INemsSubscription {
    status: string;
    contact: IContactPoint[];
    end?: string;
    reason: string;
    criteria: string;
    channel: IBackboneElement;
    id: string;
}