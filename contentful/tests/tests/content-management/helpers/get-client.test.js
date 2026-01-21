import { jest } from "@jest/globals";

const mockGetEnvironments = jest.fn().mockResolvedValue({
    items: [
        { sys: { id: "master" } },
        { sys: { id: "test-env" } },
        { sys: { id: "staging" } }
    ]
});

const mockSpace = {
    getEnvironments: mockGetEnvironments
};

const mockGetSpace = jest.fn().mockResolvedValue(mockSpace);

jest.mock("contentful-management", () => ({
    createClient: jest.fn(() => ({
        getSpace: mockGetSpace
    }))
}));

const contentfulManagement = (await import("contentful-management")).default;

const { getAndValidateClient } = await import(
    "../../../../content-management/helpers/get-client"
);

describe("getAndValidateClient", () => {
    const originalEnv = process.env;

    beforeEach(() => {
        process.env = {
            ...originalEnv,
            MANAGEMENT_TOKEN: "test-token",
            SPACE_ID: "test-space",
            ENVIRONMENT: "test-env",
        };

        jest.clearAllMocks();
    });

    afterAll(() => {
        process.env = originalEnv;
    });

    it("should create and return a valid client when environment is valid", async () => {
        const client = await getAndValidateClient();

        expect(contentfulManagement.createClient).toHaveBeenCalledWith(
            {
                accessToken: "test-token",
            }
        );

        expect(mockGetEnvironments).toHaveBeenCalledTimes(1);
        expect(mockGetSpace).toHaveBeenCalledTimes(1);

        expect(client).toBe(mockClient);
    });

    it("should throw an error when environment is invalid", async () => {
        process.env = {
            ...originalEnv,
            MANAGEMENT_TOKEN: "test-token",
            SPACE_ID: "not-a-real-space",
            ENVIRONMENT: "not-a-real-env",
        };

        await expect(getAndValidateClient()).rejects.toThrow(
            "Invalid Contentful environment"
        );
    });

    it("should throw an error when environment variables are missing", async () => {
        delete process.env.MANAGEMENT_TOKEN;
        delete process.env.SPACE_ID;
        delete process.env.ENVIRONMENT;

        await expect(getAndValidateClient()).rejects.toThrow();
    });
});
