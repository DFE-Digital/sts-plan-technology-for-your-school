import { stringify as flattenedStringify } from "flatted";

const stringify = (obj) => {
    try {
        return JSON.stringify(obj);
    } catch (e) {
        console.error(`Error stringifying ${obj}. Will try flatted`, e);
        return flattenedStringify(obj);
    }
};

export { stringify };
