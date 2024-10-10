import "dotenv/config";
import { program } from 'commander';
import fs from "fs";
import path from "path";

const DefaultOutputDirectory = "./output/";

/**
 * Get CLI arguments
 */
const getCliArgs = () => {
  program
    .option('-ts, --test-suites <true/false>', "Generate test suites.")
    .option('-uj, --export-user-journeys <true/false>', "Export user journeys for subtopics.")
    .option('-o, --output-dir <path>', "Relative path for saving files. E.g. ./output")
    .option('-s, --save-all-journeys <true/false>', "Save all user journeys..")
    .option('--environment <environment>', "Contentful environment.")
    .option('--space-id <space-id>', "Contentful space id.")
    .option('--delivery-token <delivery-token>', "Contentful delivery token.")
    .option('--management-token <management-token>', "Contentful management token.");

  program.option('-e --export [name..]',
    `What stuff to export from Contentful. Options are 'content', 'contentmodel', 'webhooks', 'roles', 'editorinterfaces'. Defaults to content and contentmodel. E.g. --export "content=false contentmodel=false"`);

  program.parse();

  return program.opts();
};

const getExportOptions = (args) => {
  if (!args.export) {
    return;
  }
  const exportOptions = {
    content: true,
    contentmodel: true,
    webhooks: false
  };

  const exportArguments = args.export.indexOf(" ") > -1 ? args.export.split(" ") : [args.export];

  for (const option of exportArguments) {
    if (option.indexOf("=") == -1) {
      console.log("Option was not supplied correctly. Must be in format whattoexport=<boolean, e.g. content=true", option);
      continue;
    }

    const [whatToExport, shouldExport] = option.split("=");

    if (shouldExport != "true" && shouldExport != "false") {
      continue;
    }

    exportOptions[whatToExport] = shouldExport == 'true';
  }

  return exportOptions;
};

/**
 * Gets the options values from the CLI if existing, otherwise the environment values.
 */
const getOptions = () => {
  const args = getCliArgs();

  const generateTestSuites = getArgumentValue(args, 'testSuites', true);
  const exportUserJourneyPaths = getArgumentValue(args, 'exportUserJourneys', true);
  const outputDir = getArgumentValue(args, 'outputDir');
  const saveAllJourneys = getArgumentValue(args, 'saveAllJourneys', true);

  const options = {
    generateTestSuites: generateTestSuites != undefined ? generateTestSuites : process.env.GENERATE_TEST_SUITES === "true",
    exportUserJourneyPaths: exportUserJourneyPaths != undefined ? exportUserJourneyPaths : process.env.EXPORT_USER_JOURNEY_PATHS === 'true',
    outputDir: outputDir ?? process.env.OUTPUT_FILE_DIR ?? DefaultOutputDirectory,
    saveAllJourneys: saveAllJourneys != undefined ? saveAllJourneys : process.env.EXPORT_ALL_PATHS === "true" || process.env.EXPORT_ALL_PATHS === true,
    exportOptions: getExportOptions(args),
    spaceId: args.spceId ?? process.env.SPACE_ID,
    deliveryToken: args.deliveryToken ?? process.env.DELIVERY_TOKEN,
    managementToken: args.managementToken ?? process.env.MANAGEMENT_TOKEN,
    environment: args.environment ?? process.env.ENV ?? "master"
  };

  options.exportDirectory = getAndCreateOutputDir(options);
  console.log('options', options);
  return options;
};

const getOutputDirPath = ({ outputDir, environment }) => {
  const now = new Date().toISOString().replaceAll(":", "").replaceAll("-", "").replace("T", "").split(".")[0];

  const subDirectory = `${outputDir}${environment}-${now}/`;

  const currentDirectory = process.cwd();

  return path.join(currentDirectory, subDirectory);
};

const getAndCreateOutputDir = ({ outputDir, environment }) => {
  const path = getOutputDirPath({ outputDir, environment });

  createFileDirectoryIfNonExistent(path);

  return path;
};

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

/**
 * Gets value from CLI arguments 
 * @param {OptionValues} args Parsed arguments from Commander
 * @param {string} prop Name of the argument/property to retrieve
 * @param {boolean} isBoolean Whether the argument is expected to be a boolean or not
 * @returns {*} The property value if not a boolean, otherwise undefined if the property is undefined, otherwise the boolean value
 */
const getArgumentValue = (args, prop, isBoolean) => {
  if (!isBoolean) {
    return args[prop];
  }

  if (args[prop] === undefined || args[prop] === null) {
    return;
  }

  return args[prop] === true || args[prop] === 'true';
};

const options = getOptions();

export {
  options,
  DefaultOutputDirectory
}

