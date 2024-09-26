import fs from "fs";
// eslint-disable-next-line no-unused-vars
import DataMapper from "./data-mapper.js";
import path from "path";
import { stringify } from "flatted";

/**
 * @param {object} args
 * @param {DataMapper} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 * @param {boolean} args.saveAllJourneys - if true saves _all_ user journeys, otherwise writes 1 (or more if needed) to navigate through every question, and 1 per maturity
 */
export default function writeUserJourneyPaths({ dataMapper, outputDir, saveAllJourneys }) {
  outputDir = createUserJourneyDirectory(outputDir);

  const journeys = mapJourneysToMinimalSectionInfo(dataMapper, saveAllJourneys);

  saveUserJourneys(journeys, outputDir);
  saveSubtopicPathsOverview(journeys, outputDir);
}

const mapJourneysToMinimalSectionInfo = (dataMapper, saveAllJourneys) => dataMapper.mappedSections.map(section => dataMapper.convertToMinimalSectionInfo(section, saveAllJourneys));

const saveSubtopicPathsOverview = (journeys, outputDir) => {
  const combined = journeys.map(journey => ({
    subtopic: journey.section,
    stats: journey.allPathsStats,
  }));

  try {
    fs.writeFileSync(`${outputDir}subtopic-paths-overview.json`, JSON.stringify(combined));
  }
  catch (e) {
    console.error(`Error saving subtopic paths overview`, e);
  }
};

const saveUserJourneys = (journeys, outputDir) => {
  for (const journey of journeys) {
    const fileName = journey.section + ".json";
    const filePath = path.join(outputDir, fileName);

    try {
      fs.writeFileSync(filePath, JSON.stringify(journey));
    }
    catch (e) {
      console.error(`Error saving user journeys for ${journey.section}. Will try flatted`, e);
      fs.writeFileSync(filePath, stringify(journey));
    }
  }
};

const createUserJourneyDirectory = (outputDir) => {
  const userJourneyDirectory = path.join(outputDir, "user-journeys");

  try {
    fs.mkdirSync(userJourneyDirectory);
    return userJourneyDirectory;
  }
  catch (e) {
    console.error(`Error creating user journey directory ${userJourneyDirectory}`, e);
    return outputDir;
  }
};
