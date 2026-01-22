import { SymbolGenerator } from './symbol-generator.js';
import { BooleanGenerator } from './boolean-generator.js';
import { LinkGenerator } from './link-generator.js';
import { RichTextGenerator } from './richtext-generator.js';
import { TextGenerator } from './text-generator.js';
import { ArrayGenerator } from './array-generator.js';

const generators = [
  new SymbolGenerator(),
  new BooleanGenerator(),
  new LinkGenerator(),
  new TextGenerator(),
  new RichTextGenerator(),
  new ArrayGenerator(),
];

const getGeneratorForField = (field) => {
  const matchingGenerator = generators.find((generator) => generator.acceptsField(field));

  if (!matchingGenerator) {
    console.error(`Could not find matching field value generator for field`, field);
    return;
  }

  return matchingGenerator;
};

const generateFieldValue = (field) => {
  const matchingGenerator = getGeneratorForField(field);

  if (!matchingGenerator) {
    return;
  }

  return matchingGenerator.generateMockFieldContent(field);
};

const postProcessFieldValue = (value, field, groupedContent) => {
  const matchingGenerator = getGeneratorForField(field);

  if (!matchingGenerator) {
    return;
  }

  return matchingGenerator.postProcess(value, field, groupedContent);
};

export { generateFieldValue, postProcessFieldValue };
