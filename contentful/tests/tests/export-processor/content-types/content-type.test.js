import ContentType from '../../../../export-processor/content-types/content-type';

describe('ContentType', () => {
  let contentType;
  let mockSys;
  beforeEach(() => {
    mockSys = { id: 'contentType1' };
    const mockFields = [
      { id: 'field1', type: 'Text' },
      {
        id: 'field2',
        type: 'Array',
        items: { validations: [{ linkContentType: ['contentType2'] }] },
      },
      {
        id: 'field3',
        type: 'Link',
        validations: [{ linkContentType: ['contentType3'] }],
      },
      { id: 'field4', type: 'Number' },
    ];

    contentType = new ContentType({ sys: mockSys, fields: mockFields });
  });

  test('should initialize with correct id and fields', () => {
    expect(contentType.id).toBe('contentType1');
    expect(Object.keys(contentType.fields)).toHaveLength(4);
    expect(contentType.fields.field1.type).toBe('Text');
    expect(contentType.fields.field2.type).toBe('Array');
    expect(contentType.fields.field3.type).toBe('Link');
  });

  test('should return undefined for non-referenced field', () => {
    const linkedTypes = contentType.getReferencedTypesForField('field1');
    expect(linkedTypes).toBeUndefined();
  });

  test('should return referenced types for Array field', () => {
    const linkedTypes = contentType.getReferencedTypesForField('field2');
    expect(linkedTypes).toEqual(['contentType2']);
  });

  test('should return referenced types for Link field', () => {
    const linkedTypes = contentType.getReferencedTypesForField('field3');
    expect(linkedTypes).toEqual(['contentType3']);
  });

  test('should throw error if field does not exist', () => {
    expect(() => contentType.getReferencedTypesForField('nonExistentField')).toThrow();
  });

  test('should return false for non-referenced Array field with no validations', () => {
    const mockFieldsWithoutValidation = [{ id: 'field1', type: 'Array', items: {} }];
    const contentTypeWithoutValidation = new ContentType({
      sys: mockSys,
      fields: mockFieldsWithoutValidation,
    });

    const linkedTypes = contentTypeWithoutValidation.getReferencedTypesForField('field1');
    expect(linkedTypes).toBe(false);
  });

  test('should return false for non-referenced Link field with no validations', () => {
    const mockFieldsWithoutValidation = [{ id: 'field1', type: 'Link' }];
    const contentTypeWithoutValidation = new ContentType({
      sys: mockSys,
      fields: mockFieldsWithoutValidation,
    });

    const linkedTypes = contentTypeWithoutValidation.getReferencedTypesForField('field1');
    expect(linkedTypes).toBe(false);
  });
});
