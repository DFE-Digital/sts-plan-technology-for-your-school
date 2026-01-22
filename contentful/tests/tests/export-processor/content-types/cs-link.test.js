import CSLink from '../../../../export-processor/content-types/cs-link';

describe('CSLink Class', () => {
  describe('Constructor', () => {
    it('should initialize properties correctly', () => {
      const fields = {
        url: 'http://plan-technology.dfe.com',
        linkText: 'Plan Technology For Your School',
      };

      const sys = { id: 'answer-id' };

      const csLink = new CSLink({ fields, sys });

      expect(csLink.id).toBe(sys.id);
      expect(csLink.url).toBe(fields.url);
      expect(csLink.linkText).toBe(fields.linkText);
    });
  });
});
