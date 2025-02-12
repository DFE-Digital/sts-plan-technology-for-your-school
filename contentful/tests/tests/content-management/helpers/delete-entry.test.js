import { jest } from "@jest/globals";

const getAndValidateClientMock = {
    entry: {
        unpublish: jest.fn().mockImplementation(() => Promise.resolve()),
        delete: jest.fn().mockImplementation(() => Promise.resolve()),
    },
};

const getClientMock = jest
    .fn()
    .mockImplementation(() => Promise.resolve(getAndValidateClientMock));

jest.unstable_mockModule(
    "../../../../content-management/helpers/get-client.js",
    () => {
        return {
            getAndValidateClient: getClientMock,
            default: getClientMock,
        };
    }
);

const getAndValidateClient = (
    await import("../../../../content-management/helpers/get-client.js")
).getAndValidateClient;

const deleteEntry = (
    await import("../../../../content-management/helpers/delete-entry.js")
).default;

describe("deleteEntry", () => {
    let mockClient;

    beforeEach(async () => {
        jest.clearAllMocks();
        mockClient = await getAndValidateClient();
    });

    it("should unpublish and delete a published entry", async () => {
        const publishedEntry = {
            sys: {
                id: "test-entry-1",
                publishedVersion: 1,
            },
        };

        await deleteEntry(publishedEntry);

        expect(mockClient.entry.unpublish).toHaveBeenCalledWith({
            entryId: "test-entry-1",
        });
        expect(mockClient.entry.delete).toHaveBeenCalledWith({
            entryId: "test-entry-1",
        });
    });

    it("should delete an unpublished entry without unpublishing", async () => {
        // Arrange
        const unpublishedEntry = {
            sys: {
                id: "test-entry-2",
                publishedVersion: null,
            },
        };

        await deleteEntry(unpublishedEntry);

        expect(mockClient.entry.unpublish).not.toHaveBeenCalled();
        expect(mockClient.entry.delete).toHaveBeenCalledWith({
            entryId: "test-entry-2",
        });
    });

    it("should handle errors during unpublishing", async () => {
        // Arrange
        const publishedEntry = {
            sys: {
                id: "test-entry-3",
                publishedVersion: 1,
            },
        };

        mockClient.entry.unpublish.mockImplementationOnce(() =>
            Promise.reject(new Error("Unpublish failed"))
        );

        await expect(deleteEntry(publishedEntry)).rejects.toThrow(
            "Unpublish failed"
        );
        expect(mockClient.entry.delete).not.toHaveBeenCalled();
    });

    it("should handle errors during deletion", async () => {
        const entry = {
            sys: {
                id: "test-entry-4",
                publishedVersion: null,
            },
        };

        mockClient.entry.delete.mockImplementationOnce(() =>
            Promise.reject(new Error("Delete failed"))
        );

        await expect(deleteEntry(entry)).rejects.toThrow("Delete failed");
    });
});
