import "dotenv/config";
import { program } from 'commander';

const DefaultOutputDirectory = "./output/";

/**
 * Get CLI arguments
 */
const getCliArgs = () => {
  program
    .option('-ts, --test-suites <true/false>', "Generate test suites.")
    .option('-uj, --export-user-journeys <true/false>', "Export user journeys for subtopics.")
    .option('-o, --output-dir <path>', "Relative path for saving files. E.g. ./output")
    .option('-s, --save-all-journeys <true/false>', "Save all user journeys..");

  program.parse();

  return program.opts();
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
    saveAllJourneys: saveAllJourneys != undefined ? saveAllJourneys : process.env.EXPORT_ALL_PATHS === "true" || process.env.EXPORT_ALL_PATHS === true
  };

  return options;
};

/**
 * Gets value from CLI arguments 
 * @param {OptionValues} args Parsed arguments from Commander
 * @param {string} prop Name of the argument/property to retrieve
 * @param {boolean} isBoolean Whether the argument is expected to be a boolean or not
 * @returns {*} The property value if not a boolean, otherwise undefined if the property is undefined, otherwise the boolean value
 */
const getArgumentValue = (args, prop, isBoolean) => {
  console.log(prop, args[prop]);
  if (!isBoolean) {
    return args[prop];
  }

  if (args[prop] === undefined || args[prop] === null) {
    return;
  }

  return args[prop] === true || args[prop] === 'true';
};


const options = getOptions();

export default { options, DefaultOutputDirectory }
