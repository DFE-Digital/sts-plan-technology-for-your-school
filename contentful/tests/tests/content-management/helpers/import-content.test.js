import { jest } from "@jest/globals";

jest.mock("dotenv/config");
const existsSyncMock = jest.fn().mockImplementation(() => true);

jest.unstable_mockModule("fs", () => ({
    existsSync: existsSyncMock,
}));

const contentfulImportMock = jest
    .fn()
    .mockImplementation(() => Promise.resolve());
jest.unstable_mockModule("contentful-import", () => ({
    default: contentfulImportMock,
}));

const getAndValidateClientMock = jest
    .fn()
    .mockImplementation(() => Promise.resolve(true));
const getClientMock = jest.fn().mockImplementation(() => ({
    getAndValidateClient: getAndValidateClientMock,
}));

jest.unstable_mockModule(
    "../../../../content-management/helpers/get-client.js",
    getClientMock
);

const contentfulImport = (await import("contentful-import")).default;
const existsSync = (await import("fs")).existsSync;

const getAndValidateClient = (
    await import("content-management/helpers/get-client.js")
).getAndValidateClient;

const mockEnv = {
    CONTENT_FILE: "./test-content.json",
    SPACE_ID: "test-space-id",
    MANAGEMENT_TOKEN: "test-token",
    ENVIRONMENT: "test-env",
    SKIP_CONTENT_MODEL: "false",
};

const importContentfulData = (
    await import("content-management/helpers/import-content.js")
).default;

describe("importContentfulData", () => {
    let consoleLogSpy;
    let consoleErrorSpy;

    beforeEach(() => {
        jest.clearAllMocks();

        consoleLogSpy = jest.spyOn(console, "log").mockImplementation();
        consoleErrorSpy = jest.spyOn(console, "error").mockImplementation();

        process.env = { ...mockEnv };
    });

    afterEach(() => {
        consoleLogSpy.mockRestore();
        consoleErrorSpy.mockRestore();
    });

    it("should successfully import content when all parameters are valid", async () => {
        await importContentfulData();
        expect(getAndValidateClient).toHaveBeenCalled();

        expect(existsSync).toHaveBeenCalledWith(mockEnv.CONTENT_FILE);

        expect(contentfulImport).toHaveBeenCalledWith({
            contentFile: mockEnv.CONTENT_FILE,
            spaceId: mockEnv.SPACE_ID,
            managementToken: mockEnv.MANAGEMENT_TOKEN,
            environmentId: mockEnv.ENVIRONMENT,
            skipContentModel: false,
        });

        expect(consoleLogSpy).toHaveBeenCalledWith(
            `Import completed successfully from ${mockEnv.CONTENT_FILE}`
        );
    });

    it("should throw an error when content file does not exist", async () => {
        existsSyncMock.mockImplementationOnce(() => false);

        await expect(importContentfulData).rejects.toThrow(
            `File not found: ${mockEnv.CONTENT_FILE}`
        );
    });

    it("should handle contentful import errors", async () => {
        const mockError = new Error("Import failed");
        contentfulImport.mockImplementationOnce(() => {
            throw mockError;
        });

        await expect(importContentfulData).rejects.toThrow("Import failed");

        expect(consoleErrorSpy).toHaveBeenCalledWith(
            "Error during import:",
            mockError
        );
    });

    it("should handle environment validation errors", async () => {
        const validationError = new Error("Validation failed");
        getAndValidateClientMock.mockImplementationOnce(() => {
            throw validationError;
        });

        await expect(importContentfulData).rejects.toThrow("Validation failed");
    });

    it("should respect SKIP_CONTENT_MODEL environment variable", async () => {
        process.env.SKIP_CONTENT_MODEL = "true";

        await importContentfulData();

        expect(contentfulImport).toHaveBeenCalledWith(
            expect.objectContaining({
                skipContentModel: true,
            })
        );
    });
});
