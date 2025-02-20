import ErrorLogger from "../errors/error-logger.js";
import PathPart from "./path-part.js";
import { UserJourney } from "./user-journey.js";

/**
 * @typedef {Record<string, PathPart[]>} PathCountPerMaturity
 */

/**
 * @typedef {PathPart[]} Path
 */

/**
 * @typedef {import('./types').Question} Question
 */

/**
 * Calculates minimum paths required for routes.
 * @class
 */
export class MinimumPathCalculator {
    /** @type {Question[]} */
    questions;
    /** @type {UserJourney[]} */
    paths;
    /** @type {UserJourney[]} */
    sortedPaths;
    /** @type {string} */
    sectionId;

    /**
     *
     * @param {{ questions: Question[], paths: UserJourney[], sortedPaths: UserJourney[], sectionId: string}} params
     */
    constructor({ questions, paths, sortedPaths, sectionId }) {
        this.questions = questions;
        this.paths = paths;
        this.sortedPaths = sortedPaths;
        this.sectionId = sectionId;
    }

    /**
     * Get unique paths from the sorted paths array.
     *
     * @return {UserJourney[]} The array of unique paths
     */
    getUniquePaths() {
        const seen = new Set();
        return this.sortedPaths.filter((path) => {
            const questionIds = path.questionIdsAnswered.join(",");
            if (seen.has(questionIds)) {
                return false;
            }
            seen.add(questionIds);
            return true;
        });
    }

    /**
     * Calculate the minimum paths to answer all target questions.
     * @returns {Path[]}
     */
    calculateMinimumPathsToQuestions() {
        const remainingQuestions = this.questions.map(
            (question) => question.id
        );
        const minimumPaths = [];

        const uniquePaths = this.getUniquePaths(this.sortedPaths);

        // Find minimum paths to answer all questions
        for (const path of uniquePaths) {
            const matchingQuestions = remainingQuestions.filter((questionId) =>
                path.questionIdsAnswered.some((id) => id == questionId)
            );

            if (matchingQuestions.length == 0) {
                continue;
            }

            minimumPaths.push(path.path);

            this.removeItemsFromArray(
                remainingQuestions,
                path.questionIdsAnswered
            );
        }

        return this.handleRemainingQuestions(remainingQuestions, minimumPaths);
    }

    /**
     *
     * @param {string[]} remainingQuestions
     * @param {Path[]} minimumPaths
     * @returns {Path[]}
     */
    handleRemainingQuestions(remainingQuestions, minimumPaths) {
        for (const questionId of remainingQuestions) {
            const pathContainingQuestion =
                this.getFirstPathContainingQuestion(questionId);

            if (pathContainingQuestion == null) continue;

            minimumPaths.push(pathContainingQuestion.path);
        }
        return minimumPaths;
    }

    /**
     *
     * @param {string} questionId
     * @returns
     */
    getFirstPathContainingQuestion(questionId) {
        // Find the first path that contains the remaining question
        const pathsForQuestion = this.sortedPaths.find((path) =>
            path.path.some((pathPart) => pathPart.question.id == questionId)
        );

        if (pathsForQuestion) {
            return pathsForQuestion;
        }

        const question = this.questions.find(
            (question) => question.id == questionId
        );

        if (!question) {
            ErrorLogger.addError({
                id: this.sectionId,
                contentType: "section",
                message: `Couldn't find question ${questionId} in section ${this.sectionId}`,
            });
        }

        ErrorLogger.addError({
            id: this.sectionId,
            contentType: "section",
            message: `Question ${questionId} does not have a path`,
        });
    }

    /**
     * Calculate the minimum paths for recommendations.
     * @returns {PathCountPerMaturity}
     */
    calculateMinimumPathsForRecommendations() {
        const possibleMaturities = ["Low", "Medium", "High"];
        const minimumPathsForRecommendations = {};

        for (const maturity of possibleMaturities) {
            this._getPathForMaturity(maturity, minimumPathsForRecommendations);
        }

        return minimumPathsForRecommendations;
    }

    /**
     *
     * @param {string} maturity
     * @param {PathCountPerMaturity} minimumPathsForRecommendations
     */
    _getPathForMaturity(maturity, minimumPathsForRecommendations) {
        const pathForRecommendation = this.sortedPaths.find(
            (path) => path.maturity == maturity
        );

        if (!pathForRecommendation) {
            ErrorLogger.addError({
                id: this.sectionId,
                contentType: "section",
                message: `No path exists for ${maturity}`,
            });
            return;
        }

        minimumPathsForRecommendations[maturity] = pathForRecommendation.path;
    }

    /**
     * Removes the item from the array and returns the new array
     * @param {Array} array
     * @param {Array} itemsToRemove
     */
    removeItemsFromArray(array, itemsToRemove) {
        array.splice(
            0,
            array.length,
            ...array.filter((item) => !itemsToRemove.includes(item))
        );
    }
}
