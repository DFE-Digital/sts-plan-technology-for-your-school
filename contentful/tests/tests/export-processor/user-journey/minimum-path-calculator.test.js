import { MinimumPathCalculator } from "../../../../export-processor/user-journey/minimum-path-calculator.js";
import ErrorLogger from "../../../../export-processor/errors/error-logger.js";

import { jest } from "@jest/globals";

jest.spyOn(ErrorLogger, "addError");

/**
 * @typedef {{ question: string, answer: string }} pathpart
 */

/**
 * Creates a mock UserJourney for testing the minimum path calculator
 * @param {{ path: pathpart[], maturity: string}} params
 * @returns {{ path: pathpart[], maturity: string, questionIdsAnswered: string[] }}
 */
const createMockUserJourney = ({ path, maturity }) => ({
    path,
    maturity,
    questionIdsAnswered: path.map((part) => part.question.id),
});

describe("MinimumPathCalculator", () => {
    let mockQuestions;
    let mockPaths;
    let mockSortedPaths;

    /**
     * @type {MinimumPathCalculator}
     */
    let calculator;
    const sectionId = "section-id";

    beforeEach(() => {
        mockQuestions = [
            {
                id: "q1",
                text: "Question 1",
            },
            {
                id: "q2",
                text: "Question 2",
            },
            {
                id: "q3",
                text: "Question 3",
            },
        ];

        mockPaths = [
            createMockUserJourney({
                path: [{ question: mockQuestions[0], answer: { id: "a1" } }],
                maturity: "Low",
            }),
            createMockUserJourney({
                path: [
                    { question: mockQuestions[0], answer: { id: "a2" } },
                    { question: mockQuestions[1], answer: { id: "a3" } },
                ],
                maturity: "Medium",
            }),
            createMockUserJourney({
                path: [
                    { question: mockQuestions[0], answer: { id: "a4" } },
                    { question: mockQuestions[2], answer: { id: "a5" } },
                ],
                maturity: "High",
            }),
        ];

        mockSortedPaths = [...mockPaths].sort(
            (a, b) => b.path.length - a.path.length
        );

        calculator = new MinimumPathCalculator({
            questions: mockQuestions,
            paths: mockPaths,
            sortedPaths: mockSortedPaths,
            sectionId,
        });
    });

    describe("getUniquePaths", () => {
        it("should return unique paths based on questions answered", () => {
            const duplicatePaths = [
                ...mockSortedPaths,
                createMockUserJourney(
                {
                    path: [
                        { question: mockQuestions[0], answer: { id: "a6" } },
                    ],
                    maturity: "Low",
                }),
            ];

            calculator = new MinimumPathCalculator({
                questions: mockQuestions,
                paths: mockPaths,
                sortedPaths: duplicatePaths,
                sectionId,
            });

            const uniquePaths = calculator.getUniquePaths();

            expect(uniquePaths.length).toBeLessThan(duplicatePaths.length);

            const questionPatterns = uniquePaths.map((p) =>
                p.questionIdsAnswered.join(",")
            );
            const uniquePatterns = new Set(questionPatterns);
            expect(uniquePatterns.size).toBe(uniquePaths.length);
        });

        it("should maintain order of paths", () => {
            const uniquePaths = calculator.getUniquePaths();

            const lengths = uniquePaths.map((p) => p.path.length);
            expect(lengths).toEqual([...lengths].sort((a, b) => b - a));
        });
    });

    describe("calculateMinimumPathsToQuestions", () => {
        it("should find minimum paths to cover all questions", () => {
            const minimumPaths = calculator.calculateMinimumPathsToQuestions();

            const coveredQuestions = new Set(
                minimumPaths.flatMap((path) =>
                    path.map((part) => part.question.id)
                )
            );

            mockQuestions.forEach((question) => {
                expect(coveredQuestions).toContain(question.id);
            });
        });

        it("should handle questions with no available paths", () => {
            const questionsWithExtra = [
                ...mockQuestions,
                { id: "q4", text: "Question 4" },
            ];

            calculator = new MinimumPathCalculator({
                questions: questionsWithExtra,
                paths: mockPaths,
                sortedPaths: mockSortedPaths,
                sectionId,
            });

            const minimumPaths = calculator.calculateMinimumPathsToQuestions();

            expect(minimumPaths.length).toBeGreaterThan(0);
        });
    });

    describe("calculateMinimumPathsForRecommendations", () => {
        it("should find paths for each maturity level", () => {
            const recommendationPaths =
                calculator.calculateMinimumPathsForRecommendations();

            expect(recommendationPaths).toHaveProperty("Low");
            expect(recommendationPaths).toHaveProperty("Medium");
            expect(recommendationPaths).toHaveProperty("High");
        });

        it("should log error for missing maturity paths", () => {
            const pathsWithMissingMaturity = mockPaths.filter(
                (p) => p.maturity !== "Medium"
            );

            calculator = new MinimumPathCalculator({
                questions: mockQuestions,
                paths: pathsWithMissingMaturity,
                sortedPaths: pathsWithMissingMaturity,
                sectionId,
            });

            calculator.calculateMinimumPathsForRecommendations();

            expect(ErrorLogger.addError).toHaveBeenCalledWith(
                expect.objectContaining({
                    contentType: "section",
                    message: expect.stringContaining("Medium"),
                })
            );
        });

        it("should return paths that match maturity levels", () => {
            const recommendationPaths =
                calculator.calculateMinimumPathsForRecommendations();

            mockPaths.forEach((mockPath) => {
                if (mockPath.maturity) {
                    expect(
                        recommendationPaths[mockPath.maturity]
                    ).toBeDefined();
                    expect(recommendationPaths[mockPath.maturity]).toEqual(
                        mockPath.path
                    );
                }
            });
        });
    });

    describe("getFirstPathContainingQuestion", () => {
        it("should return the first path containing the specified question", () => {
            const questionId = "q2";
            const result = calculator.getFirstPathContainingQuestion(questionId);

            expect(result).toBeDefined();
            expect(result).toBe(mockPaths[1]);
            expect(result.path).toContainEqual(
                expect.objectContaining({
                    question: expect.objectContaining({ id: questionId })
                })
            );
        });

        it("should log an error when question is not found", () => {
            const nonExistentQuestionId = "nonexistent";
            calculator.getFirstPathContainingQuestion(nonExistentQuestionId);

            expect(ErrorLogger.addError).toHaveBeenCalledWith({
                id: sectionId,
                contentType: "section",
                message: `Couldn't find question ${nonExistentQuestionId} in section ${sectionId}`,
            });
        });
    });

    describe("handleRemainingQuestions", () => {
        it("should add paths for remaining questions when paths exist", () => {
            const remainingQuestions = ["q2", "q3"];
            const initialPaths = [
                { question: mockQuestions[0], answer: { id: "a1" } }
            ];
            
            const result = calculator.handleRemainingQuestions(remainingQuestions, initialPaths);

            expect(result.length).toBe(3);
            
            const questionIds = result.flatMap(path => 
                Array.isArray(path) 
                    ? path.map(part => part.question.id)
                    : []
            );
            
            expect(questionIds).toContain("q2");
            expect(questionIds).toContain("q3");
        });

        it("should skip questions that have no available paths", () => {
            const remainingQuestions = ["nonexistent", "q2"];
            const initialPaths = [
                { question: mockQuestions[0], answer: { id: "a1" } }
            ];
            
            const result = calculator.handleRemainingQuestions(remainingQuestions, initialPaths);

            expect(result.length).toBe(2);
            
            expect(ErrorLogger.addError).toHaveBeenCalledWith(
                expect.objectContaining({
                    contentType: "section",
                    message: expect.stringContaining("nonexistent")
                })
            );
        });

        it("should return original paths array when no remaining questions have paths", () => {
            const remainingQuestions = [];
            const initialPaths = [
                { question: mockQuestions[0], answer: { id: "a1" } }
            ];
            
            const result = calculator.handleRemainingQuestions(remainingQuestions, initialPaths);

            expect(result).toEqual(initialPaths);
            expect(result.length).toBe(1);
        });
    });
});
