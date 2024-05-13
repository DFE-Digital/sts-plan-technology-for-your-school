import "dotenv/config";
import DataMapper from "#src/data-mapper";
import ExportContentfulData from "./exporter.js";
import GenerateTestSuites from "./generate-test-suites.js";
import ErrorLogger from "#src/errors/error-logger";
import WriteUserJourneyPaths from "#src/write-user-journey-paths";
import fs from "fs";

const DefaultOutputDirectory = "./output/";

async function processContentfulData(args) {
  if (!args.generateTestSuites && !args.exportUserJourneyPaths) {
    console.error(`No options have been enabled`);
    return;
  }

  const contentfulData = await ExportContentfulData();

  const dataMapper = new DataMapper(contentfulData);

  createFileDirectoryIfNonExistent(args);

  if (args.generateTestSuites) {
    GenerateTestSuites({ dataMapper, outputDir: args.outputDir });
  }

  if (args.exportUserJourneyPaths) {
    WriteUserJourneyPaths({ dataMapper, outputDir: args.outputDir, saveAllJourneys: args.saveAllJourneys });
  }

  ErrorLogger.outputDir = args.outputDir;
  ErrorLogger.writeErrorsToFile();
}

function createFileDirectoryIfNonExistent({ outputDir }) {
  if (fs.existsSync(outputDir)) {
    return outputDir;
  }

  try {
    fs.mkdirSync(outputDir, { recursive: true });
    return outputDir;
  }
  catch (e) {
    console.error(`Error creating directory ${outputDir}. Defaulting to ${DefaultOutputDirectory}`, e);
    return DefaultOutputDirectory;
  }
}

const options = {
  generateTestSuites: process.env.GENERATE_TEST_SUITES === "true",
  exportUserJourneyPaths: process.env.EXPORT_USER_JOURNEY_PATHS === 'true',
  outputDir: process.env.OUTPUT_FILE_DIR ?? DefaultOutputDirectory,
  saveAllJourneys: process.env.EXPORT_ALL_PATHS === "true" || process.env.EXPORT_ALL_PATHS === true
}

await processContentfulData(options);