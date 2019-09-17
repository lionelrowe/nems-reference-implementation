export class KeysValueConverter {
    toView(obj) {
        if (obj !== null && typeof obj === 'object') {
            return Reflect.ownKeys(obj).filter(x => x !== '__observers__');
        } else {
            return null;
        }
    }
}