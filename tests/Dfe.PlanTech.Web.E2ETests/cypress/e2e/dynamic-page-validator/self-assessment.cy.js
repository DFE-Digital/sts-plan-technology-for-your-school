import DataMapper from "export-processor/data-mapper.js";
import { validateSelfAssessmentPage } from "./validators/index.js";


const dataMapper = new DataMapper(require('../../fixtures/contentful-data'));

describe("Self assessment page", () => {

    it("Should validate self-assessment page", () => {
        validateSelfAssessmentPage(dataMapper);
    });

});
