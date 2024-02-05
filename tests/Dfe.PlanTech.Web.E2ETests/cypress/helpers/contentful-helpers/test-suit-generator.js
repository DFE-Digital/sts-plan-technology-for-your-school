import fs from "fs";
import DataMapper from "./data-mapper";

const { file, writeAllPossiblePaths } = getArguments();

const dataMapper = new DataMapper(file);

for (const section of dataMapper.mappedSections) {
  const output = dataMapper.convertToMinimalSectionInfo(
    section,
    writeAllPossiblePaths
  );
  const json = JSON.stringify(output);

  fs.writeFileSync(`./output/${section.name}.json`, json);
}

/**
 * Retrieves the command line arguments and returns an object containing the file path
 * and a boolean indicating whether to write all possible paths.
 *
 * @return {Object} Object with file path and boolean for writing all possible paths
 */
function getArguments() {
  const processArgs = process.argv.slice(2);

  if (processArgs.length == 0) {
    console.error("Please provide a file path as the first argument");
    process.exit(1);
  }

  return {
    file: processArgs[0],
    writeAllPossiblePaths: processArgs.length > 1 && processArgs[1] == "true",
  };
}
