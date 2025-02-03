import { Question } from "../content-types/question.js";
import { Answer } from "../content-types/answer.js";

/**
 * Represents a part of a user journey path, containing a question and its corresponding answer
 */
export default class PathPart {
    /** @type {Question} */
    question;
    /** @type {Answer} */
    answer;

    /**
     * Creates a new PathPart
     * @param {Object} params
     * @param {Question} params.question - The question in this path part
     * @param {Answer} params.answer - The answer selected for this question
     */
    constructor({ question, answer }) {
        this.question = question;
        this.answer = answer;
    }

    toMinimalOutput() {
        return {
            question: this.question.text,
            answer: this.answer.text,
        };
    }
}
