import { jest } from "@jest/globals";

const mockClient = {
    contentType: {
        getMany: jest.fn(),
    },
    entry: {
        getMany: jest.fn(),
        update: jest.fn(),
    },
};

const getAndValidateClientMock = jest
    .fn()
    .mockImplementation(() => Promise.resolve(mockClient));

jest.unstable_mockModule(
    "../../../../content-management/helpers/get-client.js",
    () => ({
        default: getAndValidateClientMock,
    })
);

const findAndDeleteMissingReferences = (
    await import(
        "../../../../content-management/changes/20241202-1200-find-and-delete-missing-references.js"
    )
).default;

describe("Find and Delete Missing References", () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    test("should fetch all content types and process each one", async () => {
        const mockContentTypes = {
            items: [{ sys: { id: "blogPost" } }, { sys: { id: "page" } }],
        };

        const mockEntries = {
            items: [
                {
                    sys: { id: "entry1" },
                    fields: {
                        title: { "en-US": "Test Entry" },
                    },
                },
            ],
        };

        mockClient.contentType.getMany.mockResolvedValue(mockContentTypes);
        mockClient.entry.getMany.mockResolvedValue(mockEntries);

        await findAndDeleteMissingReferences();

        expect(mockClient.contentType.getMany).toHaveBeenCalledTimes(1);
        expect(mockClient.entry.getMany).toHaveBeenCalledTimes(2);
    });

    test("should remove invalid references from single reference fields", async () => {
        const mockContentTypes = {
            items: [{ sys: { id: "blogPost" } }],
        };

        const mockEntry = {
            sys: {
                id: "entry1",
                contentType: { sys: { id: "blogPost" } },
            },
            fields: {
                reference: {
                    "en-US": {
                        sys: {
                            type: "Link",
                            id: "invalidId",
                        },
                    },
                },
            },
        };
        const mockEntries = {
            items: [mockEntry],
        };

        mockClient.contentType.getMany.mockResolvedValue(mockContentTypes);

        mockClient.entry.getMany.mockResolvedValue(mockEntries);

        await findAndDeleteMissingReferences();

        expect(mockClient.entry.update).toHaveBeenCalledWith(
            { entryId: mockEntry.sys.id },
            mockEntry
        );
    });

    test("should remove invalid references from array fields while keeping valid ones", async () => {
        const mockContentTypes = {
            items: [{ sys: { id: "blogPost" } }],
        };

        const validReference = {
            sys: {
                type: "Link",
                id: "validId",
            },
        };
        const erroredEntry = {
            sys: { id: "entry1" },
            fields: {
                references: {
                    "en-US": [
                        validReference,
                        {
                            sys: {
                                type: "Link",
                                id: "invalidId",
                            },
                        },
                    ],
                },
            },
        };

        const mockEntries = {
            items: [
                erroredEntry,
                {
                    sys: { id: "validId" },
                    fields: {},
                },
            ],
        };

        mockClient.contentType.getMany.mockResolvedValueOnce(mockContentTypes);
        mockClient.entry.getMany.mockResolvedValueOnce(mockEntries);

        await findAndDeleteMissingReferences();

        expect(mockClient.entry.update).toHaveBeenCalledWith(
            { entryId: erroredEntry.sys.id },
            {
                ...erroredEntry,
                fields: {
                    references: {
                        "en-US": [validReference],
                    },
                },
            }
        );
    });

    test("should handle non-reference fields without modification", async () => {
        const mockContentTypes = {
            items: [{ sys: { id: "blogPost" } }],
        };

        const mockEntries = {
            items: [
                {
                    sys: { id: "entry1" },
                    fields: {
                        title: { "en-US": "Test Title" },
                        content: { "en-US": "Test Content" },
                    },
                },
            ],
        };

        mockClient.contentType.getMany.mockResolvedValue(mockContentTypes);
        mockClient.entry.getMany.mockResolvedValueOnce(mockEntries);
        await findAndDeleteMissingReferences();

        expect(mockClient.entry.update).not.toHaveBeenCalled();
    });

    test("should handle errors gracefully", async () => {
        const mockContentTypes = {
            items: [{ sys: { id: "blogPost" } }],
        };

        mockClient.contentType.getMany.mockResolvedValue(mockContentTypes);
        mockClient.entry.getMany.mockRejectedValue(new Error("API Error"));

        await expect(findAndDeleteMissingReferences()).rejects.toThrow(
            "API Error"
        );
    });
});
