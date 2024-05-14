import fs from "fs";
import DataMapper from "./data-mapper.js";

/**
 * @param {object} args
 * @param {DataMapper} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 * @param {boolean} args.saveAllJourneys - if true saves _all_ user journeys, otherwise writes 1 (or more if needed) to navigate through every question, and 1 per maturity
 */
export default function writeUserJourneyPaths({ dataMapper, outputDir, saveAllJourneys }) {
  const journeys = dataMapper.mappedSections.map(section => dataMapper.convertToMinimalSectionInfo(section, saveAllJourneys));

  for (const journey of journeys) {
    fs.writeFileSync(`${outputDir}${journey.section}.json`, JSON.stringify(journey));
  }

  const combined = journeys.map(journey => ({
    subtopic: journey.section,
    stats: journey.allPathsStats,
  }));

  fs.writeFileSync(`${outputDir}subtopic-paths-overview.json`, JSON.stringify(combined));
}