import DataMapper from "./data-mapper.js";
import ExportContentfulData from "./exporter.js";
import GenerateTestSuites from "./generate-test-suites.js";
import ErrorLogger from "./errors/error-logger.js";
import WriteUserJourneyPaths from "./write-user-journey-paths.js";
import { options } from "./options.js";

async function processContentfulData(args) {
  const contentfulData = await ExportContentfulData({ ...args });

  if (!args.generateTestSuites && !args.exportUserJourneyPaths && !args.exportUserJourneyPaths) {
    console.log(`No options set for using data - ending run`);
    return;
  }

  if (!contentfulData.entries || !contentfulData.contentTypes) {
    console.error('Missing entries or content types');
    return;
  }

  const dataMapper = new DataMapper(contentfulData);

  const outputDir = args.outputDir;

  if (args.generateTestSuites) {
    GenerateTestSuites({ dataMapper, outputDir });
  }

  if (!!args.exportUserJourneyPaths) {
    WriteUserJourneyPaths({ dataMapper, outputDir, saveAllJourneys: args.saveAllJourneys });
  }

  ErrorLogger.outputDir = outputDir;
  ErrorLogger.writeErrorsToFile();
}

await processContentfulData(options);