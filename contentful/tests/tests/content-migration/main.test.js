import { jest } from "@jest/globals";
import path from "path";
import { fileURLToPath } from "url";

const existsSyncMock = jest.fn().mockImplementation(() => true);
jest.unstable_mockModule("fs", () => ({
    existsSync: existsSyncMock,
}));

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const testFileName = "test-change.js";
const filePath = path.join(
    __dirname,
    "..",
    "..",
    "..",
    "content-management",
    "changes",
    testFileName
);

const { default: mockTestChange } = await import(filePath);
const main = (await import("../../../content-management/main.js")).main;

describe.skip("main.js", () => {
    it("should handle invalid filename", async () => {
        existsSyncMock.mockImplementation(() => false);
        const consoleSpy = jest.spyOn(console, "error");

        await main();

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
