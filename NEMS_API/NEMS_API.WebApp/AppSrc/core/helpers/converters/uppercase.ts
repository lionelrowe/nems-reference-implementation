export class UppercaseValueConverter {
    toView(value?: string) {
        return value ? value.toLocaleUpperCase() : "";
    }
}

