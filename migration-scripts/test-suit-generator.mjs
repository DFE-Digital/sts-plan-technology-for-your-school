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

combineEntries();

const sectionPaths = new Map();

for (const [id, section] of sections) {
  console.log(section.fields.name);

  const firstQuestion = section.fields.questions[0];
  const paths = getAllPaths(
    section.fields.questions,
    [],
    firstQuestion,
    section.fields.name
  );
  const result = {
    name: section.fields.name,
    id: section.sys.id,
    paths: paths,
  };

  console.log(result.paths);

  var stringified = JSON.stringify(result);
  fs.writeFileSync(result.name + ".json", stringified);
}

fs.writeFileSync("paths.json", JSON.stringify(sectionPaths));
function getAllPaths(questions, currentPath, currentQuestion, section) {
  const paths = [];

  if (!currentQuestion) {
    if (section.indexOf("connection") > -1) {
      console.log("no current question", currentPath, paths);
    }
    return paths;
  }

  currentQuestion.fields.answers.forEach((answer) => {
    const newPath = [
      ...currentPath,
      { question: currentQuestion.fields.text, answer: answer.fields.text },
    ];

    const nextQuestion = questions.find(
      (q) => q.sys.id === answer.fields.nextQuestion?.sys.id
    );

    const nextPaths = getAllPaths(questions, newPath, nextQuestion, section);
    paths.push(...nextPaths);
  });

  if (section.indexOf("connection") > -1) {
    console.log(paths.length);
  }

  return paths.length ? paths : [currentPath];
}

function combineEntries() {
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

function onlyUnique(value, index, array) {
  return array.indexOf(value) === index;
}
