import { jest } from "@jest/globals";
import { generateContent } from "../../helpers/content-generator.js";
import ErrorLogger from "../../../export-processor/errors/error-logger.js";
import ContentType from "../../../export-processor/content-types/content-type.js";

const subtopicRecommendation = jest.fn().mockImplementation((args) => ({
    ...args,
}));

jest.unstable_mockModule(
    "../../../export-processor/content-types/subtopic-recommendation.js",
    () => ({
        default: subtopicRecommendation,
    })
);

const DataMapper = (await import("../../../export-processor/data-mapper.js"))
    .default;

describe("DataMapper", () => {
    let mockEntries;
    let mockContentTypes;

    beforeEach(() => {
        const { entries, contentTypes } = generateContent(100, 400);
        mockContentTypes = contentTypes;
        mockEntries = entries;
    });

    describe("constructor", () => {
        it("should initialize with entries and content types", () => {
            const mapper = new DataMapper({
                entries: mockEntries,
                contentTypes: mockContentTypes,
            });
            expect(mapper.contents).toBeDefined();
            expect(mapper.contentTypes).toBeDefined();
        });
    });

    describe("stripLocalisationFromField", () => {
        it("should extract en-US value from localised field", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const localisedField = { "en-US": "test value" };

            const result = mapper.stripLocalisationFromField(localisedField);

            expect(result).toBe("test value");
        });
    });

    describe("getContentTypeSet", () => {
        it("should create new Map for unknown content type", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });

            const result = mapper.getContentTypeSet("newType");

            expect(result).toBeInstanceOf(Map);
            expect(mapper.contents.get("newType")).toBe(result);
        });

        it("should return existing Map for known content type", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const firstCall = mapper.getContentTypeSet("existingType");
            const secondCall = mapper.getContentTypeSet("existingType");

            expect(secondCall).toBe(firstCall);
        });
    });

    describe("mapContentTypes", () => {
        it("should map content types to ContentType instances", () => {
            const mapper = new DataMapper({
                entries: [],
                contentTypes: mockContentTypes,
            });

            const mappedType = mapper.contentTypes.get("section");

            expect(mappedType).toBeInstanceOf(ContentType);
            expect(mappedType.id).toBe("section");
        });
    });

    describe("tryAddToSet", () => {
        it("should add entry to correct content type set and strip localisation", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockEntry = {
                sys: {
                    id: "test-id",
                    contentType: { sys: { id: "testType" } },
                },
                fields: {
                    title: { "en-US": "Test Title" },
                },
            };

            mapper.tryAddToSet(mockEntry);

            const result = mapper.contents.get("testType").get("test-id");
            expect(result.fields.title).toBe("Test Title");
        });
    });

    describe("stripLocalisationFromAllFields", () => {
        it("should strip localisation from all fields in an entry", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const entry = {
                fields: {
                    title: { "en-US": "Test Title" },
                    description: { "en-US": "Test Description" },
                },
            };

            mapper.stripLocalisationFromAllFields(entry);

            expect(entry.fields.title).toBe("Test Title");
            expect(entry.fields.description).toBe("Test Description");
        });
    });

    describe("mapRelationshipsForEntry", () => {
        it("should map single reference relationships", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockContentType = {
                getReferencedTypesForField: jest
                    .fn()
                    .mockReturnValue(["referencedType"]),
            };
            const mockReferencedEntry = {
                fields: { name: "Referenced Entry" },
            };

            mapper.contents.set(
                "referencedType",
                new Map([["ref-id", mockReferencedEntry]])
            );

            const entry = {
                fields: {
                    reference: { sys: { id: "ref-id" } },
                },
            };

            mapper.mapRelationshipsForEntry(entry, mockContentType);

            expect(entry.fields.reference).toBe(mockReferencedEntry);
        });

        it("should map array reference relationships", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockContentType = {
                getReferencedTypesForField: jest
                    .fn()
                    .mockReturnValue(["referencedType"]),
            };
            const mockReferencedEntries = [
                { fields: { name: "Referenced Entry 1" } },
                { fields: { name: "Referenced Entry 2" } },
            ];

            mapper.contents.set(
                "referencedType",
                new Map([
                    ["ref-id-1", mockReferencedEntries[0]],
                    ["ref-id-2", mockReferencedEntries[1]],
                ])
            );

            const entry = {
                fields: {
                    references: [
                        { sys: { id: "ref-id-1" } },
                        { sys: { id: "ref-id-2" } },
                    ],
                },
            };

            mapper.mapRelationshipsForEntry(entry, mockContentType);

            expect(entry.fields.references).toEqual(mockReferencedEntries);
        });

        it("should log missing content for single reference relationships", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockContentType = {
                getReferencedTypesForField: jest
                    .fn()
                    .mockReturnValue(["referencedType"]),
            };

            const entry = {
                sys: {
                    id: "test-entry-id",
                    contentType: { sys: { id: "testType" } },
                },
                fields: {
                    reference: { sys: { id: "non-existent-ref" } },
                },
            };

            const errorLoggerSpy = jest.spyOn(ErrorLogger, "addError");

            mapper.mapRelationshipsForEntry(entry, mockContentType);

            expect(errorLoggerSpy).toHaveBeenCalledWith({
                id: "test-entry-id",
                contentType: "testType",
                message:
                    "Could not find matching content for referencedType reference",
            });

            errorLoggerSpy.mockRestore();
        });

        it("should log missing content for array reference relationships", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockContentType = {
                getReferencedTypesForField: jest
                    .fn()
                    .mockReturnValue(["referencedType"]),
            };

            const entry = {
                sys: {
                    id: "test-entry-id",
                    contentType: { sys: { id: "testType" } },
                },
                fields: {
                    references: [
                        { sys: { id: "existing-ref" } },
                        { sys: { id: "non-existent-ref" } },
                    ],
                },
            };

            mapper.contents.set(
                "referencedType",
                new Map([
                    ["existing-ref", { fields: { name: "Existing Entry" } }],
                ])
            );

            const errorLoggerSpy = jest.spyOn(ErrorLogger, "addError");

            mapper.mapRelationshipsForEntry(entry, mockContentType);

            expect(errorLoggerSpy).toHaveBeenCalledWith({
                id: "test-entry-id",
                contentType: "testType",
                message:
                    "Could not find matching content for referencedType non-existent-ref",
            });

            expect(entry.fields.references).toHaveLength(1);
            expect(entry.fields.references[0].fields.name).toBe(
                "Existing Entry"
            );

            errorLoggerSpy.mockRestore();
        });
    });

    describe("getMatchingContentForReference", () => {
        it("should return undefined for invalid reference", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const result = mapper.getMatchingContentForReference(["type"], {});
            expect(result).toBeUndefined();
        });

        it("should return matching content for valid reference", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockContent = { fields: { name: "Test Content" } };
            mapper.contents.set(
                "testType",
                new Map([["content-id", mockContent]])
            );

            const result = mapper.getMatchingContentForReference(["testType"], {
                sys: { id: "content-id" },
            });

            expect(result).toBe(mockContent);
        });
    });

    describe("mappedSections", () => {
        it("should return cached sections if already mapped", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockSections = ["section1", "section2"];
            mapper._alreadyMappedSections = mockSections;

            const result = mapper.mappedSections;

            expect(result).toBe(mockSections);
        });

        it("should map sections if not already cached", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockSubtopics = ["subtopic1", "subtopic2"];

            mapper.sectionsToClasses = jest.fn().mockReturnValue(mockSubtopics);

            mapper.contents.set("section", new Map());

            const result = mapper.mappedSections;

            expect(Array.isArray(result)).toBe(true);
            expect(mapper.sectionsToClasses).toHaveBeenCalledWith(
                mapper.contents.get("section")
            );
            expect(result).toEqual(mockSubtopics);
            expect(mapper._alreadyMappedSections).toBe(result);
        });

        it("should only map sections once", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockSubtopics = ["subtopic1", "subtopic2"];

            mapper.sectionsToClasses = jest.fn().mockReturnValue(mockSubtopics);
            mapper.contents.set("section", new Map());

            const result1 = mapper.mappedSections;
            const result2 = mapper.mappedSections;

            expect(mapper.sectionsToClasses).toHaveBeenCalledTimes(1);
            expect(result1).toBe(result2);
        });
    });

    describe("sectionsToClasses", () => {
        /**
         * @type {DataMapper}
         */
        let mapper;
        const mockSubtopicRecommendation = {
            subtopic: { name: "Test Subtopic" },
        };

        const subtopicRecommendations = new Map([
            ["rec1", mockSubtopicRecommendation],
        ]);

        const sections = new Map([["section1", { subtopic: "subtopic" }]]);

        beforeEach(() => {
            mapper = new DataMapper({
                entries: [],
                contentTypes: [],
            });
        });

        it("should throw error when no subtopic recommendations found", () => {
            expect(() => {
                Array.from(mapper.sectionsToClasses(sections));
            }).toThrow("No subtopic recommendations found");
        });

        it("should return empty iterator when sections is null", () => {
            mapper.contents.set(
                "subtopicRecommendation",
                subtopicRecommendations
            );

            const iterator = mapper.sectionsToClasses(null);
            expect(Array.from(iterator)).toEqual([]);
        });

        it("should return empty iterator when sections is empty", () => {
            mapper.contents.set(
                "subtopicRecommendation",
                subtopicRecommendations
            );

            const iterator = mapper.sectionsToClasses([]);
            expect(Array.from(iterator)).toEqual([]);
        });

        it("should map subtopic recommendations to subtopics", () => {
            mapper.contents.set(
                "subtopicRecommendation",
                subtopicRecommendations
            );

            const result = Array.from(mapper.sectionsToClasses(sections));

            expect(result).toHaveLength(1);
            expect(result[0]).toEqual(mockSubtopicRecommendation.subtopic);
            expect(subtopicRecommendation).toHaveBeenCalled();
        });
    });

    describe("copyRelationships", () => {
        let mapper;

        beforeEach(() => {
            mapper = new DataMapper({ entries: [], contentTypes: [] });
        });

        it("should map parent items to their corresponding children", () => {
            const childrenMap = new Map([
                ["child1", { name: "Child 1" }],
                ["child2", { name: "Child 2" }],
                ["child3", { name: "Child 3" }],
            ]);

            const parentArray = [
                { sys: { id: "child1" } },
                { sys: { id: "child2" } },
            ];

            const result = mapper.copyRelationships(parentArray, childrenMap);

            expect(result).toHaveLength(2);
            expect(result[0]).toEqual({ name: "Child 1" });
            expect(result[1]).toEqual({ name: "Child 2" });
        });

        it("should return undefined for parent items that don't exist in children map", () => {
            const childrenMap = new Map([["child1", { name: "Child 1" }]]);

            const parentArray = [
                { sys: { id: "child1" } },
                { sys: { id: "nonexistent" } },
            ];

            const result = mapper.copyRelationships(parentArray, childrenMap);

            expect(result).toHaveLength(2);
            expect(result[0]).toEqual({ name: "Child 1" });
            expect(result[1]).toBeUndefined();
        });

        it("should return empty array for empty parent array", () => {
            const childrenMap = new Map([["child1", { name: "Child 1" }]]);

            const result = mapper.copyRelationships([], childrenMap);

            expect(result).toHaveLength(0);
        });
    });

    describe("convertToMinimalSectionInfo", () => {
        let mapper;

        beforeEach(() => {
            mapper = new DataMapper({ entries: [], contentTypes: [] });
        });

        it("should convert section to minimal info without allPossiblePaths", () => {
            const mockSection = {
                name: "Test Section",
                stats: {
                    pathsPerMaturity: { basic: 2, intermediate: 3 }
                },
                pathInfo: {
                    minimumPathsToNavigateQuestions: ["path1", "path2"],
                    minimumPathsForRecommendations: ["path3"],
                    paths: [
                        {
                            recommendation: {
                                displayName: "Rec 1",
                                maturity: "basic"
                            },
                            pathWithTextOnly: ["step1", "step2"]
                        }
                    ]
                }
            };

            const result = mapper.convertToMinimalSectionInfo(mockSection);

            expect(result).toEqual({
                section: "Test Section",
                allPathsStats: { basic: 2, intermediate: 3 },
                minimumQuestionPaths: ["path1", "path2"],
                minimumRecommendationPaths: ["path3"],
                allPossiblePaths: undefined
            });
        });

        it("should include allPossiblePaths when writeAllPossiblePaths is true", () => {
            const mockSection = {
                name: "Test Section",
                stats: {
                    pathsPerMaturity: { basic: 2 }
                },
                pathInfo: {
                    minimumPathsToNavigateQuestions: [],
                    minimumPathsForRecommendations: [],
                    paths: [
                        {
                            recommendation: {
                                displayName: "Rec 1",
                                maturity: "basic"
                            },
                            pathWithTextOnly: ["step1", "step2"]
                        }
                    ]
                }
            };

            const result = mapper.convertToMinimalSectionInfo(mockSection, true);

            expect(result.allPossiblePaths).toEqual([
                {
                    recommendation: {
                        name: "Rec 1",
                        maturity: "basic"
                    },
                    path: ["step1", "step2"]
                }
            ]);
        });

        it("should handle null recommendation in paths", () => {
            const mockSection = {
                name: "Test Section",
                stats: {
                    pathsPerMaturity: {}
                },
                pathInfo: {
                    minimumPathsToNavigateQuestions: [],
                    minimumPathsForRecommendations: [],
                    paths: [
                        {
                            recommendation: null,
                            pathWithTextOnly: ["step1"]
                        }
                    ]
                }
            };

            const result = mapper.convertToMinimalSectionInfo(mockSection, true);

            expect(result.allPossiblePaths).toEqual([
                {
                    recommendation: null,
                    path: ["step1"]
                }
            ]);
        });
    });

    describe("pages", () => {
        it("should return the pages from contents map", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            const mockPages = new Map([
                ["page1", { title: "Page 1" }],
                ["page2", { title: "Page 2" }]
            ]);
            
            mapper.contents.set("page", mockPages);

            const result = mapper.pages;

            expect(result).toBe(mockPages);
        });

        it("should return undefined if no pages exist", () => {
            const mapper = new DataMapper({ entries: [], contentTypes: [] });
            
            const result = mapper.pages;

            expect(result).toBeUndefined();
        });
    });
});
