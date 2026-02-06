import { jest } from '@jest/globals';
import PathPart from '../../../../export-processor/user-journey/path-part';

/**
 *
 * @param {PathPart[]} path
 * @returns {{path: PathPart[], toMinimalOutput: () => { }}}
 */
const createMockUserJourney = (path) => ({
  path: path,
  toMinimalOutput: () => pathToMinimalOutput(path),
});

/**
 *
 * @param {{ path: PathPart[] }} path
 * @returns
 */
const pathToMinimalOutput = (path) => ({
  path: JSON.stringify(path),
});

const pathsToMinimalOutput = (paths) => paths.map((p) => pathToMinimalOutput(p.path));

const minimumPathsToNavigateQuestions = [
  [new PathPart({ question: { text: 'minPathNavQ1' }, answer: { text: 'minPathNavA1' } })],
  [new PathPart({ question: { text: 'minPathNavQ2' }, answer: { text: 'minPathNavA2' } })],
  [new PathPart({ question: { text: 'minPathNavQ3' }, answer: { text: 'minPathNavA3' } })],
];

const minimumPathsForRecommendations = {
  Low: [new PathPart({ question: { text: 'recPathQLow' }, answer: { text: 'recPathALow' } })],
  Medium: [
    new PathPart({ question: { text: 'recPathQMedium' }, answer: { text: 'recPathAMedium' } }),
  ],
  High: [new PathPart({ question: { text: 'recPathQHigh' }, answer: { text: 'recPathAHigh' } })],
};

const pathInfo = {
  paths: [
    createMockUserJourney([
      {
        question: { id: 'question-id', text: 'question text' },
        answer: { id: 'answer-id', text: 'answer text' },
      },
    ]),
  ],
  minimumPathsToNavigateQuestions,
  minimumPathsForRecommendations,
  pathsForAllPossibleAnswers: [
    createMockUserJourney([
      {
        question: { id: 'question1' },
        answer: { id: 'answer1' },
      },
    ]),
  ],
};

const stats = { low: 1, medium: 2, high: 3 };

jest.unstable_mockModule('../../../../export-processor/user-journey/path-calculator', () => ({
  PathCalculator: jest.fn().mockImplementation(() => {
    return pathInfo;
  }),
}));

jest.unstable_mockModule('../../../../export-processor/user-journey/section-stats', () => ({
  SectionStats: jest.fn().mockImplementation(() => ({
    pathsPerMaturity: stats,
  })),
}));

jest.unstable_mockModule('../../../../export-processor/content-types/question', () => ({
  Question: jest.fn().mockImplementation(() => ({})),
}));

const { Section } = await import('../../../../export-processor/content-types/section');
const { PathCalculator } =
  await import('../../../../export-processor/user-journey/path-calculator');
const { SectionStats } = await import('../../../../export-processor/user-journey/section-stats');
const { Question } = await import('../../../../export-processor/content-types/question');

describe('Section', () => {
  let mockFields;
  let mockSys;
  let mockRecommendation;

  beforeEach(() => {
    jest.clearAllMocks();

    mockFields = {
      name: 'Test Section',
      interstitialPage: {
        fields: { title: 'Test Page' },
        sys: { id: 'page1' },
      },
      questions: [
        { sys: { id: 'q1' }, fields: { answers: [] } },
        { sys: { id: 'q2' }, fields: { answers: [] } },
      ],
    };
    mockSys = { id: 'section1' };
    mockRecommendation = { id: 'rec1', someData: 'data' };
  });

  test('constructor initializes section correctly', () => {
    const section = new Section({ fields: mockFields, sys: mockSys }, mockRecommendation);

    expect(section.id).toBe('section1');
    expect(section.name).toBe('Test Section');
    expect(section.interstitialPage).toBe(mockFields.interstitialPage);
    expect(section.recommendation).toBe(mockRecommendation);
    expect(section.questions).toHaveLength(2);
    expect(Question).toHaveBeenCalledTimes(2);
    expect(PathCalculator).toHaveBeenCalledWith({
      questions: section.questions,
      recommendation: mockRecommendation,
      sectionId: mockSys.id,
    });
    expect(SectionStats).toHaveBeenCalledWith({ paths: pathInfo.paths });
  });

  test('constructor handles missing questions', () => {
    const fieldsWithoutQuestions = { ...mockFields, questions: null };
    const section = new Section(
      { fields: fieldsWithoutQuestions, sys: mockSys },
      mockRecommendation,
    );

    expect(section.questions).toEqual([]);
  });

  test('toMinimalOutput returns correct structure with writeAllPossiblePaths=true', () => {
    const section = new Section({ fields: mockFields, sys: mockSys }, mockRecommendation);
    const output = section.toMinimalOutput(true);

    expect(output).toEqual({
      section: mockFields.name,
      allPathsStats: stats,
      minimumQuestionPaths: pathInfo.minimumPathsToNavigateQuestions.map((path) =>
        path.map((part) => part.toMinimalOutput()),
      ),
      minimumRecommendationPaths: pathInfo.minimumPathsForRecommendations,
      pathsForAnswers: pathsToMinimalOutput(pathInfo.pathsForAllPossibleAnswers),
      allPossiblePaths: pathsToMinimalOutput(pathInfo.paths),
    });
  });

  test('toMinimalOutput returns correct structure with writeAllPossiblePaths=false', () => {
    const section = new Section({ fields: mockFields, sys: mockSys }, mockRecommendation);
    const output = section.toMinimalOutput(false);

    expect(output).toEqual({
      section: mockFields.name,
      allPathsStats: stats,
      minimumQuestionPaths: pathInfo.minimumPathsToNavigateQuestions.map((path) =>
        path.map((part) => part.toMinimalOutput()),
      ),
      minimumRecommendationPaths: pathInfo.minimumPathsForRecommendations,
      pathsForAnswers: pathsToMinimalOutput(pathInfo.pathsForAllPossibleAnswers),
      allPossiblePaths: undefined,
    });
  });
});
