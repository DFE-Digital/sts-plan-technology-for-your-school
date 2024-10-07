import DataMapper from "./data-mapper.js";
import ExportContentfulData from "./exporter.js";
import GenerateTestSuites from "./generate-test-suites.js";
import ErrorLogger from "./errors/error-logger.js";
import WriteUserJourneyPaths from "./write-user-journey-paths.js";
import fs from "fs";
import { options, DefaultOutputDirectory } from "./options.js";
import path from "path";

const getOutputDirPath = ({ outputDir, contentfulEnvironment }) => {
  const now = new Date().toISOString().replaceAll(":", "").replaceAll("-", "").replace("T", "").split(".")[0];

  const subDirectory = `${outputDir}${contentfulEnvironment}-${now}/`;

  const currentDirectory = process.cwd();

  return path.join(currentDirectory, subDirectory);
};

const getAndCreateOutputDir = ({ outputDir, contentfulEnvironment }) => {
  const path = getOutputDirPath({ outputDir, contentfulEnvironment });

  createFileDirectoryIfNonExistent(path);

  return path;
};

async function processContentfulData(args) {
  if (!args.generateTestSuites && !args.exportUserJourneyPaths) {
    console.error(`No options have been enabled`);
    return;
  }

  const combinedOutputDir = getAndCreateOutputDir({ contentfulEnvironment: process.env.ENVIRONMENT, outputDir: args.outputDir });

  const contentfulData = await ExportContentfulData({ exportDirectory: combinedOutputDir });

  const dataMapper = new DataMapper(contentfulData);

  if (args.generateTestSuites) {
    GenerateTestSuites({ dataMapper, outputDir: combinedOutputDir });
  }

  if (!!args.exportUserJourneyPaths) {
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

await processContentfulData(options);