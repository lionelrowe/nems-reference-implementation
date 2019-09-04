import { IExtension } from "./IExtension";
import { IAssigner } from "./IAssigner";
import { IPeriod } from "./IPeriod";

export interface IIdentifier {
    use?: string;
    type?: string;
    system: string;
    value: string;
    period?: IPeriod;
    assigner?: IAssigner;
    elementId?: string;
    id: string;
    extension: IExtension[];
}