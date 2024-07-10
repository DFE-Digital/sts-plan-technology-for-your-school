const ReplaceNonBreakingHyphen = (text) => text.replace("&#8209;", "‑");

const ReplaceWhiteSpace = (string) =>
  string
    .replace(/\s/g, " ")
    .replace(/&nbsp;/g, " ")
    .replace(/\s/g, " ")
    .replace(" ", " ")
    .replace(" ", " ")
    .replace(" ", " ")
    .replace("  ", " ");

const CleanText = (text) =>
  ReplaceNonBreakingHyphen(ReplaceWhiteSpace(text)).trim();

export { ReplaceNonBreakingHyphen, ReplaceWhiteSpace, CleanText };
