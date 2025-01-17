import RecommendationChunk from "../../../../export-processor/content-types/recommendation-chunk";
import ErrorLogger from "../../../../export-processor/errors/error-logger";

import { jest } from "@jest/globals";

jest.spyOn(ErrorLogger, "addError");

jest.mocked(ErrorLogger.addError).mockImplementation(() => {
    return;
});

describe("RecommendationChunk", () => {
    const sys = { id: "chunkId" };
    const fields = {
        title: "Mock Title",
        header: "Mock Header",
        content: "Mock Content",
        answers: [
            { sys: { id: "answer1" }, fields: { text: "Answer 1" } },
            { sys: { id: "answer2" }, fields: { text: "Answer 2" } },
        ],
        csLink: {
            sys: { id: "cslink1" },
            fields: { url: "http://example.com", text: "Example Link" },
        },
    };

    test("should create a RecommendationChunk with valid data", () => {
        const recommendationChunk = new RecommendationChunk({
            fields: fields,
            sys: sys,
        });

        expect(recommendationChunk.title).toEqual(fields.title);
        expect(recommendationChunk.header).toEqual(fields.header);
        expect(recommendationChunk.id).toEqual(sys.id);
        expect(recommendationChunk.content).toEqual(fields.content);
        expect(recommendationChunk.answers.length).toEqual(
            fields.answers.length
        );
        expect(recommendationChunk.csLink.url).toBe(fields.csLink.fields.url);
    });

    test("should handle missing answers gracefully", () => {
        const recommendationChunk = new RecommendationChunk({
            fields: { ...fields, answers: undefined },
            sys: sys,
        });

        expect(recommendationChunk.answers.length).toBe(0);
    });

    test("should handle missing content gracefully", () => {
        const recommendationChunk = new RecommendationChunk({
            fields: { ...fields, content: undefined },
            sys: sys,
        });

        expect(recommendationChunk.content).toBeUndefined();
    });

    test("should log an error if no answers are provided", () => {
        new RecommendationChunk({
            fields: { ...fields, answers: undefined },
            sys: sys,
        });

        expect(ErrorLogger.addError).toHaveBeenCalledWith({
            id: sys.id,
            contentType: "recommendationChunk",
            message: `No answers for chunk`,
        });
    });
});
