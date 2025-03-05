import { runForEachContentType, AllContentTypes } from "../../../content-migrations/helpers/content-types"
import { jest } from "@jest/globals";

describe("content-types.js", () => {
  test("AllContentTypes should be an object", () => {
    expect(typeof AllContentTypes).toBe("object");
  });

  test("runForEachContentType should call the callback for each content type", () => {
    const mockCallback = jest.fn();
    runForEachContentType(mockCallback);
    expect(mockCallback).toHaveBeenCalledTimes(Object.keys(AllContentTypes).length);
  });
});