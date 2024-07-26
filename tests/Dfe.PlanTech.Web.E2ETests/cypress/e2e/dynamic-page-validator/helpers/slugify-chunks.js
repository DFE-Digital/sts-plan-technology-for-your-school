export const slugifyChunks = (text) => {
    const removeNonAlphanumericCharactersRegexPattern = /[^a-zA-Z0-9\s]/g

    return text.replace(removeNonAlphanumericCharactersRegexPattern, "").trim().replace(/ /g, "-").toLowerCase();
}