import { faker } from "@faker-js/faker";

export class FieldGenerator {
    fieldType;

    constructor(fieldType) {
        this.fieldType = fieldType;
    }

    acceptsField(field) {
        return this.fieldType == field.type;
    }

    localiseValue(field) {
        return {
            "en-US": field,
        };
    }

    generateMockFieldContent(field) {
        if (!field.required && faker.datatype.boolean(0.9)) {
            return;
        }

        return this.localiseValue(this.concreteGenerateContent(field));
    }

    concreteGenerateContent(field) {
        console.error(
            `Concrete implementation for field ${this.fieldType} has not been implemented!`
        );
    }

    postProcess(value, field, allContent) {
        //Optional callback to execute on content once all content generated
        //Used for link adding
    }
}