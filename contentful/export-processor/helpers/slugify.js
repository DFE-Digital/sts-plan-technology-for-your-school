const matchNonAlphanumericExceptHyphensRegex = /[^a-zA-Z0-9-]/g;
const matchWhitespaceCharactersRegex = /\s/g;

/**
 * Slugify text. Should match the implementation in /src/Dfe.PlanTech.Domain/Extensions/StringHelpers.cs
 * @param {*} text
 */
const slugify = (text) =>
    text
        .trim()
        .replace(matchWhitespaceCharactersRegex, "-")
        .replace(matchNonAlphanumericExceptHyphensRegex, "")
        .toLowerCase();

export { slugify };
