import { Answer } from '../../../../export-processor/content-types/answer';

describe('Answer Class', () => {
  describe('Constructor', () => {
    it('should initialize properties correctly', () => {
      const fields = {
        maturity: 'Low',
        text: 'This is a test answer.',
        nextQuestion: 'next-question-id',
      };
      const sys = { id: 'answer-id' };

      const answer = new Answer({ fields, sys });

      expect(answer.id).toBe(sys.id);
      expect(answer.text).toBe(fields.text);
      expect(answer.maturity).toBe(fields.maturity);
      expect(answer.nextQuestion).toBe(fields.nextQuestion);
    });
  });
});
