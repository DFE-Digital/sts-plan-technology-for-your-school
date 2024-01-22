import fs from "fs";
import { Section } from "./section.mjs";

const file = "contentful-export-py5afvqdlxgo-master-2024-01-22T09-11-46.json";
const fileContents = fs.readFileSync(file, "utf-8");
const asJsonObject = JSON.parse(fileContents);

const entries = asJsonObject.entries;

const sections = new Map();
const questions = new Map();
const answers = new Map();
const recommendations = new Map();
const pages = new Map();

for (const entry of entries) {
  const contentType = entry.sys.contentType.sys.id;
  const id = entry.sys.id;

  const mappedFields = {};
  Object.entries(entry.fields).forEach(([key, value]) => {
    mappedFields[key] = stripLocalisation(value);
  });

  entry.fields = mappedFields;

  switch (contentType) {
    case "answer": {
      answers.set(id, entry);
      break;
    }
    case "question": {
      questions.set(id, entry);
      break;
    }
    case "section": {
      sections.set(id, entry);
      break;
    }
    case "recommendationPage": {
      recommendations.set(id, entry);
      break;
    }
    case "page": {
      pages.set(id, entry);
      break;
    }
  }
}

combineEntries();

const sectionClasses = new Map();

for (const [id, section] of sections) {
  const asClass = new Section(section);

  asClass.writeFile("./output/");
  //var stringified = JSON.stringify(asClass);
  //fs.writeFileSync("./output/" + asClass.name + ".json", stringified);
  //stringified = null;
}

/*
for (const [id, section] of sections) {
  const firstQuestion = section.fields.questions[0];
  const paths = getAllPaths(section.fields.questions, firstQuestion);

  const result = {
    name: section.fields.name,
    id: section.sys.id,
    paths: paths,
  };

  var stringified = JSON.stringify(result);
  fs.writeFileSync(result.name + ".json", stringified);
}

fs.writeFileSync("paths.json", JSON.stringify(sectionPaths));

*/
function combineEntries() {
  for (const [id, recommendation] of recommendations) {
    recommendation.fields.page = pages.get(recommendation.fields.page.sys.id);
  }

  for (const [id, question] of questions) {
    question.fields.answers = copyRelationships(
      question.fields.answers,
      answers
    );
  }

  for (const [id, section] of sections) {
    section.fields.questions = copyRelationships(
      section.fields.questions,
      questions
    );

    section.fields.recommendations = copyRelationships(
      section.fields.recommendations,
      recommendations
    );
  }
}

function stripLocalisation(obj) {
  return obj["en-US"];
}

function copyRelationships(parent, children) {
  return parent.map((child) => children.get(child.sys.id));
}
