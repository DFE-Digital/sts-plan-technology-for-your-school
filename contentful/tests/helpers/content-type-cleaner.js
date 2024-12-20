import fs from "fs";
const filePath = "./contentTypes.json"

const contentTypeCleaner = (filePath) => {

    const readContentTypes = () => {
        const fileContent = fs.readFileSync(filePath, "utf-8");

        return JSON.parse(fileContent);
    };

    const cleanContentTypes = (contentTypes) => {
        const cleaned = contentTypes.map(contentType => ({
            ...contentType,
            sys: {
                id: contentType.sys.id,
                type: "ContentType",
            }
        }));

        return cleaned;
    }

    const saveFile = (contentTypes) => {
        const stringified = JSON.stringify(contentTypes);
        
        fs.writeFileSync(`${filePath}.minimal.json`, stringified, { encoding: "utf-8"});
    }

    const contentTypes = readContentTypes();
    const cleaned = cleanContentTypes(contentTypes);
    saveFile(cleaned);
}

contentTypeCleaner(filePath);