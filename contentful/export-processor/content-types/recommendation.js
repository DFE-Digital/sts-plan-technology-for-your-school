export class Recommendation {
    slug;
    maturity;
    displayName;
    id;
    page;

    constructor({ fields, sys }) {
        this.slug = fields.slug;
        this.maturity = fields.maturity;
        this.displayName = fields.displayName;
        this.id = sys.id;
        this.page = fields.page;

        if (!fields.page?.fields?.slug) {
            console.error(this);
            return;
        }
        this.slug = fields.page?.fields.slug;
    }
}
