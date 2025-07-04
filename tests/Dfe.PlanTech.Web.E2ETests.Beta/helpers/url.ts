export function textToHyphenatedUrl(url: string): string {
  return url
    .trim()
    .replace(/\s+/g, '-')            
    .replace(/[*'"`’‘”“]/g, '')     
    .replace(/[^a-zA-Z0-9\-]/g, '') 
    .toLowerCase();
}
