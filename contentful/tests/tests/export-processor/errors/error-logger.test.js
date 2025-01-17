import { ErrorLoggerClass as ErrorLogger } from "../../../../export-processor/errors/error-logger.js";
import { jest } from "@jest/globals";

import fs from "fs";
jest.spyOn(fs, "writeFileSync").mockImplementation((func) => {});

describe("ErrorLogger", () => {
    let errorLogger;

    beforeEach(() => {
        errorLogger = new ErrorLogger();
        jest.clearAllMocks();
    });

    describe("addError", () => {
        it("should add an error to the errors array", () => {
            errorLogger.addError({
                id: "123",
                contentType: "article",
                message: "Missing title",
            });

            expect(errorLogger.errors).toHaveLength(1);
            expect(errorLogger.errors[0]).toMatchObject({
                id: "123",
                contentType: "article",
                message: "Missing title",
            });
        });

        it("should log error to console", () => {
            console.error = jest.fn();

            errorLogger.addError({
                id: "123",
                contentType: "article",
                message: "Missing title",
            });

            expect(console.error).toHaveBeenCalledWith(
                "article error for content 123: Missing title"
            );
        });
    });

    describe("writeErrorsToFile", () => {
        it("should write formatted errors to file", () => {
            const errors = [
                {
                    id: "123",
                    contentType: "article",
                    message: "Missing title",
                },
                {
                    id: "456",
                    contentType: "article",
                    message: "Invalid date",
                },
                {
                    id: "789",
                    contentType: "video",
                    message: "Missing duration",
                },
            ];

            for (const error of errors) {
                errorLogger.addError(error);
            }

            errorLogger.writeErrorsToFile();

            const expectedContent = `# Content Errors:

## article:

| Id | Message |
| -- | ------- |
| 123 | Missing title |
| 456 | Invalid date |

## video:

| Id | Message |
| -- | ------- |
| 789 | Missing duration |`;

            const writeFileCalls = fs.writeFileSync.mock.calls;

            expect(writeFileCalls.length).toEqual(1);

            const call = writeFileCalls[0];

            expect(call[0]).toEqual("content-errors.md");

            const content = call[1].split("\n").map(line => line.trim()).join("\n");

            expect(content).toEqual(expectedContent);
        });

        it("should use custom file path when provided", () => {
            errorLogger.addError({
                id: "123",
                contentType: "article",
                message: "Missing title",
            });

            errorLogger.writeErrorsToFile("custom-path.md");

            expect(fs.writeFileSync).toHaveBeenCalledWith(
                "custom-path.md",
                expect.any(String)
            );
        });

        it("should handle empty errors array", () => {
            errorLogger.writeErrorsToFile();

            expect(fs.writeFileSync).toHaveBeenCalledWith(
                "content-errors.md",
                "# Content Errors: \n\n"
            );
        });
    });

    describe("groupBy", () => {
        it("should group array items by key", () => {
            const items = [
                { type: "a", value: 1 },
                { type: "b", value: 2 },
                { type: "a", value: 3 },
            ];

            const result = errorLogger.groupBy(items, "type");

            expect(result).toEqual({
                a: [
                    { type: "a", value: 1 },
                    { type: "a", value: 3 },
                ],
                b: [{ type: "b", value: 2 }],
            });
        });
    });
});
