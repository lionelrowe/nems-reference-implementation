import { IEntry } from "./IEntry";

export interface IBundle<T> {
    resourceType: string; 
    type: string; 
    entry: IEntry<T>[];
}