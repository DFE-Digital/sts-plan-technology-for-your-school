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
    .option('--management-token <management-token>', "Contentful management token.")
    .option("--save-file <true/false>", "Whether to save file or not. Defaults to true")
    .option("--use-preview <true/false>", "Whether to export preview content or not. Defaults to false");

  program.option('-e, --export [export...]', `What stuff to export from Contentful; this HAS to be the last option provided. Options are 'content', 'contentmodel', 'webhooks', 'roles', 'editorinterfaces'. Defaults to content and contentmodel. Specify '<TYPE>=<true/false> or just <TYPE> for true. E.g. --export "content=false contentmodel=false webhooks" would result in content and content model not being exported, but would export webhooks.`);

  program.parse();

  return program.opts();
};

/**
 * Get the "export" argument value from the CLI; what data (e.g. content, contentmodel, etc.) to export/not export from Contentful
 * @param {Object} args 
 * @returns {Object | undefined} What data to export from Contentful
 */
const getExportOptions = (args) => {
  if (!args.export) {
    return;
  }
  const exportOptions = {
    content: true,
    contentmodel: true,
    webhooks: false
  };

  for (const option of args.export) {
    if (!option.includes("=")) {
      exportOptions[option] = true;
      continue;
    }

    const [whatToExport, shouldExport] = option.split("=");

    if (shouldExport != "true" && shouldExport != "false") {
      console.error(`Invalid option "${option}". Expected "true" or "false".`);
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

  const getOptionValue = (optionKey, environmentKey, defaultValue, isBoolean) => getValueFromOptionsOrEnv(args, optionKey, environmentKey, defaultValue, isBoolean);

  const options = {
    generateTestSuites: getOptionValue("testSuites", "GENERATE_TEST_SUITES", false, true),
    exportUserJourneyPaths: getOptionValue("exportUserJourneys", "EXPORT_USER_JOURNEY_PATHS", false, true),
    outputDir: getOptionValue("outputDir", "OUTPUT_FILE_DIR", DefaultOutputDirectory, false),
    saveAllJourneys: getOptionValue("saveAllJourneys", "EXPORT_ALL_PATHS", false, true),
    exportOptions: getExportOptions(args),
    spaceId: args.spaceId ?? process.env.SPACE_ID,
    deliveryToken: args.deliveryToken ?? process.env.DELIVERY_TOKEN,
    managementToken: args.managementToken ?? process.env.MANAGEMENT_TOKEN,
    environment: args.environment ?? process.env.ENV ?? process.env.ENVIRONMENT ?? "master",
    saveFile: getOptionValue("saveFile", "SAVE_FILE", true, true),
    usePreview: getOptionValue("usePreview", "USE_PREVIEW", false, true),
  };

  options.exportDirectory = getAndCreateOutputDir(options);
  options.outputDir = options.exportDirectory;;
  return options;
};

/**
 * Creats an output directory for the export, using the Contentful environment, the current time, and the specified parent output directory.
 */
const getOutputDirPath = ({ outputDir, environment }) => {
  const now = new Date().toISOString().replaceAll(":", "").replaceAll("-", "").replace("T", "").split(".")[0];

  const subDirectory = `${outputDir}${environment}-${now}/`;

  const currentDirectory = process.cwd();

  return path.join(currentDirectory, subDirectory);
};


/**
 * Get and create the output directory if it doesn't exist
 * @param {Object} params
 * @param {string} params.outputDir - Base output directory
 * @param {string} params.environment - Contentful environment; used as part of path
 * @returns {string} The final output directory path
 */
const getAndCreateOutputDir = ({ outputDir, environment }) => {
  const path = getOutputDirPath({ outputDir, environment });

  createFileDirectoryIfNonExistent(path);

  return path;
};


/**
 * Create the file directory if it doesn't exist
 * @param {string} outputDir - The directory to be created
 */
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
 * Get argument value from options or environment
 * @param {object} obj - Object containing arguments (e.g. parsed CLI arguments or process.env)
 * @param {string} key - The key of the option
 * @param {boolean} isBoolean - Whether the value should be treated as a boolean
 * @returns {*} Value based on the options or environment, or undefined if not found
 */
const getArgumentValue = (obj, key, isBoolean) => {
  if (!isBoolean) {
    return obj[key];
  }

  if (obj[key] === undefined || obj[key] === null) {
    return;
  }

  return obj[key] === 'true' || obj[key] === true;
};

/**
 * Get value from options or environment
 * @param {Object} options - Parsed CLI arguments
 * @param {string} optionsKey - Key in the options object
 * @param {string} envKey - Environment variable key
 * @param {*} defaultValue - Default value if not found
 * @param {boolean} isBoolean - Whether to treat the value as a boolean
 * @returns {*} The final value, either from options or environment
 */
const getValueFromOptionsOrEnv = (options, optionsKey, envKey, defaultValue, isBoolean) => {
  const optionsValue = getArgumentValue(options, optionsKey, isBoolean);

  if (optionsValue !== undefined) {
    return optionsValue;
  }

  const environmentVariableValue = getArgumentValue(process.env, envKey, isBoolean);

  if (environmentVariableValue !== undefined) {
    return environmentVariableValue;
  }

  return defaultValue;
};

const options = getOptions();

export {
  options,
  DefaultOutputDirectory,
  getArgumentValue
}

