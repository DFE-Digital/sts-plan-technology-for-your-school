import SubtopicRecommendation, {
    SubtopicRecommendationContentType,
} from "../../../../export-processor/content-types/subtopic-recommendation";
import RecommendationSection from "../../../../export-processor/content-types/recommendation-section";
import ErrorLogger from "../../../../export-processor/errors/error-logger";

import { jest } from "@jest/globals";

jest.spyOn(ErrorLogger, "addError");

jest.mocked(ErrorLogger.addError).mockImplementation(() => {
    return;
});

describe("SubtopicRecommendation", () => {
    const sys = { id: "subtopicRecommendationId" };
    const intros = [
        { sys: { id: "intro1" }, fields: { maturity: "Low" } },
        { sys: { id: "intro2" }, fields: { text: "Medium" } },
        { sys: { id: "intro3" }, fields: { text: "High" } },
    ];

    const subtopic = {
        sys: {
            id: "subtopic-id",
        },
        fields: {
            questions: [],
        },
    };

    const section = {
        sys: {
            id: "section-id",
        },
        fields: {
            chunks: [{ sys: { id: "chunk-one" }, fields: {} }],
        },
    };

    const fields = {
        intros: intros,
        subtopic: subtopic,
        section: section,
    };

    describe("Constructor", () => {
        test("should create a SubtopicRecommendation with valid data", () => {
            const subtopicRecommendation = new SubtopicRecommendation({
                fields: fields,
                sys: sys,
            });

            expect(subtopicRecommendation.id).toEqual(sys.id);
            expect(subtopicRecommendation.intros.length).toEqual(
                fields.intros.length
            );
            expect(subtopicRecommendation.section.id).toEqual(
                fields.section.sys.id
            );
            expect(subtopicRecommendation.subtopic.id).toEqual(
                fields.subtopic.sys.id
            );
        });

        describe("should handle missing subtopic gracefully", () => {
            const expectedErrorArgs = {
                id: sys.id,
                contentType: SubtopicRecommendationContentType,
                message: `No Subtopic found`,
            };

            test("Undefined", () => {
                const subtopicRecommendation = new SubtopicRecommendation({
                    fields: { ...fields, subtopic: undefined },
                    sys: sys,
                });

                expect(subtopicRecommendation.subtopic).toBeNull();
                expect(ErrorLogger.addError).toHaveBeenCalledWith(
                    expectedErrorArgs
                );
            });

            test("Missing fields", () => {
                const subtopicRecommendation = new SubtopicRecommendation({
                    fields: {
                        ...fields,
                        subtopic: { ...subtopic, fields: undefined },
                    },
                    sys: sys,
                });

                expect(subtopicRecommendation.subtopic).toBeNull();
                expect(ErrorLogger.addError).toHaveBeenCalledWith(
                    expectedErrorArgs
                );
            });

            test("Missing sys", () => {
                const subtopicRecommendation = new SubtopicRecommendation({
                    fields: {
                        ...fields,
                        subtopic: { ...subtopic, sys: undefined },
                    },
                    sys: sys,
                });

                expect(subtopicRecommendation.subtopic).toBeNull();
                expect(ErrorLogger.addError).toHaveBeenCalledWith(
                    expectedErrorArgs
                );
            });
        });

        test("getContentForMaturityAndPath", () => {
            jest.spyOn(
                RecommendationSection.prototype,
                "getChunksForPath"
            ).mockImplementation((path) => path);

            const path = [{ sys: "answer-1" }];

            const subtopicRecommendation = new SubtopicRecommendation({
                fields: fields,
                sys: sys,
            });

            const maturity = intros[0].fields.maturity;

            const result = subtopicRecommendation.getContentForMaturityAndPath({
                maturity: maturity,
                path,
            });

            expect(result.chunks).toEqual(path);
            expect(result.intro.id).toEqual(intros[0].sys.id);
        });
    });
});
