import fs from "fs";
import DataMapper from "./data-mapper.js";

/**
 * @param {object} args
 * @param {DataMapper} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 * @param {boolean} args.saveAllJourneys - if true saves _all_ user journeys, otherwise writes 1 (or more if needed) to navigate through every question, and 1 per maturity
 */
export default function writeUserJourneyPaths({ dataMapper, outputDir, saveAllJourneys }) {
  for (const section of dataMapper.mappedSections) {
    const output = dataMapper.convertToMinimalSectionInfo(
      section,
      saveAllJourneys
    );

    const json = JSON.stringify(output);

    fs.writeFileSync(`${outputDir}${section.name}.json`, json);
  }
}