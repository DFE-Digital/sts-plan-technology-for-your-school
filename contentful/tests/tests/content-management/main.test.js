import { jest } from "@jest/globals";
import path from "path";
import { fileURLToPath } from "url";

const existsSyncMock = jest.fn().mockImplementation(() => true);

jest.unstable_mockModule("fs", () => ({
    existsSync: existsSyncMock,
}));

let mockTestChange;
let main;
let filePath;
let __filename;
let __dirname;

beforeAll(async () => {
    __filename = fileURLToPath(import.meta.url);
    __dirname = path.dirname(__filename);

    const testFileName = "test-change.js";
    filePath = path.join(
        __dirname,
        "..",
        "..",
        "..",
        "content-management",
        "changes",
        testFileName
    );

    jest.unstable_mockModule(filePath, () => ({
        default: jest.fn()
    }));

    const mod1 = await import(filePath);
    mockTestChange = mod1.default;

    const mod2 = await import("../../../content-management/main.js");
    main = mod2.main;
});

describe.skip("main.js", () => {
    it("should handle invalid filename", async () => {
        existsSyncMock.mockImplementation(() => false);
        const consoleSpy = jest.spyOn(console, "error").mockImplementation(() => {});

        const originalArgv = process.argv;
        process.argv = ["node", "main.js"]

        await main();

        process.argv = originalArgv;

        expect(consoleSpy).toHaveBeenCalledWith("Invalid filename provided");
        expect(consoleSpy).toHaveBeenCalledTimes(1);
    });

    it("should execute change function when valid file is provided", async () => {
        existsSyncMock.mockImplementation(() => true);

        const originalArgv = process.argv;
        process.argv = ["node", "main.js", "test-change.js"];

        try {
            await main();

            const expectedPath = path.join(
                __dirname,
                "..",
                "..",
                "..",
                "content-management",
                "changes",
                "test-change.js"
            );

            expect(existsSyncMock).toHaveBeenCalledWith(expectedPath);
            expect(mockTestChange).toHaveBeenCalled();
        } finally {
            process.argv = originalArgv;
        }
    });
});
