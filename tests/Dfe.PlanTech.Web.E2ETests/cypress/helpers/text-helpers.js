const ReplaceNonBreakingHyphen = (text) => text.replace('&#8209;', '‑');

const ReplaceWhiteSpace = (string) =>
  string.replace(/&nbsp;/g, ' ').replace(/(\s|\u00A0|\u202f|\u0020|\u2007)+/g, ' ');

const CleanText = (text) => ReplaceNonBreakingHyphen(ReplaceWhiteSpace(text)).trim();

export { ReplaceNonBreakingHyphen, ReplaceWhiteSpace, CleanText };
