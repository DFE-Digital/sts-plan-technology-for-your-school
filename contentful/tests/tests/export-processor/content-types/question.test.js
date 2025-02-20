import { Question } from "../../../../export-processor/content-types/question";

describe("Question Class", () => {
    /** @type {{
     *   maturity: string,
     *   text: string,
     *   helpText: string,
     *   slug: string,
     *   answers: Array<{
     *     fields: {
     *       maturity: string,
     *       text: string,
     *       nextQuestion: string
     *     },
     *     sys: {
     *       id: string
     *     }
     *   }>
     * }} */

    let fields;

    /** @type {{ id: string }} */
    let sys;

    /** @type {Question} */
    let question;

    beforeEach(() => {
        fields = {
            maturity: "Low",
            text: "This is a test question.",
            helpText: "Test question help text",
            slug: "test-question",
            answers: [
                {
                    fields: {
                        maturity: "Low",
                        text: "This is a test answer.",
                        nextQuestion: "next-question-id",
                    },
                    sys: {
                        id: "test-answer",
                    },
                },
                {
                    fields: {
                        maturity: "Medium",
                        text: "This is a different answer.",
                        nextQuestion: "other-next-question-id",
                    },
                    sys: {
                        id: "test-answer-two",
                    },
                },
            ],
        };

        sys = { id: "answer-id" };

        question = new Question({ fields, sys });
    });

    describe("Constructor", () => {
        it("should initialize properties correctly", () => {
            expect(question.id).toBe(sys.id);
            expect(question.text).toBe(fields.text);
            expect(question.helpText).toBe(fields.helpText);
            expect(question.slug).toBe(fields.slug);

            expect(question.answers.length).toEqual(fields.answers.length);
        });
    });

    describe("methods", () => {
        it("answerIds should return all answer ids", () => {
            const answerIds = question.answerIds;

            expect(answerIds.length).toEqual(fields.answers.length);

            for(const answer of fields.answers){
                expect(answerIds).toContain(answer.sys.id);
            }
        });
    });
});
