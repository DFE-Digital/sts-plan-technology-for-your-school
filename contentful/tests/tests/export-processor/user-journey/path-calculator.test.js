import { PathCalculator } from "../../../../export-processor/user-journey/path-calculator.js";
import { UserJourney } from "../../../../export-processor/user-journey/user-journey.js";
import { PathBuilder } from "../../../../export-processor/user-journey/path-builder.js";
import { MinimumPathCalculator } from "../../../../export-processor/user-journey/minimum-path-calculator.js";
import { AnswerPathCalculator } from "../../../../export-processor/user-journey/answer-path-calculator.js";

import { jest } from "@jest/globals";

jest.mock("../../../../export-processor/user-journey/user-journey.js");
jest.mock("../../../../export-processor/user-journey/path-builder.js");
jest.mock(
    "../../../../export-processor/user-journey/minimum-path-calculator.js"
);
jest.mock(
    "../../../../export-processor/user-journey/answer-path-calculator.js"
);

describe("PathCalculator", () => {
    let mockQuestions;
    let mockRecommendation;
    let mockPaths;

    /**
     * @type {PathCalculator}
     */
    let pathCalculator;

    beforeEach(() => {
        mockQuestions = [
            {
                id: "q1",
                text: "Question 1",
                answers: [
                    {
                        id: "a1",
                        text: "Answer 1",
                        nextQuestion: { sys: { id: "q2" } },
                    },
                ],
            },
            {
                id: "q2",
                text: "Question 2",
                answers: [
                    {
                        id: "a2",
                        text: "Answer 2",
                        nextQuestion: null,
                    },
                ],
            },
        ];

        mockRecommendation = {
            section: {
                chunks: [{ id: "chunk1" }],
                getChunksForPath: jest.fn(),
            },
        };

        mockPaths = [
            [
                {
                    question: mockQuestions[0],
                    answer: mockQuestions[0].answers[0],
                },
            ],
            [
                {
                    question: mockQuestions[1],
                    answer: mockQuestions[1].answers[0],
                },
            ],
        ];

        PathBuilder.prototype.getPathsForQuestion = jest
            .fn()
            .mockReturnValue(mockPaths);
        UserJourney.prototype.setRecommendation = jest.fn();

        MinimumPathCalculator.prototype.calculateMinimumPathsToQuestions = jest
            .fn()
            .mockReturnValue(["path1", "path2"]);
        MinimumPathCalculator.prototype.calculateMinimumPathsForRecommendations =
            jest.fn().mockReturnValue(new Map([["rec1", "path1"]]));

        AnswerPathCalculator.prototype.getPathsForAllAnswers = jest
            .fn()
            .mockReturnValue(["answerPath1", "answerPath2"]);

        pathCalculator = new PathCalculator({
            questions: mockQuestions,
            recommendation: mockRecommendation,
        });
    });

    describe("constructor", () => {
        it("should initialize with questions and recommendation", () => {
            expect(pathCalculator.questions).toBe(mockQuestions);
            expect(pathCalculator.recommendation).toBe(mockRecommendation);
            expect(
                PathBuilder.prototype.getPathsForQuestion
            ).toHaveBeenCalledWith(mockQuestions[0]);
        });

        it("should create user journeys from paths", () => {
            expect(pathCalculator.paths.length).toBe(mockPaths.length);
            expect(
                UserJourney.prototype.setRecommendation
            ).toHaveBeenCalledWith(mockRecommendation);
        });
    });

    describe("sortedPaths", () => {
        it("should return paths sorted by length", () => {
            const sortedPaths = pathCalculator.sortedPaths;
            expect(sortedPaths).toBeDefined();
            expect(sortedPaths.length).toBe(mockPaths.length);
        });
    });

    describe("minimumPathsToNavigateQuestions", () => {
        it("should return minimum paths to navigate questions", () => {
            const paths = pathCalculator.minimumPathsToNavigateQuestions;
            expect(paths).toEqual(["path1", "path2"]);
            expect(
                MinimumPathCalculator.prototype.calculateMinimumPathsToQuestions
            ).toHaveBeenCalled();
        });

        it("should return cached minimum paths to navigate questions", () => {
            const cachedPaths = ["cachedPath1", "cachedPath2"];
            pathCalculator._minimumPathsToNavigateQuestions = cachedPaths;

            const paths = pathCalculator.minimumPathsToNavigateQuestions;
            expect(paths).toEqual(cachedPaths);

            expect(
                MinimumPathCalculator.prototype.calculateMinimumPathsToQuestions
            ).not.toHaveBeenCalled();
        });
    });

    describe("minimumPathsForRecommendations", () => {
        it("should return minimum paths for recommendations", () => {
            const paths = pathCalculator.minimumPathsForRecommendations;
            expect(paths).toEqual(new Map([["rec1", "path1"]]));
            expect(
                MinimumPathCalculator.prototype
                    .calculateMinimumPathsForRecommendations
            ).toHaveBeenCalled();
        });
    });

    describe("pathsForAllPossibleAnswers", () => {
        it("should return paths for all possible answers", () => {
            const paths = pathCalculator.pathsForAllPossibleAnswers;
            expect(paths).toBeDefined();
            expect(
                AnswerPathCalculator.prototype.getPathsForAllAnswers
            ).toHaveBeenCalled();
        });

        it("should return cached paths for all possible answers", () => {
            const cachedPaths = ["cachedPath1", "cachedPath2"];
            pathCalculator._pathsForAllPossibleAnswers = cachedPaths;

            const paths = pathCalculator.pathsForAllPossibleAnswers;
            expect(paths).toEqual(cachedPaths);

            expect(
                AnswerPathCalculator.prototype.getPathsForAllAnswers
            ).not.toHaveBeenCalled();
        });
    });

    describe("setNextQuestions", () => {
        it("should set next questions for answers", () => {
            const questions = [
                {
                    id: "q1",
                    answers: [
                        {
                            nextQuestion: { sys: { id: "q2" } },
                        },
                    ],
                },
                {
                    id: "q2",
                    answers: [],
                },
            ];

            //setNextQuestions is called in constructor
            new PathCalculator({
                questions: questions,
                recommendation: mockRecommendation,
            });

            expect(questions[0].answers[0].nextQuestion).toBe(questions[1]);
        });

        it("should handle missing next question references", () => {
            const consoleSpy = jest
                .spyOn(console, "error")
                .mockImplementation(() => {});

            const questions = [
                {
                    id: "q1",
                    text: "Question 1",
                    answers: [
                        {
                            id: "a1",
                            text: "Answer 1",
                            nextQuestion: { sys: { id: "non-existent" } },
                        },
                    ],
                },
            ];

            //setNextQuestions is called in constructor
            new PathCalculator({
                questions: questions,
                recommendation: mockRecommendation,
            });

            expect(questions[0].answers[0].nextQuestion).toBeNull();
            expect(consoleSpy).toHaveBeenCalled();

            consoleSpy.mockRestore();
        });
    });

    describe("checkAllChunksTested", () => {
        it("should log error for untested chunks", () => {
            const consoleSpy = jest
                .spyOn(console, "error")
                .mockImplementation(() => {});

            mockRecommendation.section.getChunksForPath.mockReturnValue([]);

            pathCalculator.checkAllChunksTested();

            expect(consoleSpy).toHaveBeenCalled();

            consoleSpy.mockRestore();
        });

        it("should correctly map chunk IDs", () => {
            const mockChunks = [{ id: "chunk1" }, { id: "chunk2" }];
            mockRecommendation.section.chunks = mockChunks;
            mockRecommendation.section.getChunksForPath.mockReturnValue(
                mockChunks
            );

            const consoleSpy = jest
                .spyOn(console, "error")
                .mockImplementation(() => {});

            pathCalculator.checkAllChunksTested();

            expect(
                mockRecommendation.section.chunks.map((chunk) => chunk.id)
            ).toEqual(["chunk1", "chunk2"]);

            consoleSpy.mockRestore();
        });
    });
});
