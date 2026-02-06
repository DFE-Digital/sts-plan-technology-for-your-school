import { FieldGenerator } from './field-generator.js';
import { faker } from '@faker-js/faker';

export class RichTextGenerator extends FieldGenerator {
  constructor() {
    super('RichText');
  }

  concreteGenerateContent(field) {
    return {
      nodeType: 'document',
      data: {},
      content: this.generateRandomContent(),
    };
  }

  generateRandomContent() {
    // Generate 1-4 random content blocks
    const numBlocks = faker.number.int({ min: 1, max: 4 });
    return Array.from({ length: numBlocks }, () => {
      const contentType = faker.helpers.arrayElement([
        'paragraph',
        'unordered-list',
        'ordered-list',
        'table',
      ]);

      switch (contentType) {
        case 'paragraph':
          return this.generateParagraph();
        case 'unordered-list':
          return this.generateList('unordered-list');
        case 'ordered-list':
          return this.generateList('ordered-list');
        case 'table':
          return this.generateTable();
        default:
          return this.generateParagraph();
      }
    });
  }

  generateParagraph() {
    return {
      nodeType: 'paragraph',
      data: {},
      content: this.generateParagraphContent(),
    };
  }

  generateParagraphContent() {
    // Generate 1-3 content elements for the paragraph
    const numElements = faker.number.int({ min: 1, max: 3 });
    return Array.from({ length: numElements }, () => {
      // 20% chance of generating a hyperlink
      return faker.datatype.boolean(0.2) ? this.generateHyperlink() : this.generateText();
    });
  }

  generateText() {
    return {
      nodeType: 'text',
      value: faker.lorem.sentence(),
      marks: this.generateTextMarks(),
      data: {},
    };
  }

  generateTextMarks() {
    // 30% chance of having marks
    if (faker.datatype.boolean(0.3)) {
      return Array.from({ length: faker.number.int({ min: 1, max: 2 }) }, () => ({
        type: faker.helpers.arrayElement(['bold', 'italic', 'underline']),
      }));
    }
    return [];
  }

  generateHyperlink() {
    return {
      nodeType: 'hyperlink',
      data: {
        uri: faker.internet.url(),
      },
      content: [
        {
          nodeType: 'text',
          value: faker.lorem.words({ min: 1, max: 4 }),
          marks: [],
          data: {},
        },
      ],
    };
  }

  generateList(listType) {
    return {
      nodeType: listType,
      data: {},
      content: Array.from({ length: faker.number.int({ min: 2, max: 5 }) }, () => ({
        nodeType: 'list-item',
        data: {},
        content: [this.generateParagraph()],
      })),
    };
  }

  generateTable() {
    const rows = faker.number.int({ min: 2, max: 4 });
    const cols = faker.number.int({ min: 2, max: 3 });

    return {
      nodeType: 'table',
      data: {},
      content: [
        // Header row
        this.generateTableRow(cols, true),
        // Content rows
        ...Array.from({ length: rows - 1 }, () => this.generateTableRow(cols, false)),
      ],
    };
  }

  generateTableRow(cols, isHeader) {
    return {
      nodeType: 'table-row',
      data: {},
      content: Array.from({ length: cols }, () => ({
        nodeType: isHeader ? 'table-header-cell' : 'table-cell',
        data: {},
        content: [this.generateParagraph()],
      })),
    };
  }
}
