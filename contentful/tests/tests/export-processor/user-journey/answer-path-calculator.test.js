import { AnswerPathCalculator } from '../../../../export-processor/user-journey/answer-path-calculator.js';
import { PathBuilder } from '../../../../export-processor/user-journey/path-builder.js';

import { jest } from '@jest/globals';

describe('AnswerPathCalculator', () => {
  let mockQuestions;
  let mockPathBuilder;
  let calculator;

  beforeEach(() => {
    mockQuestions = [
      {
        id: 'q1',
        answerIds: ['a1', 'a2'],
        answers: [
          {
            id: 'a1',
            text: 'Answer 1',
            nextQuestion: { id: 'q2' },
          },
          {
            id: 'a2',
            text: 'Answer 2',
            nextQuestion: { id: 'q3' },
          },
        ],
      },
      {
        id: 'q2',
        answerIds: ['a3'],
        answers: [
          {
            id: 'a3',
            text: 'Answer 3',
            nextQuestion: null,
          },
        ],
      },
      {
        id: 'q3',
        answerIds: ['a4'],
        answers: [
          {
            id: 'a4',
            text: 'Answer 4',
            nextQuestion: null,
          },
        ],
      },
    ];

    mockPathBuilder = new PathBuilder(mockQuestions);
    calculator = new AnswerPathCalculator(mockQuestions, mockPathBuilder);
  });

  describe('getPathsForAllAnswers', () => {
    it('should generate paths to cover all unused answers', () => {
      const minimumPaths = {
        rec1: [
          {
            question: { id: 'q1' },
            answer: {
              id: 'a1',
              nextQuestion: { id: 'q2', answers: [] },
            },
          },
        ],
      };

      const paths = calculator.getPathsForAllAnswers(minimumPaths);

      expect(paths.length).toBeGreaterThan(0);

      const usedAnswerIds = new Set([
        'a1',
        ...paths.flatMap((path) => path.map((part) => part.answer.id)),
      ]);

      expect(usedAnswerIds.size).toBe(4);
    });

    it('should return empty array when all answers are already used', () => {
      const minimumPaths = {
        rec1: [{ answer: { id: 'a1' } }],
        rec2: [{ answer: { id: 'a2' } }],
        rec3: [{ answer: { id: 'a3' } }],
        rec4: [{ answer: { id: 'a4' } }],
      };

      const paths = calculator.getPathsForAllAnswers(minimumPaths);
      expect(paths).toHaveLength(0);
    });

    it('should break and return paths when no questions with unused answers are found', () => {
      const spy = jest
        .spyOn(calculator, '_findLastQuestionWithUnusedAnswers')
        .mockImplementationOnce(() => -1);

      const minimumPaths = {
        rec1: [{ answer: { id: 'a1' } }],
      };

      const paths = calculator.getPathsForAllAnswers(minimumPaths);

      expect(paths).toEqual([]);
      expect(spy).toHaveBeenCalled();

      spy.mockRestore();
    });
  });

  describe('assignBestAnswer', () => {
    it('should return first answer for last question', () => {
      const answers = [
        { id: 'a1', nextQuestion: { sys: { id: 'next' } } },
        { id: 'a2', nextQuestion: null },
      ];

      const result = calculator.assignBestAnswer(answers, mockQuestions.length - 1);
      expect(result).toBe(answers[0]);
    });

    it('should prioritize path-ending answers', () => {
      const answers = [
        { id: 'a1', nextQuestion: { sys: { id: 'next' } } },
        { id: 'a2', nextQuestion: null },
        { id: 'a3', nextQuestion: { sys: { id: 'other' } } },
      ];

      const result = calculator.assignBestAnswer(answers, 0);
      expect(result).toBe(answers[1]);
    });
  });

  describe('_getAnswerIdsFromQuestions', () => {
    it('should return all answer IDs from questions', () => {
      const answerIds = calculator._getAnswerIdsFromQuestions(mockQuestions);
      expect(answerIds).toEqual(['a1', 'a2', 'a3', 'a4']);
    });
  });

  describe('_getUsedAnswerIds', () => {
    it('should extract used answer IDs from minimum paths', () => {
      const minimumPaths = {
        rec1: [{ question: { id: 'test' }, answer: { id: 'a1' } }],
        rec2: [{ question: { id: 'test2' }, answer: { id: 'a2' } }],
      };

      const usedIds = calculator._getUsedAnswerIds(minimumPaths);
      expect(usedIds).toEqual(['a1', 'a2']);
    });
  });

  describe('_allAnswersUsed', () => {
    it('should return true when all answers are used', () => {
      const allAnswers = ['a1', 'a2'];
      const usedAnswers = ['a1', 'a2'];

      const result = calculator._allAnswersUsed(allAnswers, usedAnswers);
      expect(result).toBe(true);
    });

    it('should return false when some answers are unused', () => {
      const allAnswers = ['a1', 'a2', 'a3'];
      const usedAnswers = ['a1', 'a2'];

      const result = calculator._allAnswersUsed(allAnswers, usedAnswers);
      expect(result).toBe(false);
    });
  });

  describe('_findLastQuestionWithUnusedAnswers', () => {
    it('should find last question with unused answers', () => {
      const usedAnswers = ['a1', 'a2', 'a3'];

      const index = calculator._findLastQuestionWithUnusedAnswers(usedAnswers);
      expect(index).toBe(2);
    });

    it('should return -1 when all answers are used', () => {
      const usedAnswers = ['a1', 'a2', 'a3', 'a4'];

      const index = calculator._findLastQuestionWithUnusedAnswers(usedAnswers);
      expect(index).toBe(-1);
    });
  });

  describe('_buildPathFromIndex', () => {
    it('should build complete path from start index and skip usedAnswers', () => {
      const startIndex = 0;
      const usedAnswers = ['a2'];

      const path = calculator._buildPathFromIndex(startIndex, usedAnswers);

      expect(path.length).toBe(2);
      expect(path[0].question.id).toBe('q1');
      expect(path[0].answer.id).toBe('a1');

      expect(path[1].question.id).toBe('q2');
      expect(path[1].answer.id).toBe('a3');
    });

    it('should build complete path from start index and skip usedAnswers', () => {
      const startIndex = 0;
      const usedAnswers = ['a1'];

      const path = calculator._buildPathFromIndex(startIndex, usedAnswers);

      expect(path.length).toBe(2);
      expect(path[0].question.id).toBe('q1');
      expect(path[0].answer.id).toBe('a2');

      expect(path[1].question.id).toBe('q3');
      expect(path[1].answer.id).toBe('a4');
    });

    it('should build complete path from another index', () => {
      const startIndex = 1;
      const usedAnswers = ['a2'];

      const path = calculator._buildPathFromIndex(startIndex, usedAnswers);

      expect(path.length).toBeGreaterThan(0);
      expect(path[0].question.id).toBe('q1');

      expect(path[1].question.id).toBe('q2');
      expect(path[1].answer.id).toBe('a3');
    });
  });

  describe('_updateUsedAnswers', () => {
    it('should add new answer IDs to used answers set', () => {
      const path = [{ answer: { id: 'a3' } }, { answer: { id: 'a4' } }];
      const usedAnswers = ['a1', 'a2'];

      calculator._updateUsedAnswers(path, usedAnswers);

      expect(usedAnswers).toContain('a3');
      expect(usedAnswers).toContain('a4');
    });

    it('should not duplicate existing answer IDs', () => {
      const path = [{ answer: { id: 'a1' } }, { answer: { id: 'a2' } }];
      const usedAnswers = ['a1', 'a2'];

      calculator._updateUsedAnswers(path, usedAnswers);

      expect(usedAnswers).toEqual(['a1', 'a2']);
    });
  });

  describe('_findBestPreviousAnswer', () => {
    it('should find unused answer that leads to needed question', () => {
      const question = mockQuestions[0];
      const nextNeeded = 'q2';
      const usedAnswers = ['a2'];

      const result = calculator._findBestPreviousAnswer(question, nextNeeded, usedAnswers);
      expect(result.id).toBe('a1');
    });

    it('should fall back to used answer if no unused answers available', () => {
      const question = mockQuestions[0];
      const nextNeeded = 'q2';
      const usedAnswers = ['a1', 'a2'];

      const result = calculator._findBestPreviousAnswer(question, nextNeeded, usedAnswers);
      expect(result.id).toBe('a1');
    });
  });
});
