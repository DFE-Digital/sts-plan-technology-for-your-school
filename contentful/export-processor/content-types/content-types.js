import { Answer } from "./answer";
import ContentType from "./content-type";
import { Question } from "./question";
import { Recommendation } from "./recommendation";
import { Section } from "./section";

const exportObj = {
  ContentTypes: [Answer, ContentType, Question, Recommendation, Section],
};

export default exportObj;
