import { dataLoaded, ValidateContent } from "../helpers/index.js";

export const validateNavigationLinks = (dataMapper) => {
    if (!dataLoaded(dataMapper)) {
        return;
    }

    for (const navigationLink of dataMapper.contents.get("navigationLink").values()) {
        ValidateContent(navigationLink);
    }
}