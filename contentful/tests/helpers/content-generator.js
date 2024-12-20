import { faker } from "@faker-js/faker";
import fs from "fs";
import { generateFieldValue, postProcessFieldValue }from "./content-generators/field-generators.js";
import { randomRange} from "./helpers.js";

const fetchContentTypes = () =>{
    const filePath = "./contentTypes.json.minimal.json";

    const contentTypes = JSON.parse(fs.readFileSync(filePath, "utf-8"));
    const contentTypeMap = new Map();

    const fields = new Map();

    for(const contentType of contentTypes){
        contentTypeMap.set(contentType.sys.id, contentType);
        for(const field of contentType.fields){
            fields.set(field.type, field);
        }
    }

    return {
        map: contentTypeMap,
        fields,
        contentTypes
    }
};

const generateContentForContentType = (contentType) => {
    const initial = {
        sys: {
            id: faker.string.uuid(),
            contentType: generateContentTypeSys(contentType.sys.id)
        },
        fields: {
            
        }
    };

    for(const field of contentType.fields){
        const value = generateFieldValue(field); 

        if(!value){
            continue;
        }

        initial.fields[field.id] = value;
    }

    return initial;
}

const generateContentTypeSys = (contentTypeId) => ({
    sys: {
        type: "Link",
        linkType: "ContentType",
        id: contentTypeId
    },
});

const getFieldInfoForContent =(contentTypes, content, id) => {
    const contentType = contentTypes.map.get(content.sys.contentType.sys.id);
    const fieldType = contentType.fields.find(field => field.id == id);
    return fieldType;
}


const postProcessGeneratedContent = (allContents, contentTypes, groupedContents) => {
    for (const content of allContents) {
        for (const [id, value] of Object.entries(content.fields)) {
            if (value === undefined || value['en-US'] == undefined) {
                continue;
            }

            const fieldType = getFieldInfoForContent(contentTypes, content, id);

            if (!fieldType) {
                console.error(`Couldn't find field info for ${id}`);
                continue;
            }

            postProcessFieldValue(value, fieldType, groupedContents);
        }
    }
}


const generateContent = (min, max) => {
    const contentTypes = fetchContentTypes();
    const allContents = [];
    const groupedContents = new Map();
    
    for(const contentType of contentTypes.contentTypes){
        const contentTypeContents = randomRange({ min, max, callback: () => generateContentForContentType(contentType)});

        allContents.push(...contentTypeContents);
        groupedContents.set(contentType.sys.id, contentTypeContents);
    }

    postProcessGeneratedContent(allContents, contentTypes, groupedContents);

    return {
        contentTypes: contentTypes.contentTypes,
        entries: allContents 
    }
}

export {
    generateContent
}