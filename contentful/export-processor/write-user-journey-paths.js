import fs from "fs";
// eslint-disable-next-line no-unused-vars
import DataMapper from "./data-mapper.js";
import path from "path";
import { stringify } from "#src/helpers/json";
import { log } from "#src/helpers/log";

/**
 * @typedef {import("#src/content-types/section").SectionMinimalOutput} SectionMinimalOutput
 */

/**
 * @param {object} args
 * @param {DataMapper} args.dataMapper - created and initialised DataMapper
 * @param {string} args.outputDir - directory to write journey paths to
 * @param {boolean} args.saveAllJourneys - if true saves _all_ user journeys, otherwise writes 1 (or more if needed) to navigate through every question, and 1 per maturity
 */
export default function writeUserJourneyPaths({
    dataMapper,
    outputDir,
    saveAllJourneys,
}) {
    log(`Writing all user journey paths`);

    outputDir = createUserJourneyDirectory(outputDir);

    const journeys = mapJourneysToMinimalSectionInfo(
        dataMapper,
        saveAllJourneys
    );

    saveUserJourneys(journeys, outputDir);
    saveSubtopicPathsOverview(journeys, outputDir);

    log(`Finished writing all user journey paths`);
}

/**
 *
 * @param {DataMapper} dataMapper
 * @param {boolean} saveAllJourneys
 * @returns {SectionMinimalOutput[]}
 */
const mapJourneysToMinimalSectionInfo = (dataMapper, saveAllJourneys) =>
    dataMapper.mappedSections.map((section) =>
        section.toMinimalOutput(saveAllJourneys)
    );

/**
 *
 * @param {SectionMinimalOutput[]} journeys
 * @param {string} outputDir
 */
const saveSubtopicPathsOverview = (journeys, outputDir) => {
    log(`Writing all subtopic paths overview`, { addBlankLine: false });

    const combined = journeys.map((journey) => ({
        subtopic: journey.section,
        stats: journey.allPathsStats,
    }));

    try {
        fs.writeFileSync(
            `${outputDir}subtopic-paths-overview.json`,
            stringify(combined)
        );
    } catch (e) {
        console.error(`Error saving subtopic paths overview`, e);
    }

    log(`Wrote all subtopic paths overview`);
};

/**
 *
 * @param {SectionMinimalOutput[]} journeys
 * @param {string} outputDir
 */
const saveUserJourneys = (journeys, outputDir) => {
    log(`Writing user journey path files`);
    for (const journey of journeys) {
        const fileName = journey.section + ".json";
        const filePath = path.join(outputDir, fileName);

        saveUserJourney(journey, filePath);
    }

    log(`Wrote all user journey paths`);
};

/**
 *
 * @param {SectionMinimalOutput} journey
 * @param {string} filePath
 */
const saveUserJourney = (journey, filePath) => {
    log(`Writing user journey path file for ${journey.section}`);

    try {
        fs.writeFileSync(filePath, stringify(journey));
    } catch (e) {
        console.error(`Error saving user journeys for ${journey.section}`, e);

        if (!journey.allPossiblePaths || journey.allPossiblePaths.length == 0) {
            return;
        }
        log(
            `Will clearing all possible paths from subtopic paths and try again`
        );

        journey.allPossiblePaths = undefined;
        saveUserJourney(journey, filePath);
    }

    log(`Wrote user journey path file for ${journey.section}`, {
        addSeperator: true,
    });
};

/**
 *
 * @param {string} outputDir
 * @returns
 */
const createUserJourneyDirectory = (outputDir) => {
    const userJourneyDirectory = path.join(outputDir, "user-journeys");

    try {
        fs.mkdirSync(userJourneyDirectory);
        return userJourneyDirectory;
    } catch (e) {
        console.error(
            `Error creating user journey directory ${userJourneyDirectory}`,
            e
        );
        return outputDir;
    }
};
