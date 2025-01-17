import { UserJourney } from "../../../../export-processor/user-journey/user-journey.js";
import ErrorLogger from "../../../../export-processor/errors/error-logger.js";

import { jest } from "@jest/globals";

jest.spyOn(ErrorLogger, "addError");
jest.mocked(ErrorLogger.addError).mockImplementation(() => {
    return;
});

describe("UserJourney", () => {
    let mockPath;
    let mockSection;

    /**
     * @type {UserJourney}
     */
    let userJourney;

    beforeEach(() => {
        jest.clearAllMocks();

        mockPath = [
            {
                question: { id: "q1", text: "Question 1" },
                answer: { text: "Answer 1", maturity: "Low" },
            },
            {
                question: { id: "q2", text: "Question 2" },
                answer: { text: "Answer 2", maturity: "Medium" },
            },
        ];

        mockSection = { name: "Test Section" };
        userJourney = new UserJourney(mockPath, mockSection);
    });

    describe("maturity", () => {
        it("should return the lowest maturity level", () => {
            expect(userJourney.maturity).toBe("Low");
        });

        it("should log error when maturity cannot be determined", () => {
            const invalidPath = [
                {
                    question: { id: "q1", text: "Question 1" },
                    answer: { text: "Answer 1", maturity: undefined },
                },
            ];
            const journey = new UserJourney(invalidPath, mockSection);

            journey.maturity;

            expect(ErrorLogger.addError).toHaveBeenCalled();
        });
    });

    describe("questionIdsAnswered", () => {
        it("should return array of question IDs", () => {
            expect(userJourney.questionIdsAnswered).toEqual(["q1", "q2"]);
        });

        it("should return cached array of question IDs", () => {
            const cachedQuestionIds = ["cachedq1", "cachedq2"];
            userJourney._questionIdsAnswered = cachedQuestionIds;
            expect(userJourney.questionIdsAnswered).toEqual(cachedQuestionIds);
        });
    });

    describe("pathToString", () => {
        it("should return formatted path string", () => {
            const expected =
                'Question "Question 1" - "Answer 1" -> Question "Question 2" - "Answer 2"';
            expect(userJourney.pathToString).toBe(expected);
        });
    });

    describe("setRecommendation", () => {
        it("should set recommendation intro when matching maturity exists", () => {
            const mockRecommendation = {
                id: "rec1",
                intros: [
                    { maturity: "Low", displayName: "Low Rec" },
                    { maturity: "Medium", displayName: "Medium Rec" },
                ],
            };

            userJourney.setRecommendation(mockRecommendation);

            expect(userJourney.recommendationIntro).toEqual({
                maturity: "Low",
                displayName: "Low Rec",
            });
        });

        it("should log error when no matching recommendation intro found", () => {
            const mockRecommendation = {
                id: "rec1",
                intros: [{ maturity: "High", displayName: "High Rec" }],
            };

            userJourney.setRecommendation(mockRecommendation);

            expect(ErrorLogger.addError).toHaveBeenCalled();
            expect(userJourney.recommendationIntro).toBeUndefined();
        });
    });

    describe("maturityRanking", () => {
        it("should convert string maturity to number", () => {
            expect(userJourney.maturityRanking("Low")).toBe(0);
            expect(userJourney.maturityRanking("Medium")).toBe(1);
            expect(userJourney.maturityRanking("High")).toBe(2);
        });

        it("should convert number to string maturity", () => {
            expect(userJourney.maturityRanking(0)).toBe("Low");
            expect(userJourney.maturityRanking(1)).toBe("Medium");
            expect(userJourney.maturityRanking(2)).toBe("High");
        });

        it("should return null for invalid maturity", () => {
            expect(userJourney.maturityRanking("Invalid")).toBeNull();
        });
    });

    describe("pathWithTextOnly", () => {
        it("should return simplified path with only question and answer texts", () => {
            const expected = [
                { question: "Question 1", answer: "Answer 1" },
                { question: "Question 2", answer: "Answer 2" },
            ];
            expect(userJourney.pathWithTextOnly).toEqual(expected);
        });
    });

    describe("toMinimalOutput", () => {
        it("should return formatted output with recommendation when available", () => {
            const mockRecommendation = {
                id: "rec1",
                intros: [{ maturity: "Low", displayName: "Low Rec" }],
            };
            userJourney.setRecommendation(mockRecommendation);

            const expected = {
                recommendation: {
                    name: "Low Rec",
                    maturity: "Low",
                },
                path: [
                    { question: "Question 1", answer: "Answer 1" },
                    { question: "Question 2", answer: "Answer 2" },
                ],
            };
            expect(userJourney.toMinimalOutput()).toEqual(expected);
        });

        it("should return null recommendation when not set", () => {
            const output = userJourney.toMinimalOutput();
            expect(output.recommendation).toBeNull();
        });
    });
});
