import "dotenv/config";
import DataMapper from "./data-mapper.js";
import ExportContentfulData from "./exporter.js.js";
import GenerateTestSuites from "./generate-test-suites.js.js";
import ErrorLogger from "./errors/error-logger.js";
import WriteUserJourneyPaths from "./write-user-journey-paths.js";
import fs from "fs";

const DefaultOutputDirectory = "./output/";
const getOutputDirWithContentfulEnv = ({ outputDir, contentfulEnvironment }) => `${outputDir}${contentfulEnvironment}/`;

async function processContentfulData(args) {
  if (!args.outputDir) {
    args.outputDir = DefaultOutputDirectory;
  }

  if (!args.generateTestSuites && !args.exportUserJourneyPaths) {
    console.error(`No options have been enabled`);
    return;
  }

  const contentfulData = await ExportContentfulData();

  const dataMapper = new DataMapper(contentfulData);

  const combinedOutputDir = getOutputDirWithContentfulEnv({ contentfulEnvironment: process.env.ENVIRONMENT, outputDir: args.outputDir });

  createFileDirectoryIfNonExistent(combinedOutputDir);

  if (args.generateTestSuites) {
    GenerateTestSuites({ dataMapper, outputDir: combinedOutputDir });
  }

  if (args.exportUserJourneyPaths) {
    WriteUserJourneyPaths({ dataMapper, outputDir: combinedOutputDir, saveAllJourneys: args.saveAllJourneys });
  }

  ErrorLogger.outputDir = combinedOutputDir;
  ErrorLogger.writeErrorsToFile();
}

function createFileDirectoryIfNonExistent(outputDir) {
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
};

await processContentfulData(options);