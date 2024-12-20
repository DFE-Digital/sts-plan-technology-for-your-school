import RecommendationSection from "../../../../export-processor/content-types/recommendation-section";
import ErrorLogger from "../../../../export-processor/errors/error-logger";

import { jest } from "@jest/globals";

jest.spyOn(ErrorLogger, "addError");

jest.mocked(ErrorLogger.addError).mockImplementation(() => {
    return;
});

describe("RecommendationSection", () => {
    const sys = { id: "sectionId" };
    const answers = [
        { sys: { id: "answer1" }, fields: { text: "Answer 1" } },
        { sys: { id: "answer2" }, fields: { text: "Answer 2" } },
        { sys: { id: "answer3" }, fields: { text: "Answer 3" } },
        { sys: { id: "answer4" }, fields: { text: "Answer 4" } },
        { sys: { id: "answer5" }, fields: { text: "Answer 5" } },
    ];

    const chunks = [
        {
            sys: { id: "chunk1" },
            fields: { title: "Chunk 1", answers: [answers[0]] },
        },
        {
            sys: { id: "chunk2" },
            fields: { title: "Chunk 2", answers: [answers[2]] },
        },
        {
            sys: { id: "chunk3" },
            fields: { title: "Chunk 3", answers: [answers[3]] },
        },
        {
            sys: { id: "chunk4" },
            fields: { title: "Chunk 4", answers: [answers[4]] },
        },
        {
            sys: { id: "chunk5" },
            fields: { title: "Chunk 5", answers: [answers[4]] },
        },
    ];

    const fields = {
        answers: answers,
        chunks: chunks,
    };

    describe("Constructor", () => {
        test("should create a RecommendationSection with valid data", () => {
            const recommendationSection = new RecommendationSection({
                fields: fields,
                sys: sys,
            });

            expect(recommendationSection.id).toEqual(sys.id);
            expect(recommendationSection.answers.length).toEqual(
                fields.answers.length
            );
            expect(recommendationSection.chunks.length).toEqual(
                fields.chunks.length
            );
        });

        test("should handle missing answers gracefully", () => {
            const recommendationSection = new RecommendationSection({
                fields: { ...fields, answers: undefined },
                sys: sys,
            });

            expect(recommendationSection.answers.length).toBe(0);
        });

        test("should handle missing chunks gracefully", () => {
            const recommendationSection = new RecommendationSection({
                fields: { ...fields, chunks: undefined },
                sys: sys,
            });

            expect(recommendationSection.chunks).toHaveLength(0);
        });

        test("should log an error if no chunks are provided", () => {
            new RecommendationSection({
                fields: { ...fields, chunks: undefined },
                sys: sys,
            });

            expect(ErrorLogger.addError).toHaveBeenCalledWith({
                id: "sectionId",
                contentType: "recommendationSection",
                message: `No chunks found`,
            });
        });
    });

    describe("getChunksForPath", () => {
        it("should return chunks with answers matching the path", () => {
            const path = [
                { answer: { id: answers[0].sys.id } },
                { answer: { id: answers[2].sys.id } },
            ];
            const expectedChunks = [chunks[0], chunks[1]];
            const recommendationSection = new RecommendationSection({
                fields: fields,
                sys: sys,
            });

            const result = recommendationSection.getChunksForPath(path);

            expect(result.length).toEqual(expectedChunks.length);

            for (const chunk of expectedChunks) {
                const matching = result.find((res) => res.id == chunk.id);
                expect(matching).not.toBeNull();
            }
        });

        it("should return unique chunks even if they have the same answer IDs but different properties", () => {
            const path = [
                { answer: { id: answers[4].sys.id } },
                { answer: { id: answers[4].sys.id } },
            ];
            const expectedChunks = [chunks[3], chunks[4]];
            const recommendationSection = new RecommendationSection({
                fields: fields,
                sys: sys,
            });

            const result = recommendationSection.getChunksForPath(path);

            expect(result.length).toEqual(expectedChunks.length);

            for (const chunk of expectedChunks) {
                const matching = result.find((res) => res.id == chunk.id);
                expect(matching).not.toBeNull();
            }
        });

        it("should return an empty array if no chunks match the path", () => {
            const path = [{ answer: { id: "not-an-existing-answer-id" } }];
            const expectedChunks = [];

            const recommendationSection = new RecommendationSection({
                fields: fields,
                sys: sys,
            });

            const result = recommendationSection.getChunksForPath(path);

            expect(result).toEqual(expectedChunks);
        });

        it("should deduplicate identical chunks", () => {
            const identicalChunks = [
                {
                    sys: { id: "chunk1" },
                    fields: { 
                        title: "Duplicate Chunk",
                        answers: [answers[0]]
                    }
                },
                {
                    sys: { id: "chunk1" },
                    fields: { 
                        title: "Duplicate Chunk",
                        answers: [answers[0]]
                    }
                }
            ];

            const fieldsWithDuplicates = {
                ...fields,
                chunks: identicalChunks
            };

            const path = [
                { answer: { id: answers[0].sys.id } }
            ];

            const recommendationSection = new RecommendationSection({
                fields: fieldsWithDuplicates,
                sys: sys,
            });

            const result = recommendationSection.getChunksForPath(path);

            expect(result.length).toBe(1);
            expect(result[0].title).toBe("Duplicate Chunk");
        });
    });
});
