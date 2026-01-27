export const slugifyChunks = (text) => {
  const removeNonAlphanumericExceptHyphensRegexPattern = /[^a-zA-Z0-9\s-]/g;

  return text
    .replace(removeNonAlphanumericExceptHyphensRegexPattern, '')
    .trim()
    .replace(/ /g, '-')
    .toLowerCase();
};
