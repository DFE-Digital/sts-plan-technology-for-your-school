import RecommendationIntro from '../../../../export-processor/content-types/recommendation-intro';

describe('RecommendationIntro', () => {
  const sys = { id: 'introId' };
  const fields = {
    maturity: 'Mock Title',
    header: 'Mock Header',
    content: 'Mock Content',
    slug: 'mock-slug',
  };

  test('should create a RecommendationIntro with valid data', () => {
    const recommendationIntro = new RecommendationIntro({
      fields: fields,
      sys: sys,
    });

    expect(recommendationIntro.content).toEqual(fields.content);
    expect(recommendationIntro.header).toEqual(fields.header);
    expect(recommendationIntro.id).toEqual(sys.id);
    expect(recommendationIntro.content).toEqual(fields.content);
    expect(recommendationIntro.slug).toEqual(fields.slug);
  });
});
