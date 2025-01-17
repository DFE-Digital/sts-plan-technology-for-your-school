import { FieldGenerator } from "./field-generator.js";
import { faker } from "@faker-js/faker";

export class BooleanGenerator extends FieldGenerator {
    constructor() {
        super("Boolean");
    }

    concreteGenerateContent(field) {
        return faker.datatype.boolean();
    }
}
