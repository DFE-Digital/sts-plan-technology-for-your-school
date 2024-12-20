import { UserJourney } from "./user-journey.js";
import PathPart from "./path-part.js";
import { PathBuilder } from "./path-builder.js";
import { MinimumPathCalculator } from "./minimum-path-calculator.js";
import { AnswerPathCalculator } from "./answer-path-calculator.js";
import errorLogger from "#src/errors/error-logger";

/** @typedef {import("../content-types/question.js").Question} Question */
/** @typedef {import("../content-types/subtopic-recommendation.js")} SubtopicRecommendation */
/** @typedef {PathPart[]} Path */

/**
 * Calculates and manages different types of paths through a section's questions
 * @class
 */
export class PathCalculator {
    /** @type {Question[]} Questions in the section */
    questions;
    
    /** @type {UserJourney[]} All possible paths through the questions */
    paths;
    
    /** @type {PathBuilder} Builds paths through questions */
    pathBuilder;
    
    /** @type {SubtopicRecommendation} Recommendation for the section */
    recommendation;
    
    /** @type {string} Unique identifier for the section */
    sectionId;

    /** @type {UserJourney[] | undefined} Cached sorted paths */
    _sortedPaths;
    
    /** @type {UserJourney[] | undefined} Cached minimum paths to navigate questions */
    _minimumPathsToNavigateQuestions;
    
    /** @type {Record<string, Path> | undefined} Cached minimum paths for recommendations */
    _minimumPathsForRecommendations;
    
    /** @type {UserJourney[] | undefined} Cached paths for all possible answers */
    _pathsForAllPossibleAnswers;

    /**
     * Creates a new PathCalculator instance
     * @param {Object} params Constructor parameters
     * @param {Question[]} params.questions Questions in the section
     * @param {SubtopicRecommendation} params.recommendation Subtopic recommendation for the section
     * @param {string} params.sectionId Unique identifier for the section
     */
    constructor({ questions, recommendation, sectionId }) {
        this.questions = questions;
        this.recommendation = recommendation;
        this.sectionId = sectionId;

        this.pathBuilder = new PathBuilder(questions);

        this.pathToUserJourney = this.pathToUserJourney.bind(this);
        this.paths = this.pathBuilder
            .getPathsForQuestion(this.questions[0])
            .map(this.pathToUserJourney);

        this.minimumPathCalculator = new MinimumPathCalculator({
            questions,
            paths: this.paths,
            sortedPaths: this.sortedPaths,
            sectionId,
        });

        this.answerPathCalculator = new AnswerPathCalculator(
            questions,
            this.pathBuilder
        );

        this.setNextQuestions();
    }

    /**
     * @type {UserJourney[]} paths sorted by shortest -> longest
     */
    get sortedPaths() {
        if (this._sortedPaths === undefined) {
            this._sortedPaths = this.paths
                .slice()
                .sort((a, b) => b.path.length - a.path.length);
        }

        return this._sortedPaths;
    }

    /**
     * @type {UserJourney[]}
     */
    get minimumPathsToNavigateQuestions() {
        if (this._minimumPathsToNavigateQuestions === undefined) {
            this._minimumPathsToNavigateQuestions =
                this.minimumPathCalculator.calculateMinimumPathsToQuestions();
        }
        return this._minimumPathsToNavigateQuestions;
    }

    /**
     * @type {Record<string, Path>}
     */
    get minimumPathsForRecommendations() {
        if (this._minimumPathsForRecommendations === undefined) {
            this._minimumPathsForRecommendations =
                this.minimumPathCalculator.calculateMinimumPathsForRecommendations();
        }
        return this._minimumPathsForRecommendations;
    }

    /**
     * @public2
     * @type {UserJourney[]}
     */
    get pathsForAllPossibleAnswers() {
        if (this._pathsForAllPossibleAnswers === undefined) {
            this._pathsForAllPossibleAnswers = this.answerPathCalculator
                .getPathsForAllAnswers(this.minimumPathsForRecommendations)
                .map(this.pathToUserJourney);
        }
        return this._pathsForAllPossibleAnswers;
    }

    /**
     * 
     * @param {Path} path 
     * @returns {UserJourney}
     */
    pathToUserJourney(path) {
        const userJourney = new UserJourney(path, this);
        userJourney.setRecommendation(this.recommendation);
        return userJourney;
    }

    /**
     * Checks to see if all recommendation chunks have been included in a path, and if not adds errors to the error logger.
     */
    checkAllChunksTested() {
        const sectionChunks = this.recommendation.section.chunks.map(
            (chunk) => chunk.id
        );

        const uniqueTestedChunks = [
            ...new Set(
                [
                    ...Object.values(this.minimumPathsForRecommendations),
                    ...this.pathsForAllPossibleAnswers.map(
                        (userJourney) => userJourney.path
                    ),
                ]
                    .map((path) =>
                        this.recommendation.section.getChunksForPath(path)
                    )
                    .flat()
                    .map((chunk) => chunk.id)
            ),
        ];

        if (sectionChunks.length !== uniqueTestedChunks.length) {
            sectionChunks
                .filter((chunkId) => !uniqueTestedChunks.includes(chunkId))
                .forEach((missingChunk) => this._addError(`Recommendation chunk ${missingChunk} not included in any test paths.`));
        }
    }

    /**
     * For each answer, in each question (in the this.questions property),
     * find the matching question for the 'nextQuestion' property, and set it if found
     */
    setNextQuestions() {
        for (const question of this.questions) {
            for (const answer of question.answers) {
                const nextQuestionId = answer.nextQuestion?.sys.id;
                if (nextQuestionId == null) {
                    continue;
                }

                const matchingQuestions = this.questions.filter(
                    (q) => q.id == nextQuestionId
                );

                if (matchingQuestions.length > 0) {
                    answer.nextQuestion = matchingQuestions[0];
                    continue;
                }

                this._addError(`Could not find question with ID '${nextQuestionId}' for answer ${answer.text} ${answer.id} in ${question.text} ${question.id}`);
                answer.nextQuestion = null;
            }
        }
    }

    /**
     * Adds message to error logger using the section's id.
     * @param {string} message 
     */
    _addError(message){
        errorLogger.addError({
            id: this.sectionId,
            contentType: "section",
            message: message
        });

    }
}
