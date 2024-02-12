import { Answer } from "#src/content-types/answer";
import ContentType from "#src/content-types/content-type";
import { Question } from "#src/content-types/question";
import { Recommendation } from "#src/content-types/recommendation";
import { Section } from "#src/content-types/section";

const exportObj = {
  ContentTypes: [Answer, ContentType, Question, Recommendation, Section],
};

export default exportObj;
