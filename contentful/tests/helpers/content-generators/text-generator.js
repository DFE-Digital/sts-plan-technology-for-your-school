import { FieldGenerator } from "./field-generator.js";
import { faker } from "@faker-js/faker";

export class TextGenerator extends FieldGenerator {
    constructor() {
        super("Text");
    }

    concreteGenerateContent(field) {
        return faker.lorem.sentences({ min: 1, max: 3 });
    }
}
