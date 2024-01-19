import fs from "fs";

const file = "contentful-export-py5afvqdlxgo-dev-2024-01-17T13-18-05.json";
const fileContents = fs.readFileSync(file, "utf-8");
const asJsonObject = JSON.parse(fileContents);

const entries = asJsonObject.entries;

const sections = new Map();
const questions = new Map();
const answers = new Map();
const recommendations = new Map();

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
    case "recommendation": {
      recommendations.set(id, entry);
      break;
    }
  }
}

for (const [id, question] of questions) {
  question.fields.answers = copyRelationships(question.fields.answers, answers);
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

  console.log(section.fields.questions);
}

function stripLocalisation(obj) {
  return obj["en-US"];
}

function copyRelationships(parent, children) {
  return parent.map((child) => children.get(child.sys.id));
}
