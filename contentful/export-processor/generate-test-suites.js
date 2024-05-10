import contentfulExport from "contentful-export";
import "dotenv/config";
import DataMapper from "#src/data-mapper";
import TestSuite from "#src/test-suite/test-suite";
import WriteCsv from "./write-csv.js";

const options = {
  spaceId: process.env.SPACE_ID,
  deliveryToken: process.env.DELIVERY_TOKEN,
  managementToken: process.env.MANAGEMENT_TOKEN,
  environmentId: process.env.ENVIRONMENT,
  host: "api.contentful.com",
  skipEditorInterfaces: true,
  skipRoles: true,
  skipWebhooks: true,
  saveFile: false,
};

const exportedData = await contentfulExport(options);

const mapped = new DataMapper(exportedData);

let index = 1;

const testSuites = Object.values(mapped.mappedSections).filter(section => !!section).map((section) => {
  const testSuite = new TestSuite({
    subtopic: section,
    testReferenceIndex: index,
  });

  index = testSuite.testReferenceIndex;

  return testSuite;
});

WriteCsv(testSuites);
