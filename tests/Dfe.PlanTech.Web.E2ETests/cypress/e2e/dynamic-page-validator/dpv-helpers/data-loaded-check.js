/**
 * Check if data has been loaded and log a message if not.
 *
 * @param {Array | Map} contentMap - the map of content
 * @return {boolean} haveContent - whether data has been loaded
 */

export const dataLoaded = (contentMap) => {
    const haveContent = contentMap?.size > 0 || contentMap?.length > 0;

    if (!haveContent) {
        console.log("Data has not been loaded");
    }

    return haveContent;
}