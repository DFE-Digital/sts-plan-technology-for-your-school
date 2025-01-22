import { jest } from "@jest/globals";

export default jest.fn().mockImplementation(() => Promise.resolve());
