import ValidateContent from "../../../helpers/content-validators/content-validator.js";
import { dataLoaded } from "../dpv-helpers/data-loaded-check.js";

export const validateNavigationLinks = (dataMapper) => {
    if (!dataMapper) {
        return;
    }

    const navigationLinks = Array.from(dataMapper.contents.get("navigationLink").entries());

    if (!dataLoaded(navigationLinks)) {
        return;
    }

    for (const [_, navigationLink] of navigationLinks) {
        ValidateContent(navigationLink);
    }
}