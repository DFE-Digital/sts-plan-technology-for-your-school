import { PathBuilder } from '../../../../export-processor/user-journey/path-builder.js';
import PathPart from '../../../../export-processor/user-journey/path-part.js';

describe('PathBuilder', () => {
  let mockQuestions;
  let pathBuilder;

  beforeEach(() => {
    mockQuestions = [
      {
        id: 'q1',
        text: 'Question 1',
        answers: [
          {
            id: 'a1',
            text: 'Answer 1 (to Q2)',
            nextQuestion: { sys: { id: 'q2' } },
          },
          {
            id: 'a2',
            text: 'Answer 2 (to Q3)',
            nextQuestion: { sys: { id: 'q3' } },
          },
        ],
      },
      {
        id: 'q2',
        text: 'Question 2',
        answers: [
          {
            id: 'a3',
            text: 'Answer 3 (end)',
            nextQuestion: null,
          },
        ],
      },
      {
        id: 'q3',
        text: 'Question 3',
        answers: [
          {
            id: 'a4',
            text: 'Answer 4 (end)',
            nextQuestion: null,
          },
        ],
      },
    ];

    pathBuilder = new PathBuilder(mockQuestions);
  });

  describe('getPathsForQuestion', () => {
    it('should generate all possible paths from a starting question', () => {
      const paths = pathBuilder.getPathsForQuestion(mockQuestions[0]);

      expect(paths).toHaveLength(2);

      expect(paths[0][0].question).toBe(mockQuestions[0]);
      expect(paths[1][0].question).toBe(mockQuestions[0]);

      for (const answer of mockQuestions[0].answers) {
        const matchingPath = paths.find((path) => path[0].answer.id == answer.id);

        expect(matchingPath).not.toBeNull();

        expect(matchingPath[1].question.id).toBe(answer.nextQuestion.sys.id);
      }
    });

    it('should handle a single question with no next questions', () => {
      const singleQuestion = [
        {
          id: 'q1',
          text: 'Single Question',
          answers: [
            {
              id: 'a1',
              text: 'Answer 1',
              nextQuestion: null,
            },
          ],
        },
      ];

      const builder = new PathBuilder(singleQuestion);
      const paths = builder.getPathsForQuestion(singleQuestion[0]);

      expect(paths).toHaveLength(1);
      expect(paths[0]).toHaveLength(1);
      expect(paths[0][0].question).toBe(singleQuestion[0]);
      expect(paths[0][0].answer).toBe(singleQuestion[0].answers[0]);
    });

    it('should handle a question with no answers', () => {
      const questionWithNoAnswers = [
        {
          id: 'q1',
          text: 'No Answers Question',
          answers: [],
        },
      ];

      const builder = new PathBuilder(questionWithNoAnswers);
      const paths = builder.getPathsForQuestion(questionWithNoAnswers[0]);

      expect(paths).toHaveLength(0);
    });
  });

  describe('assignBestAnswer', () => {
    it('should return first answer for last question', () => {
      const answers = [
        { id: 'a1', nextQuestion: { id: 'next' } },
        { id: 'a2', nextQuestion: null },
      ];

      const result = pathBuilder.assignBestAnswer(answers, mockQuestions.length - 1);
      expect(result).toBe(answers[0]);
    });

    it('should prioritize path-ending answers', () => {
      const answers = [
        { id: 'a1', nextQuestion: { id: 'next' } },
        { id: 'a2', nextQuestion: null },
        { id: 'a3', nextQuestion: { id: 'other' } },
      ];

      const result = pathBuilder.assignBestAnswer(answers, 0);
      expect(result).toBe(answers[1]);
    });

    it('should prioritize skipping answers over sequential ones', () => {
      const answers = [
        { id: 'a1', nextQuestion: { id: 'q2' } },
        { id: 'a2', nextQuestion: { id: 'q3' } },
      ];

      const result = pathBuilder.assignBestAnswer(answers, 0);
      expect(result).toBe(answers[1]);
    });

    it('should fall back to sequential answer if no better options exist', () => {
      const answers = [{ id: 'a1', nextQuestion: { id: 'q2' } }];

      const result = pathBuilder.assignBestAnswer(answers, 0);
      expect(result).toBe(answers[0]);
    });
  });

  describe('_getPathsForAnswer', () => {
    it('should create new path part and find next question', () => {
      const currentPath = [];
      const currentQuestion = mockQuestions[0];
      const answer = currentQuestion.answers[0];

      const result = pathBuilder._getPathsForAnswer(currentPath, currentQuestion, answer);

      expect(result.currentPath).toHaveLength(1);
      expect(result.currentPath[0]).toBeInstanceOf(PathPart);
      expect(result.currentPath[0].question).toBe(currentQuestion);
      expect(result.currentPath[0].answer).toBe(answer);
      expect(result.currentQuestion).toBe(mockQuestions[1]);
    });

    it('should handle answer with no next question', () => {
      const currentPath = [];
      const currentQuestion = mockQuestions[1];
      const answer = currentQuestion.answers[0];

      const result = pathBuilder._getPathsForAnswer(currentPath, currentQuestion, answer);

      expect(result.currentPath).toHaveLength(1);
      expect(result.currentQuestion).toBeUndefined();
    });
  });
});
