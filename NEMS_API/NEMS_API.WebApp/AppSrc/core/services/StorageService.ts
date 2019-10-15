export class StorageSvc {

    store: Storage;

    constructor() {

        this.store = window.localStorage;
    }

    public getCache(key: string) {

        let dataStore = this.store.getItem(key);

        if (!dataStore) {
            return null;
        }

        return JSON.parse(dataStore);
    }

    public setCache(key: string, data: any) {

        let dataStore = JSON.stringify(data);

        return this.store.setItem(key, dataStore);
    }

    public removeCache(key: string) {

        return this.store.removeItem(key);
    }
}