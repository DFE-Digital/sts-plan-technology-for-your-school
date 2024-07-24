import ValidateContent from "../../../helpers/content-validators/content-validator.js";
import { dataLoaded } from "../dpv-helpers/data-loaded-check.js";

export const validateNavigationLinks = (dataMapper) => {
    if (!dataLoaded(dataMapper.contents)) {
        return;
    }

    const navigationLinks = Array.from(dataMapper.contents.get("navigationLink").entries());

    for (const [_, navigationLink] of navigationLinks) {
        ValidateContent(navigationLink);
    }
}