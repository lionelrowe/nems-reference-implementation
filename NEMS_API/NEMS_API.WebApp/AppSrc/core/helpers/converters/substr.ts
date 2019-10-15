export class SubstrValueConverter {
    toView(value: string, start: number, len?: number) {
        return value ? value.substr(start, len) : "";
    }
}

