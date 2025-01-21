import { Question } from "./question.js";
import { PathCalculator } from "../user-journey/path-calculator.js";
import { SectionStats } from "#src/user-journey/section-stats";
import { UserJourney } from "#src/user-journey/user-journey";

/**
 * @typedef {import("./subtopic-recommendation.js").SubtopicRecommendation} SubtopicRecommendation
 */

/**
 * @typedef {import("#src/user-journey/minimum-path-calculator").PathCountPerMaturity} PathCountPerMaturity
 */

/**
 * @typedef {import("#src/user-journey/user-journey").UserJourneyMinimalOutput} UserJourneyMinimalOutput
 */

/**
 * @typedef {import("#src/user-journey/user-journey").UserJourney} UserJourney
 */

/**
 * Represents a section containing questions and recommendations
 * @class
 */
export class Section {
    /**
     * @type {{fields: Record<string, any>, sys: { id: string}}}
     */
    interstitialPage;

    /**
     * @type {SubtopicRecommendation}
     */
    recommendation;

    /**
     * @type {Question[]}
     */
    questions;

    /**
     * @type {string}
     */
    name;

    /**
     * @type {string}
     */
    id;

    /**
     * @type {PathCalculator}
     */
    pathInfo;

    /**
     * @type {SectionStats}
     */
    stats;

    /**
     * @param {{fields: Record<string, any>, sys: {id: string }}} params
     * @param {SubtopicRecommendation} recommendation - Subtopic recommendation for the seciton
     */
    constructor({ fields, sys }, recommendation) {
        this.interstitialPage = fields.interstitialPage;
        this.questions =
            fields.questions?.map((question) => new Question(question)) ?? [];
        this.id = sys.id;
        this.name = fields.name;
        this.recommendation = recommendation;

        this.pathInfo = new PathCalculator({
            questions: this.questions,
            recommendation,
            sectionId: this.id,
        });

        this.stats = new SectionStats({ paths: this.pathInfo.paths });
    }

    /**
     * Convert section to minimal section info; only information we care for writing to file
     * @param {boolean} writeAllPossiblePaths - Whether to write all possible paths
     * @returns {SectionMinimalOutput} Minimal section info
     */
    toMinimalOutput(writeAllPossiblePaths) {
        const recommendationPaths =
            this.pathInfo.minimumPathsForRecommendations;

        for (const [maturity, path] of Object.entries(recommendationPaths)) {
            recommendationPaths[maturity] = path.map((part) =>
                part.toMinimalOutput()
            );
        }

        const result = {
            section: this.name,
            allPathsStats: this.stats.pathsPerMaturity,
            minimumQuestionPaths:
                this.pathInfo.minimumPathsToNavigateQuestions.map((path) =>
                    path.map((part) => part.toMinimalOutput())
                ),
            minimumRecommendationPaths:
                this.pathInfo.minimumPathsForRecommendations,
            pathsForAnswers: this.pathInfo.pathsForAllPossibleAnswers.map(
                (path) => path.toMinimalOutput()
            ),
            allPossiblePaths: writeAllPossiblePaths
                ? this.pathInfo.paths.map((path) => path.toMinimalOutput())
                : undefined,
        };

        return result;
    }
}

/**
 * @typedef {Object} SectionMinimalOutput
 * @property {string} section
 * @property {allPathStats} PathCountPerMaturity How many paths there are per maturity
 * @property {UserJourney[]} minimumQuestionPaths Shortest amount of paths possible to navigate through every question
 * @property {Record<string, PathPart[]>} minimumRecommendationPaths Shortest amount of paths possible to get every possible recommendation chunk
 * @property {UserJourneyMinimalOutput[]} pathsForAnswers Shortest amount of paths possible to navigate through every answer
 * @property {(UserJourneyMinimalOutput[] | undefined)} allPossiblePaths  All possible paths
 */
