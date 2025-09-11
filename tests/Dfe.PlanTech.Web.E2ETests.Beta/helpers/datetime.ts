export function getCurrentShortDate():string {
    const today = new Date();
    const options: Intl.DateTimeFormatOptions = { day: 'numeric', month: 'short', year: 'numeric' };
    const formattedDate = today.toLocaleDateString('en-GB', options).replace(',', '');
    const normalisedDate = normaliseMonth(formattedDate);
    return normalisedDate;
}

function normaliseMonth(text: string): string {
  return text.replace(/\bSept\b/, "Sep");
}

//Normalises the Sept -> Sep text we have on the pages.
export const normaliseShortDateTimeText = (s: string) =>
  s.replace(/\bSept\b/g, "Sep").replace(/\s+/g, " ").trim();
