const ARRAY_TYPE = "Array";
const LINK_TYPE = "Link";

class ContentType {
  id;
  fields;

  constructor({ sys, fields }) {
    this.id = sys.id;
    this.fields = {};

    for (const field of fields) {
      this.fields[field.id] = field;
    }
  }

  getReferencedTypesForField(field) {
    const matching = this.fields[field];

    if (!matching) throw `Couldn't find ${field} in ${this.fields}`;

    if (matching.type !== ARRAY_TYPE && matching.type !== LINK_TYPE) return;

    const validations =
      matching.type == ARRAY_TYPE
        ? matching.items.validations
        : matching.validations;

    if (!validations) return false;

    const linkContentType = validations
      .filter((validation) => validation.linkContentType)
      .map((validation) => validation.linkContentType)[0];

    return linkContentType.length == 0 ? undefined : linkContentType;
  }
}

export default ContentType;
