import { FieldGenerator } from "./field-generator.js";
import { faker } from "@faker-js/faker";

export class SymbolGenerator extends FieldGenerator {
    constructor() {
        super("Symbol");
    }

    getOptions(field) {
        if (!field.validations?.length) {
            return;
        }

        const options = field.validations.find((validation) => !!validation.in);

        if (!options) {
            return;
        }

        return options.in;
    }

    concreteGenerateContent(field) {
        const options = this.getOptions(field);

        return !options
            ? faker.lorem.words({ min: 1, max: 10 })
            : faker.helpers.arrayElement(options);
    }
}
