import { FieldGenerator } from "./field-generator.js";
import { faker } from "@faker-js/faker";

export class LinkGenerator extends FieldGenerator {
    constructor(fieldType) {
        super(fieldType ?? "Link");
    }

    concreteGenerateContent(field) {
        return this.generateEntryLink();
    }

    postProcess(value, field, groupedContents) { 
        const acceptedContentTypes = this.acceptedContentTypes(field);
        
        if(!acceptedContentTypes || acceptedContentTypes.length == 0){
            return;
        }

        this.setIdToAcceptedContent(value, groupedContents, acceptedContentTypes);
    }
    
    setIdToAcceptedContent(value, groupedContents, acceptedContentTypes){
        if(!value){
            console.error("Received no value");
            return;
        }

        const contentTypeToUse = faker.helpers.arrayElement(acceptedContentTypes);

        const contents = groupedContents.get(contentTypeToUse);

        if(!contents){
            console.error(`Could not find contents for content type "${contentTypeToUse}"`, acceptedContentTypes);
            console.error(groupedContents);
            throw "";
        }
        const content = faker.helpers.arrayElement(contents);

        const sys = value['en-US']?.sys ?? value.sys;
        sys.id = content.sys.id;
    }

    generateEntryLink(id = "") {
        return {
            sys: {
                type: "Link",
                linkType: "Entry",
                id: id,
            },
        };
    }

    acceptedContentTypes(field){
        return field.validations.filter(validation => !!validation.linkContentType)
                                .flatMap(validation => validation.linkContentType);
        
    }
}
